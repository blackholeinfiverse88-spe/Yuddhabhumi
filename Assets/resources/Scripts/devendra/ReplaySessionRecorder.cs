using System;
using UnityEngine;

public class ReplaySessionRecorder : MonoBehaviour
{
    public static ReplaySessionRecorder Instance { get; private set; }

    public bool autoBeginSession = true;

    public string ActiveSessionId { get; private set; }
    public string ActiveTraceId { get; private set; }

    private int _eventIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BeginNewSession(string traceId)
    {
        ActiveSessionId = Guid.NewGuid().ToString("N");
        ActiveTraceId = string.IsNullOrEmpty(traceId) ? "trace_unknown" : traceId;
        _eventIndex = 0;

        var header = new ReplaySessionHeader
        {
            session_id = ActiveSessionId,
            trace_id = ActiveTraceId,
            replay_version = ReplayVersions.ReplayVersion,
            schema_version = ReplayVersions.TraceSchemaVersion,
            interpretation_version = ReplayVersions.InterpretationVersion,
            consequence_version = ReplayVersions.ConsequenceVersion,
            unity_version = Application.unityVersion,
            created_utc = DateTime.UtcNow.ToString("o"),
            note = "append-only jsonl"
        };

        ReplayPersistence.SaveHeader(header);

        Debug.Log($"[REPLAY] Session started id={ActiveSessionId} dir={ReplayPersistence.GetSessionDir(ActiveSessionId)}");
    }

    public void EndSession()
    {
        Debug.Log($"[REPLAY] Session ended id={ActiveSessionId} events={_eventIndex}");
        ActiveSessionId = null;
        ActiveTraceId = null;
        _eventIndex = 0;
    }

    public void RecordEvent(
        KarmaStateTracker.GameplayValidationRecord rec,
        BehaviorTrace trace,
        KarmaDerivationEngine.KarmaOutput karma,
        CombatConsequenceEngine.CombatConsequence consequence)
    {
        if (string.IsNullOrEmpty(ActiveSessionId))
        {
            if (!autoBeginSession) return;

            string traceId = (BehaviorTraceRecorder.Instance != null) ? BehaviorTraceRecorder.Instance.GetTraceId() : "trace_unknown";
            BeginNewSession(traceId);
        }

        var ev = new ReplayEventRecord
        {
            session_id = ActiveSessionId,
            trace_id = ActiveTraceId,

            replay_version = ReplayVersions.ReplayVersion,
            schema_version = ReplayVersions.TraceSchemaVersion,
            interpretation_version = ReplayVersions.InterpretationVersion,
            consequence_version = ReplayVersions.ConsequenceVersion,

            event_index = _eventIndex++,

            turn_id = rec.turn_id,
            frame = rec.frame,
            match_time_sec = rec.match_time_sec,

            action_type = "deploy",
            action_card = rec.action_card,
            action_cost01 = rec.action_cost01,

            trace = trace,
            karma = karma,
            consequence = consequence
        };

        ReplayPersistence.AppendEvent(ActiveSessionId, ev);
    }
}