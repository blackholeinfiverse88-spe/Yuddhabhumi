using UnityEngine;

/// <summary>
/// Deterministic, rule-based karma derivation.
/// Input: BehaviorTrace (from Roshan or Mock)
/// Output: karma_output schema (explanation layer only)
/// IMPORTANT: No randomness, no event-driven accumulation, no scoring/rewards.
/// </summary>
public static class KarmaDerivationEngine
{
    [System.Serializable]
    public struct KarmaOutput
    {
        public string trace_id;
        public int turn_id;

        public Karma karma;

        [System.Serializable]
        public struct Karma
        {
            public string pattern;       // classification label
            public float intensity;      // 0..1 strength of pattern (NOT a score/reward)
            public string direction;     // "risky" / "stable" / "limited" / "neutral"
            public string explanation;   // human-readable explanation
        }
    }

    /// <summary>
    /// Core derivation entry. Fixed rule order => deterministic.
    /// </summary>
    public static KarmaOutput Derive(string traceId, int turnId, BehaviorTrace t)
    {
        t = Clamp01(t);

        // RULE 1 (spec example)
        if (t.aggression > 0.8f && t.risk_taking > 0.7f)
        {
            return Build(
                traceId, turnId,
                pattern: "impulsive_force",
                intensity: Avg(t.aggression, t.risk_taking),
                direction: "risky",
                explanation: "High aggression with high risk-taking created vulnerable over-commitment."
            );
        }

        // RULE 2 (spec example)
        if (t.patience > 0.7f && t.consistency > 0.7f)
        {
            return Build(
                traceId, turnId,
                pattern: "disciplined_control",
                intensity: Avg(t.patience, t.consistency),
                direction: "stable",
                explanation: "High patience and consistency produced controlled, reliable decisions."
            );
        }

        // RULE 3 (spec example)
        if (t.adaptability < 0.3f)
        {
            return Build(
                traceId, turnId,
                pattern: "rigidity",
                intensity: 1f - t.adaptability,
                direction: "limited",
                explanation: "Low adaptability suggests repeated patterns and difficulty responding to change."
            );
        }

        // OPTIONAL deterministic rule (still explanation-only)
        if (t.aggression > 0.7f && t.foresight < 0.4f)
        {
            return Build(
                traceId, turnId,
                pattern: "reckless_pressure",
                intensity: Avg(t.aggression, 1f - t.foresight),
                direction: "risky",
                explanation: "High aggression with low foresight increased exposure to counter-play."
            );
        }

        // DEFAULT
        return Build(
            traceId, turnId,
            pattern: "balanced_response",
            intensity: 0.5f,
            direction: "neutral",
            explanation: "Behavior signals were balanced without a dominant extreme."
        );
    }

    private static KarmaOutput Build(string traceId, int turnId, string pattern, float intensity, string direction, string explanation)
    {
        return new KarmaOutput
        {
            trace_id = traceId,
            turn_id = turnId,
            karma = new KarmaOutput.Karma
            {
                pattern = pattern,
                intensity = Mathf.Clamp01(intensity),
                direction = direction,
                explanation = explanation
            }
        };
    }

    private static float Avg(float a, float b) => (a + b) * 0.5f;

    private static BehaviorTrace Clamp01(BehaviorTrace t)
    {
        t.aggression = Mathf.Clamp01(t.aggression);
        t.risk_taking = Mathf.Clamp01(t.risk_taking);
        t.patience = Mathf.Clamp01(t.patience);
        t.consistency = Mathf.Clamp01(t.consistency);
        t.adaptability = Mathf.Clamp01(t.adaptability);
        t.foresight = Mathf.Clamp01(t.foresight);
        return t;
    }
}