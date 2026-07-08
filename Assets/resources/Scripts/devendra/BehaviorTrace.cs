using UnityEngine;

[System.Serializable]
public struct BehaviorTrace
{
    // Core metrics (0..1)
    public float aggression;
    public float risk_taking;
    public float patience;
    public float consistency;
    public float adaptability;
    public float foresight;

    // Sprint-1 expanded deterministic signals (0..1, quantized)
    public float hesitation_frequency;         // waited until high energy before deploying
    public float failed_push_streak;           // repeated aggressive deploys with no payoff
    public float defensive_holding_tendency;   // low-cost deploys while holding high energy
    public float retreat_reengage_tendency;    // fail -> retreat -> re-engage pattern
    public float adaptive_switching;           // cost-band switching rate
    public float pacing_consistency;           // stability of energy_before across deploys

    // Debug
    public int window_plays;
}