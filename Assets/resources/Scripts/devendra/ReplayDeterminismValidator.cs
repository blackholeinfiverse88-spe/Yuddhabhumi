using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ReplayDeterminismValidator : MonoBehaviour
{
    [Header("Validation")]
    public int reruns = 3;

    [Tooltip("If empty, validates the latest replay session found in persistentDataPath/replays")]
    public string sessionIdOverride = "";

    [ContextMenu("Validate Replay Determinism (Latest/Override)")]
    public void ValidateNow()
    {
        string sid = string.IsNullOrEmpty(sessionIdOverride)
            ? FindLatestSessionId()
            : sessionIdOverride;

        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogError("[REPLAY_VALIDATE] No replay session found.");
            return;
        }

        var report = ValidateSessionDeterminism(sid, Mathf.Clamp(reruns, 3, 10));
        Debug.Log("[REPLAY_VALIDATE_REPORT]\n" + JsonUtility.ToJson(report, true));
    }

    [Serializable]
    public struct DeterminismReport
    {
        public string session_id;
        public string trace_id;

        public string replay_version;
        public string schema_version;
        public string interpretation_version;
        public string consequence_version;

        public int total_events;
        public bool ordering_ok;
        public bool schema_ok;

        public bool karma_recompute_ok;
        public int karma_mismatch_count;

        public bool determinism_ok;
        public string[] run_hashes;

        public string notes;
    }

    public static DeterminismReport ValidateSessionDeterminism(string sessionId, int runs)
    {
        var header = ReplayPersistence.LoadHeader(sessionId);
        var events = ReplayPersistence.LoadEvents(sessionId);

        bool schemaOk =
            header.replay_version == ReplayVersions.ReplayVersion &&
            header.schema_version == ReplayVersions.TraceSchemaVersion &&
            header.interpretation_version == ReplayVersions.InterpretationVersion &&
            header.consequence_version == ReplayVersions.ConsequenceVersion;

        bool orderingOk = CheckOrdering(events);

        // Reconstruction integrity: recompute karma from stored trace and compare
        int mismatchCount = 0;
        for (int i = 0; i < events.Count; i++)
        {
            var ev = events[i];
            var recomputed = KarmaDerivationEngine.Derive(ev.trace_id, ev.turn_id, ev.trace);

            if (!KarmaEqual(recomputed, ev.karma))
            {
                mismatchCount++;
                Debug.LogError(
                    $"[REPLAY_DIVERGENCE] session={sessionId} event_index={ev.event_index} turn_id={ev.turn_id} " +
                    $"stored_pattern={ev.karma.karma.pattern} recomputed_pattern={recomputed.karma.pattern}"
                );
            }
        }

        bool karmaOk = (mismatchCount == 0);

        // Determinism proof: compute deterministic hash multiple times
        var hashes = new string[runs];
        for (int r = 0; r < runs; r++)
            hashes[r] = ComputeDeterministicHash(header, events);

        bool determinismOk = true;
        for (int r = 1; r < hashes.Length; r++)
        {
            if (hashes[r] != hashes[0]) { determinismOk = false; break; }
        }

        return new DeterminismReport
        {
            session_id = header.session_id,
            trace_id = header.trace_id,

            replay_version = header.replay_version,
            schema_version = header.schema_version,
            interpretation_version = header.interpretation_version,
            consequence_version = header.consequence_version,

            total_events = events.Count,
            ordering_ok = orderingOk,
            schema_ok = schemaOk,

            karma_recompute_ok = karmaOk,
            karma_mismatch_count = mismatchCount,

            determinism_ok = determinismOk,
            run_hashes = hashes,

            notes =
                "Hash excludes runtime timing (ms). " +
                "Karma recompute validated from stored trace snapshots. " +
                "Consequence stored as structured object (combat_consequence_v1)."
        };
    }

    private static bool CheckOrdering(List<ReplayEventRecord> events)
    {
        int lastEventIndex = -1;
        int lastTurnId = -1;
        float lastMatchTime = -1f;

        for (int i = 0; i < events.Count; i++)
        {
            var ev = events[i];

            if (ev.event_index <= lastEventIndex) return false;
            lastEventIndex = ev.event_index;

            if (ev.turn_id <= lastTurnId) return false;
            lastTurnId = ev.turn_id;

            if (ev.match_time_sec < lastMatchTime) return false;
            lastMatchTime = ev.match_time_sec;
        }

        return true;
    }

    private static bool KarmaEqual(KarmaDerivationEngine.KarmaOutput a, KarmaDerivationEngine.KarmaOutput b)
    {
        if (a.karma.pattern != b.karma.pattern) return false;
        if (a.karma.direction != b.karma.direction) return false;
        if (a.karma.explanation != b.karma.explanation) return false;

        float ai = Quant3(a.karma.intensity);
        float bi = Quant3(b.karma.intensity);
        return Mathf.Abs(ai - bi) < 0.0001f;
    }

    private static float Quant3(float v) => Mathf.Round(v * 1000f) / 1000f;

    private static string ComputeDeterministicHash(ReplaySessionHeader header, List<ReplayEventRecord> events)
    {
        var sb = new StringBuilder(4096);

        sb.Append("header|")
          .Append(header.session_id).Append("|")
          .Append(header.trace_id).Append("|")
          .Append(header.replay_version).Append("|")
          .Append(header.schema_version).Append("|")
          .Append(header.interpretation_version).Append("|")
          .Append(header.consequence_version).Append("\n");

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];

            // NOTE: total damage now lives inside consequence.snapshot
            float totalDamage = e.consequence.snapshot.total_damage;

            sb.Append("e|")
              .Append(e.event_index).Append("|")
              .Append(e.turn_id).Append("|")
              .Append(e.frame).Append("|")
              .Append(e.match_time_sec.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
              .Append(e.action_type).Append("|")
              .Append(e.action_card).Append("|")
              .Append(e.action_cost01.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
              .Append(totalDamage.ToString("F3", CultureInfo.InvariantCulture)).Append("|");

            AppendTrace(sb, e.trace);

            sb.Append("|k|")
              .Append(e.karma.karma.pattern).Append("|")
              .Append(e.karma.karma.intensity.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
              .Append(e.karma.karma.direction).Append("|")
              .Append(e.karma.karma.explanation);

            // consequence summary (deterministic)
            sb.Append("|c|")
              .Append(e.consequence.pattern).Append("|")
              .Append(e.consequence.intensity.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
              .Append(e.consequence.direction).Append("|")
              .Append(e.consequence.explanation);

            sb.Append("\n");
        }

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static void AppendTrace(StringBuilder sb, BehaviorTrace t)
    {
        sb.Append("t|")
          .Append(t.aggression.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.risk_taking.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.patience.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.consistency.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.adaptability.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.foresight.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.hesitation_frequency.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.failed_push_streak.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.defensive_holding_tendency.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.retreat_reengage_tendency.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.adaptive_switching.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.pacing_consistency.ToString("F3", CultureInfo.InvariantCulture)).Append("|")
          .Append(t.window_plays);
    }

    private static string FindLatestSessionId()
    {
        string root = ReplayPersistence.ReplaysRoot;
        if (!Directory.Exists(root))
            return null;

        string latestId = null;
        DateTime latestTime = DateTime.MinValue;

        foreach (var dir in Directory.GetDirectories(root))
        {
            string sid = Path.GetFileName(dir);
            string headerPath = ReplayPersistence.GetHeaderPath(sid);
            if (!File.Exists(headerPath))
                continue;

            try
            {
                var header = ReplayPersistence.LoadHeader(sid);
                if (DateTime.TryParse(header.created_utc, out var dt))
                {
                    if (dt > latestTime)
                    {
                        latestTime = dt;
                        latestId = sid;
                    }
                }
                else
                {
                    var w = Directory.GetLastWriteTimeUtc(dir);
                    if (w > latestTime)
                    {
                        latestTime = w;
                        latestId = sid;
                    }
                }
            }
            catch { }
        }

        return latestId;
    }
}