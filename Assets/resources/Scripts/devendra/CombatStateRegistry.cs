using System.Collections.Generic;
using UnityEngine;

public class CombatStateRegistry : MonoBehaviour
{
    public static CombatStateRegistry Instance { get; private set; }

    [Header("Frontline normalization")]
    [Tooltip("Distance along forward axis that maps to frontlineDepth01 = 1.0")]
    public float frontlineMaxMeters = 30f;

    [Tooltip("Formation spread (X) meters that maps to spread01 = 1.0")]
    public float spreadMaxMeters = 12f;

    private readonly List<Transform> _playerUnits = new List<Transform>(64);

    private Transform _playerOrigin;

    private float _prevTotalDamage;
    private bool _hasPrevDamage;

    private CombatConsequenceEngine.CombatStateSnapshot _prevSnapshot;
    private bool _hasPrevSnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerOrigin(Transform origin)
    {
        _playerOrigin = origin;
    }

    public void RegisterPlayerUnit(Transform t)
    {
        if (t == null) return;
        _playerUnits.Add(t);
    }

    public void UnregisterPlayerUnit(Transform t)
    {
        if (t == null) return;
        _playerUnits.Remove(t);
    }

    public CombatConsequenceEngine.CombatStateSnapshot CaptureSnapshot(float matchTimeSec, float totalDamage)
    {
        // cleanup nulls
        for (int i = _playerUnits.Count - 1; i >= 0; i--)
        {
            if (_playerUnits[i] == null)
                _playerUnits.RemoveAt(i);
        }

        int alive = _playerUnits.Count;

        float originZ = (_playerOrigin != null) ? _playerOrigin.position.z : 0f;
        float maxForward = 0f;

        // spread (x)
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        for (int i = 0; i < _playerUnits.Count; i++)
        {
            var p = _playerUnits[i].position;
            float forward = p.z - originZ;
            if (forward > maxForward) maxForward = forward;

            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
        }

        float depth01 = (frontlineMaxMeters <= 0.0001f) ? 0f : Mathf.Clamp01(maxForward / frontlineMaxMeters);
        float spreadMeters = (alive >= 2) ? (maxX - minX) : 0f;
        float spread01 = (spreadMaxMeters <= 0.0001f) ? 0f : Mathf.Clamp01(spreadMeters / spreadMaxMeters);

        float dmgDelta01 = 0f;
        if (_hasPrevDamage)
            dmgDelta01 = (totalDamage - _prevTotalDamage) > 0.0001f ? 1f : 0f;

        _prevTotalDamage = totalDamage;
        _hasPrevDamage = true;

        var snap = new CombatConsequenceEngine.CombatStateSnapshot
        {
            frame = Time.frameCount,
            match_time_sec = Mathf.Round(matchTimeSec * 1000f) / 1000f,
            player_units_alive = alive,
            player_frontline_depth01 = depth01,
            player_formation_spread01 = spread01,
            total_damage = totalDamage,
            damage_delta01 = dmgDelta01
        };

        return CombatConsequenceEngine.QuantizeSnapshot(snap);
    }

    public CombatConsequenceEngine.CombatConsequence EvaluateConsequence(float matchTimeSec, float totalDamage)
    {
        var curr = CaptureSnapshot(matchTimeSec, totalDamage);

        var prev = _hasPrevSnapshot ? _prevSnapshot : curr;
        var consequence = CombatConsequenceEngine.Evaluate(prev, curr);

        _prevSnapshot = curr;
        _hasPrevSnapshot = true;

        return consequence;
    }

    public void ResetRegistry()
    {
        _playerUnits.Clear();
        _playerOrigin = null;

        _hasPrevDamage = false;
        _prevTotalDamage = 0f;

        _hasPrevSnapshot = false;
        _prevSnapshot = default;
    }
}