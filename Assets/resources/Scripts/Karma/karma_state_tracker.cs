using UnityEngine;

public class KarmaStateTracker : MonoBehaviour
{
    public static KarmaStateTracker Instance { get; private set; }

    [Header("Behavior Trace Source (optional). If null, uses BehaviorTraceRecorder.Instance.")]
    [SerializeField] private MonoBehaviour behaviorTraceSource; // must implement IBehaviorTraceSource

    private IBehaviorTraceSource _source;
    private int _turnId = 0;

    private KarmaDerivationEngine.KarmaOutput _lastOutput;
    private bool _hasOutput;
    private string _lastOutputJson;

    public bool HasOutput => _hasOutput;
    public string LastOutputJson => _lastOutputJson;

    [System.Serializable]
    public struct GameplayValidationRecord
    {
        public int turn_id;
        public int frame;
        public float match_time_sec;

        public string action_card;
        public float action_cost01;

        public float damage_delta_flag01;
        public float total_damage;

        public BehaviorTrace trace;
        public KarmaDerivationEngine.KarmaOutput karma;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BindSource();
    }

    private void BindSource()
    {
        _source = behaviorTraceSource as IBehaviorTraceSource;

        if (_source == null && BehaviorTraceRecorder.Instance != null)
            _source = BehaviorTraceRecorder.Instance;

        if (behaviorTraceSource != null && _source == null)
            Debug.LogError("[KARMA] behaviorTraceSource does not implement IBehaviorTraceSource.");

        if (_source == null)
            Debug.LogWarning("[KARMA] No behavior trace source available (ensure BehaviorTraceRecorder exists in Boot).");
    }

    // Compatibility (GameManagerVR etc.)
    public void ResetMatchKarma() => ResetForNewMatch();

    public void ResetForNewMatch()
    {
        _turnId = 0;
        _hasOutput = false;
        _lastOutput = default;
        _lastOutputJson = null;
        Debug.Log("[KARMA] ResetForNewMatch completed.");
    }

    /// <summary>
    /// COMPATIBILITY: Used by GameFlowManager or Result systems.
    /// Returns a stable final label without adding any scoring.
    /// </summary>
    public string GetFinalKarmaState()
    {
        if (!_hasOutput) return "Unknown";
        // returning pattern is consistent with new explanation-first system
        return _lastOutput.karma.pattern;
    }

    public void TriggerDerivation()
    {
        if (_source == null)
            BindSource();

        if (_source == null)
        {
            Debug.LogError("[KARMA] TriggerDerivation failed: missing behavior trace source.");
            return;
        }

        _turnId += 1;

        string traceId = _source.GetTraceId();
        BehaviorTrace trace = _source.GetCurrentTrace();

        Debug.Log("[BEHAVIOR_TRACE] " + JsonUtility.ToJson(trace));

        _lastOutput = KarmaDerivationEngine.Derive(traceId, _turnId, trace);
        _hasOutput = true;

        _lastOutputJson = JsonUtility.ToJson(_lastOutput, true);
        Debug.Log("[KARMA_JSON]\n" + _lastOutputJson);

        Debug.Log(
            $"[KARMA_OUTPUT] trace_id={_lastOutput.trace_id} turn_id={_lastOutput.turn_id} " +
            $"pattern={_lastOutput.karma.pattern} intensity={_lastOutput.karma.intensity:F2} " +
            $"direction={_lastOutput.karma.direction} explanation={_lastOutput.karma.explanation}"
        );

        float mt = (GameFlowManager.Instance != null) ? Mathf.Round(GameFlowManager.Instance.matchDuration) : 0f;

        var btr = BehaviorTraceRecorder.Instance;
        var rec = new GameplayValidationRecord
        {
            turn_id = _turnId,
            frame = Time.frameCount,
            match_time_sec = mt,
            action_card = (btr != null) ? btr.LastCardName : "",
            action_cost01 = (btr != null) ? btr.LastCardCost01 : 0f,
            damage_delta_flag01 = (btr != null) ? btr.LastDamageDeltaFlag01 : 0f,
            total_damage = (btr != null) ? btr.LastDamageTotal : 0f,
            trace = trace,
            karma = _lastOutput
        };

        Debug.Log("[VALIDATION_JSON]\n" + JsonUtility.ToJson(rec, true));
    }

    public void RecomputeFromBehaviorTrace() => TriggerDerivation();

    public bool TryGetLastKarmaOutput(out KarmaDerivationEngine.KarmaOutput output)
    {
        output = _lastOutput;
        return _hasOutput;
    }
}