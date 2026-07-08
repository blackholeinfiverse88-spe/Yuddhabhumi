using UnityEngine;

public class BehaviorTraceRecorder : MonoBehaviour, IBehaviorTraceSource
{
    public static BehaviorTraceRecorder Instance { get; private set; }

    [Header("Identity")]
    [SerializeField] private string traceId = "trace_local";

    [Header("Window (multi-turn)")]
    [SerializeField] private int windowSize = 16;

    [Header("Normalization")]
    [SerializeField] private float maxExpectedCardCost = 10f;

    [Header("Cost thresholds (normalized 0..1)")]
    [Range(0f, 1f)] [SerializeField] private float highCostThreshold01 = 0.75f;
    [Range(0f, 1f)] [SerializeField] private float lowCostThreshold01 = 0.35f;

    [Header("Signal thresholds (energy_before normalized 0..1)")]
    [Range(0f, 1f)] [SerializeField] private float hesitationEnergyBefore01 = 0.85f;
    [Range(0f, 1f)] [SerializeField] private float defensiveHoldEnergyBefore01 = 0.70f;

    private struct DeployEvent
    {
        public float cost01;
        public float energyBefore01;
        public float energyAfter01;
        public byte costBand;        // 0=low,1=mid,2=high
        public string cardName;
        public float damageDeltaFlag01; // 0 or 1 (quantized)
    }

    private DeployEvent[] _events;
    private int _count;
    private int _head;

    private BehaviorTrace _current;

    // Multi-turn state
    private bool _prevWasHigh;
    private float _prevDamageTotal;
    private bool _hasPrevDamageTotal;

    private int _failedPushStreak;
    private bool _expectRetreat;
    private bool _didRetreat;
    private int _retreatReengageCount;

    // Exposed for Phase 3 validation logs
    public string LastCardName { get; private set; } = "";
    public float LastCardCost01 { get; private set; } = 0f;

    // IMPORTANT: this is the exact property your KarmaStateTracker expects
    public float LastDamageDeltaFlag01 { get; private set; } = 0f;

    public float LastDamageTotal { get; private set; } = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        windowSize = Mathf.Clamp(windowSize, 4, 64);
        _events = new DeployEvent[windowSize];
    }

    public string GetTraceId() => traceId;
    public BehaviorTrace GetCurrentTrace() => _current;

    // -------------------------
    // Overloads (to fix your compile mismatch)
    // -------------------------

    // If some old caller only passes card
    public void RecordSuccessfulDeploy(UnitData card)
    {
        // deterministic fallback values (no energy/damage context)
        RecordSuccessfulDeploy(card, 0f, 0f, 10f, 0f);
    }

    // If some caller passes (card, energyBefore, energyAfter, maxEnergy)
    public void RecordSuccessfulDeploy(UnitData card, float energyBefore, float energyAfter, float maxEnergy)
    {
        float dmgTotal = (GameFlowManager.Instance != null) ? GameFlowManager.Instance.totalDamageDealt : 0f;
        RecordSuccessfulDeploy(card, energyBefore, energyAfter, maxEnergy, dmgTotal);
    }

    /// <summary>
    /// Preferred call: card + energyBefore/after + maxEnergy + totalDamageDealt.
    /// Call ONLY after successful deploy (energy spent).
    /// </summary>
    public void RecordSuccessfulDeploy(UnitData card, float energyBefore, float energyAfter, float maxEnergy, float totalDamageDealt)
    {
        if (card == null) return;

        float cost01 = Q01(card.cost / Mathf.Max(0.0001f, maxExpectedCardCost));
        float denomE = Mathf.Max(0.0001f, maxEnergy);
        float eBefore01 = Q01(energyBefore / denomE);
        float eAfter01 = Q01(energyAfter / denomE);

        byte band = 1;
        if (cost01 >= highCostThreshold01) band = 2;
        else if (cost01 <= lowCostThreshold01) band = 0;

        // consequence proxy (0/1): did total damage increase since last deploy?
        float dmgDelta = 0f;
        if (_hasPrevDamageTotal)
            dmgDelta = totalDamageDealt - _prevDamageTotal;

        _prevDamageTotal = totalDamageDealt;
        _hasPrevDamageTotal = true;

        float dmgFlag01 = (dmgDelta > 0.0001f) ? 1f : 0f;
        dmgFlag01 = Q01(dmgFlag01);

        // multi-turn pattern state
        if (_prevWasHigh)
        {
            if (dmgFlag01 <= 0f) _failedPushStreak++;
            else _failedPushStreak = 0;

            if (dmgFlag01 <= 0f) _expectRetreat = true;
        }

        if (_expectRetreat && band != 2) _didRetreat = true;
        if (_didRetreat && band == 2)
        {
            _retreatReengageCount++;
            _expectRetreat = false;
            _didRetreat = false;
        }

        _prevWasHigh = (band == 2);

        var ev = new DeployEvent
        {
            cost01 = cost01,
            energyBefore01 = eBefore01,
            energyAfter01 = eAfter01,
            costBand = band,
            cardName = card.unitName,
            damageDeltaFlag01 = dmgFlag01
        };

        _events[_head] = ev;
        _head = (_head + 1) % _events.Length;
        _count = Mathf.Min(_count + 1, _events.Length);

        LastCardName = card.unitName;
        LastCardCost01 = cost01;
        LastDamageDeltaFlag01 = dmgFlag01;
        LastDamageTotal = totalDamageDealt;

        RecomputeTraceFromWindow();
    }

    private void RecomputeTraceFromWindow()
    {
        if (_count <= 0)
        {
            _current = default;
            return;
        }

        float sumCost = 0f, sumCostSq = 0f;
        float sumEBefore = 0f, sumEBeforeSq = 0f;
        float sumEAfter = 0f;

        int high = 0, low = 0;
        int hesitation = 0;
        int defensiveHold = 0;

        int switches = 0;
        byte prevBand = 255;

        int uniqueCount = 0;

        for (int i = 0; i < _count; i++)
        {
            var e = GetEventFromNewest(i);

            sumCost += e.cost01;
            sumCostSq += e.cost01 * e.cost01;

            sumEBefore += e.energyBefore01;
            sumEBeforeSq += e.energyBefore01 * e.energyBefore01;

            sumEAfter += e.energyAfter01;

            if (e.costBand == 2) high++;
            if (e.costBand == 0) low++;

            if (e.energyBefore01 >= hesitationEnergyBefore01) hesitation++;
            if (e.costBand == 0 && e.energyBefore01 >= defensiveHoldEnergyBefore01) defensiveHold++;

            if (prevBand != 255 && e.costBand != prevBand) switches++;
            prevBand = e.costBand;

            bool seen = false;
            for (int j = 0; j < i; j++)
            {
                if (GetEventFromNewest(j).cardName == e.cardName)
                {
                    seen = true;
                    break;
                }
            }
            if (!seen) uniqueCount++;
        }

        float plays = _count;

        float avgCost = sumCost / plays;
        float costVar = Mathf.Max(0f, (sumCostSq / plays) - (avgCost * avgCost));
        float costStd = Mathf.Sqrt(costVar);

        float avgEBefore = sumEBefore / plays;
        float eVar = Mathf.Max(0f, (sumEBeforeSq / plays) - (avgEBefore * avgEBefore));
        float eStd = Mathf.Sqrt(eVar);

        float avgEAfter = sumEAfter / plays;

        float consistency01 = Mathf.Clamp01(1f - (costStd / 0.5f));
        float pacingConsistency01 = Mathf.Clamp01(1f - (eStd / 0.5f));

        float highRatio = high / plays;
        float lowRatio = low / plays;

        float switchRate = (plays > 1f) ? (switches / (plays - 1f)) : 0f;

        float adaptability01 = Mathf.Clamp01(uniqueCount / plays);
        float foresight01 = Mathf.Clamp01(avgEAfter);

        _current = new BehaviorTrace
        {
            aggression = Q01(avgCost),
            risk_taking = Q01(highRatio),
            patience = Q01(lowRatio),
            consistency = Q01(consistency01),
            adaptability = Q01(adaptability01),
            foresight = Q01(foresight01),

            hesitation_frequency = Q01(hesitation / plays),
            failed_push_streak = Q01(Mathf.Clamp01(_failedPushStreak / 3f)),
            defensive_holding_tendency = Q01(defensiveHold / plays),
            retreat_reengage_tendency = Q01(Mathf.Clamp01(_retreatReengageCount / 3f)),
            adaptive_switching = Q01(switchRate),
            pacing_consistency = Q01(pacingConsistency01),

            window_plays = _count
        };
    }

    private DeployEvent GetEventFromNewest(int i)
    {
        int idx = _head - 1 - i;
        while (idx < 0) idx += _events.Length;
        idx %= _events.Length;
        return _events[idx];
    }

    private static float Q01(float v)
    {
        v = Mathf.Clamp01(v);
        return Mathf.Round(v * 1000f) / 1000f;
    }
}