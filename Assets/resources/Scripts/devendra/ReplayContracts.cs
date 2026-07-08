using UnityEngine;

public static class ReplayVersions
{
    public const string ReplayVersion = "replay_v2";
    public const string TraceSchemaVersion = "behavior_trace_v_sprint1";
    public const string InterpretationVersion = "karma_interpretation_v_sprint1";
    public const string ConsequenceVersion = "combat_consequence_v1";
}

[System.Serializable]
public struct ReplaySessionHeader
{
    public string session_id;
    public string trace_id;

    public string replay_version;
    public string schema_version;
    public string interpretation_version;
    public string consequence_version;

    public string unity_version;
    public string created_utc;

    public string note;
}

[System.Serializable]
public struct ReplayEventRecord
{
    public string session_id;
    public string trace_id;

    public string replay_version;
    public string schema_version;
    public string interpretation_version;
    public string consequence_version;

    public int event_index;

    public int turn_id;
    public int frame;
    public float match_time_sec;

    public string action_type;   // "deploy"
    public string action_card;
    public float action_cost01;

    public BehaviorTrace trace;
    public KarmaDerivationEngine.KarmaOutput karma;

    // Phase 4: deterministic combat-state consequence intelligence (replaces primitive damage flag)
    public CombatConsequenceEngine.CombatConsequence consequence;
}