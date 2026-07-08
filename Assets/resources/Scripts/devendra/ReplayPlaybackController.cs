using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class ReplayPlaybackController : MonoBehaviour
{
    [Header("Session Selection")]
    [Tooltip("If empty, loads latest session from persistentDataPath/replays")]
    public string sessionIdOverride = "";

    [Header("Playback")]
    public bool autoplay = true;
    public float playbackSpeed = 1f;

    [Tooltip("Deterministic playback clock (fixed step).")]
    public bool deterministicClock = true;

    public float fixedStepSeconds = 1f / 30f;

    [Header("UI (Optional)")]
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI eventText;

    private ReplaySessionHeader _header;
    private List<ReplayEventRecord> _events;
    private int _nextIndex;

    private float _playbackTime;
    private int _tick;
    private bool _loaded;

    private void Start()
    {
        LoadLatestOrOverride();
    }

    [ContextMenu("Load Latest/Override Replay Session")]
    public void LoadLatestOrOverride()
    {
        string sid = string.IsNullOrEmpty(sessionIdOverride)
            ? FindLatestSessionId()
            : sessionIdOverride;

        if (string.IsNullOrEmpty(sid))
        {
            Debug.LogError("[REPLAY_PLAYBACK] No replay session found. (Did Phase 1 record any session?)");
            return;
        }

        _header = ReplayPersistence.LoadHeader(sid);
        _events = ReplayPersistence.LoadEvents(sid);

        _nextIndex = 0;
        _playbackTime = 0f;
        _tick = 0;
        _loaded = true;

        Debug.Log($"[REPLAY_PLAYBACK] Loaded session id={_header.session_id} events={_events.Count}");

        if (headerText != null)
        {
            headerText.text =
                $"Replay Session\n" +
                $"session_id: {_header.session_id}\n" +
                $"trace_id: {_header.trace_id}\n" +
                $"replay_version: {_header.replay_version}\n" +
                $"schema_version: {_header.schema_version}\n" +
                $"interpretation_version: {_header.interpretation_version}\n" +
                $"consequence_version: {_header.consequence_version}\n";
        }
    }

    private void Update()
    {
        if (!_loaded || _events == null || _events.Count == 0) return;
        if (!autoplay) return;

        float dt = deterministicClock ? fixedStepSeconds : Time.unscaledDeltaTime;
        _tick++;
        _playbackTime += dt * Mathf.Max(0.01f, playbackSpeed);

        while (_nextIndex < _events.Count && _events[_nextIndex].match_time_sec <= _playbackTime + 0.0001f)
        {
            EmitSyncEvidence(_events[_nextIndex]);
            _nextIndex++;
        }
    }

    private void EmitSyncEvidence(ReplayEventRecord ev)
    {
        if (eventText != null)
        {
            eventText.text =
                $"T(stored): {ev.match_time_sec:0.000}s | T(play): {_playbackTime:0.000}s\n" +
                $"frame(stored): {ev.frame} | tick(play): {_tick}\n" +
                $"action: {ev.action_type} {ev.action_card} cost01={ev.action_cost01:0.000}\n" +
                $"karma: {ev.karma.karma.pattern} ({ev.karma.karma.direction})\n" +
                $"{ev.karma.karma.explanation}\n\n" +
                $"consequence: {ev.consequence.pattern} ({ev.consequence.direction})\n" +
                $"{ev.consequence.explanation}\n";
        }

        Debug.Log(
            "[REPLAY_SYNC] " +
            $"session={_header.session_id} idx={ev.event_index} turn={ev.turn_id} " +
            $"t_stored={ev.match_time_sec:0.000} t_play={_playbackTime:0.000} " +
            $"frame_stored={ev.frame} tick_play={_tick} " +
            $"card={ev.action_card} karma={ev.karma.karma.pattern} consequence={ev.consequence.pattern}"
        );
    }

    private string FindLatestSessionId()
    {
        string root = ReplayPersistence.ReplaysRoot;
        if (!Directory.Exists(root)) return null;

        string latestId = null;
        DateTime latestTime = DateTime.MinValue;

        foreach (var dir in Directory.GetDirectories(root))
        {
            string sid = Path.GetFileName(dir);
            string headerPath = ReplayPersistence.GetHeaderPath(sid);
            if (!File.Exists(headerPath)) continue;

            try
            {
                var h = ReplayPersistence.LoadHeader(sid);
                if (DateTime.TryParse(h.created_utc, out var dt))
                {
                    if (dt > latestTime) { latestTime = dt; latestId = sid; }
                }
                else
                {
                    var w = Directory.GetLastWriteTimeUtc(dir);
                    if (w > latestTime) { latestTime = w; latestId = sid; }
                }
            }
            catch { }
        }

        return latestId;
    }
}