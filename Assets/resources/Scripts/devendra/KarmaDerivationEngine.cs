using UnityEngine;

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
            public string pattern;
            public float intensity;     // 0..1
            public string direction;    // self-harm / neutral / strategic_gain
            public string explanation;
        }
    }

    public static KarmaOutput Derive(string traceId, int turnId, BehaviorTrace t)
    {
        t = Clamp01(t);

        // -----------------------------
        // Multi-turn priority patterns (Sprint 1)
        // -----------------------------
        // Repeated aggressive failures -> stubborn escalation
        if (t.failed_push_streak > 0.66f && t.aggression > 0.70f && t.adaptive_switching < 0.40f)
        {
            return Build(traceId, turnId,
                pattern: "stubborn_escalation",
                intensity: Q01((t.failed_push_streak + t.aggression) * 0.5f),
                direction: "self-harm",
                explanation: "Repeated aggressive pushes without payoff suggests stubborn escalation instead of adjustment."
            );
        }

        // Repeated defensive stabilization -> measured control
        if (t.defensive_holding_tendency > 0.60f && t.pacing_consistency > 0.60f && t.patience > 0.60f)
        {
            return Build(traceId, turnId,
                pattern: "measured_control",
                intensity: Q01((t.defensive_holding_tendency + t.pacing_consistency) * 0.5f),
                direction: "strategic_gain",
                explanation: "Consistent pacing with defensive holding indicates measured control and stable decision-making."
            );
        }

        // Switching after failures -> adaptive recovery
        if (t.failed_push_streak > 0.33f && t.adaptive_switching > 0.60f)
        {
            return Build(traceId, turnId,
                pattern: "adaptive_recovery",
                intensity: Q01((t.failed_push_streak + t.adaptive_switching) * 0.5f),
                direction: "strategic_gain",
                explanation: "Switching strategies after setbacks indicates adaptive recovery under pressure."
            );
        }

        // Hesitation is high -> delayed commitment
        if (t.hesitation_frequency > 0.70f && t.patience > 0.55f)
        {
            return Build(traceId, turnId,
                pattern: "delayed_commitment",
                intensity: Q01((t.hesitation_frequency + t.patience) * 0.5f),
                direction: "neutral",
                explanation: "Frequent waiting before deploying suggests delayed commitment and cautious timing."
            );
        }

        // Retreat/re-engage present -> probing pressure
        if (t.retreat_reengage_tendency > 0.33f && t.risk_taking > 0.55f)
        {
            return Build(traceId, turnId,
                pattern: "probing_pressure",
                intensity: Q01((t.retreat_reengage_tendency + t.risk_taking) * 0.5f),
                direction: "neutral",
                explanation: "Retreating after failure and re-engaging suggests probing pressure to find openings."
            );
        }

        // -----------------------------
        // Existing single-turn-style patterns (fallback)
        // -----------------------------
        if (t.aggression > 0.8f && t.risk_taking > 0.7f)
        {
            return Build(traceId, turnId,
                pattern: "impulsive_force",
                intensity: Q01((t.aggression + t.risk_taking) * 0.5f),
                direction: "self-harm",
                explanation: "High aggression with high risk-taking created vulnerability through over-commitment."
            );
        }

        if (t.patience > 0.7f && t.consistency > 0.7f)
        {
            return Build(traceId, turnId,
                pattern: "disciplined_control",
                intensity: Q01((t.patience + t.consistency) * 0.5f),
                direction: "strategic_gain",
                explanation: "High patience and consistency produced controlled, reliable decisions."
            );
        }

        if (t.adaptability < 0.3f)
        {
            return Build(traceId, turnId,
                pattern: "rigidity",
                intensity: Q01(1f - t.adaptability),
                direction: "neutral",
                explanation: "Low adaptability suggests repeated patterns and difficulty responding to change."
            );
        }

        if (t.aggression > 0.7f && t.foresight < 0.4f)
        {
            return Build(traceId, turnId,
                pattern: "reckless_pressure",
                intensity: Q01((t.aggression + (1f - t.foresight)) * 0.5f),
                direction: "self-harm",
                explanation: "High aggression with low foresight created vulnerability."
            );
        }

        return Build(traceId, turnId,
            pattern: "balanced_response",
            intensity: 0.5f,
            direction: "neutral",
            explanation: "Behavior signals were balanced without a dominant extreme."
        );
    }

    private static KarmaOutput Build(string traceId, int turnId, string pattern, float intensity, string direction, string explanation)
    {
        intensity = Q01(Mathf.Clamp01(intensity));

        return new KarmaOutput
        {
            trace_id = traceId,
            turn_id = turnId,
            karma = new KarmaOutput.Karma
            {
                pattern = pattern,
                intensity = intensity,
                direction = direction,
                explanation = explanation
            }
        };
    }

    private static float Q01(float v)
    {
        v = Mathf.Clamp01(v);
        return Mathf.Round(v * 1000f) / 1000f;
    }

    private static BehaviorTrace Clamp01(BehaviorTrace t)
    {
        // clamp everything to be safe; recorder already quantizes
        t.aggression = Mathf.Clamp01(t.aggression);
        t.risk_taking = Mathf.Clamp01(t.risk_taking);
        t.patience = Mathf.Clamp01(t.patience);
        t.consistency = Mathf.Clamp01(t.consistency);
        t.adaptability = Mathf.Clamp01(t.adaptability);
        t.foresight = Mathf.Clamp01(t.foresight);

        t.hesitation_frequency = Mathf.Clamp01(t.hesitation_frequency);
        t.failed_push_streak = Mathf.Clamp01(t.failed_push_streak);
        t.defensive_holding_tendency = Mathf.Clamp01(t.defensive_holding_tendency);
        t.retreat_reengage_tendency = Mathf.Clamp01(t.retreat_reengage_tendency);
        t.adaptive_switching = Mathf.Clamp01(t.adaptive_switching);
        t.pacing_consistency = Mathf.Clamp01(t.pacing_consistency);

        return t;
    }
}