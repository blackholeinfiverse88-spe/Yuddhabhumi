using UnityEngine;

public static class CombatConsequenceEngine
{
    [System.Serializable]
    public struct CombatStateSnapshot
    {
        public int frame;
        public float match_time_sec;          // quantized
        public int player_units_alive;        // observed count
        public float player_frontline_depth01; // 0..1
        public float player_formation_spread01; // 0..1
        public float total_damage;            // observable (from GameFlowManager)
        public float damage_delta01;          // quantized 0/1 (internal, not used as final output)
    }

    [System.Serializable]
    public struct CombatConsequence
    {
        public string pattern;       // overextension / failed_push / formation_collapse / spacing_recovery / defensive_stabilization / pressure_imbalance / none
        public float intensity;      // 0..1
        public string direction;     // self-harm / neutral / strategic_gain
        public string explanation;   // sentence

        public CombatStateSnapshot snapshot; // stored for replay proof
    }

    public static CombatConsequence Evaluate(CombatStateSnapshot prev, CombatStateSnapshot curr)
    {
        // NOTE: rule order = priority order (deterministic)

        // Formation collapse: units drop sharply and frontline retreats
        if (prev.player_units_alive >= 0 &&
            (prev.player_units_alive - curr.player_units_alive) >= 2 &&
            (curr.player_frontline_depth01 + 0.10f) < prev.player_frontline_depth01)
        {
            float drop = Mathf.Clamp01((prev.player_units_alive - curr.player_units_alive) / 5f);
            return Build(curr,
                pattern: "formation_collapse",
                intensity: Q01(drop),
                direction: "self-harm",
                explanation: "Unit presence dropped and the frontline retreated, indicating a formation collapse."
            );
        }

        // Overextension: frontline very deep with low unit count
        if (curr.player_frontline_depth01 > 0.75f && curr.player_units_alive <= 2)
        {
            float intensity = Mathf.Clamp01((curr.player_frontline_depth01 - 0.75f) / 0.25f);
            return Build(curr,
                pattern: "overextension",
                intensity: Q01(intensity),
                direction: "self-harm",
                explanation: "Frontline pushed too deep with insufficient support, indicating overextension."
            );
        }

        // Spacing recovery: spread reduces materially after being high
        if (prev.player_formation_spread01 > 0.60f && (curr.player_formation_spread01 + 0.20f) < prev.player_formation_spread01)
        {
            float intensity = Mathf.Clamp01(prev.player_formation_spread01 - curr.player_formation_spread01);
            return Build(curr,
                pattern: "spacing_recovery",
                intensity: Q01(intensity),
                direction: "strategic_gain",
                explanation: "Unit spacing tightened after a wide spread, indicating spacing recovery."
            );
        }

        // Failed push: deep-ish frontline but no damage gain in this step
        if (curr.player_frontline_depth01 > 0.55f && curr.damage_delta01 <= 0.0f)
        {
            float intensity = Mathf.Clamp01((curr.player_frontline_depth01 - 0.55f) / 0.45f);
            return Build(curr,
                pattern: "failed_push",
                intensity: Q01(intensity),
                direction: "self-harm",
                explanation: "A forward push did not convert into damage, indicating a failed push."
            );
        }

        // Defensive stabilization: shallow frontline + tight formation + stable presence
        if (curr.player_frontline_depth01 < 0.35f && curr.player_formation_spread01 < 0.35f && curr.player_units_alive >= 4)
        {
            float intensity = Mathf.Clamp01((curr.player_units_alive - 4) / 6f);
            return Build(curr,
                pattern: "defensive_stabilization",
                intensity: Q01(intensity),
                direction: "strategic_gain",
                explanation: "Frontline stayed controlled with tight formation, indicating defensive stabilization."
            );
        }

        // Pressure imbalance: strong presence + forward depth + damage gain
        if (curr.player_units_alive >= 5 && curr.player_frontline_depth01 > 0.50f && curr.damage_delta01 > 0.0f)
        {
            float intensity = Mathf.Clamp01((curr.player_units_alive - 5) / 6f);
            return Build(curr,
                pattern: "pressure_imbalance",
                intensity: Q01(intensity),
                direction: "strategic_gain",
                explanation: "Sustained forward presence with damage gain indicates pressure imbalance in your favor."
            );
        }

        return Build(curr,
            pattern: "none",
            intensity: 0.0f,
            direction: "neutral",
            explanation: "No dominant combat consequence detected at this moment."
        );
    }

    private static CombatConsequence Build(CombatStateSnapshot snap, string pattern, float intensity, string direction, string explanation)
    {
        return new CombatConsequence
        {
            pattern = pattern,
            intensity = Q01(intensity),
            direction = direction,
            explanation = explanation,
            snapshot = snap
        };
    }

    public static CombatStateSnapshot QuantizeSnapshot(CombatStateSnapshot s)
    {
        s.match_time_sec = Q01(s.match_time_sec);
        s.player_frontline_depth01 = Q01(s.player_frontline_depth01);
        s.player_formation_spread01 = Q01(s.player_formation_spread01);
        s.total_damage = Q01(s.total_damage);
        s.damage_delta01 = Q01(s.damage_delta01);
        return s;
    }

    private static float Q01(float v)
    {
        v = Mathf.Clamp01(v);
        return Mathf.Round(v * 1000f) / 1000f;
    }
}