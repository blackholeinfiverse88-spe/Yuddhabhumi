using System.Collections.Generic;
using UnityEngine;

public class AntiHiddenModifierValidator : MonoBehaviour
{
    public static AntiHiddenModifierValidator Instance { get; private set; }

    [Header("Authority-bearing assets to watch")]
    [Tooltip("Assign UnitData assets used in gameplay (deck/debugDeck). If empty, will attempt auto-collect at runtime.")]
    public List<UnitData> watchedUnits = new List<UnitData>(32);

    [Header("Validation")]
    public bool validateOnEachDeploy = true;
    public bool validateOnMatchEnd = true;

    private AuthorityStateSnapshot _baseline;
    private bool _hasBaseline;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CaptureBaseline(string tag)
    {
        AutoCollectIfEmpty();
        _baseline = AuthorityStateSnapshot.Capture($"baseline_{tag}", watchedUnits);
        _hasBaseline = true;

        Debug.Log($"[ANTI_MODIFIER_BASELINE] tag={tag} hash={_baseline.hash} units={_baseline.unitHashes.Count}");
    }

    public void ValidateNow(string tag)
    {
        if (!_hasBaseline)
        {
            CaptureBaseline("auto_first_check");
        }

        AutoCollectIfEmpty();
        var now = AuthorityStateSnapshot.Capture($"check_{tag}", watchedUnits);

        if (now.hash == _baseline.hash)
        {
            Debug.Log($"[ANTI_MODIFIER_CHECK] ok=true tag={tag} hash={now.hash}");
            return;
        }

        Debug.LogError($"[HIDDEN_MODIFIER_DETECTED] ok=false tag={tag} baseline_hash={_baseline.hash} now_hash={now.hash}");

        // Diff: which unit changed?
        foreach (var kv in _baseline.unitHashes)
        {
            string key = kv.Key;
            string baseHash = kv.Value;

            if (!now.unitHashes.TryGetValue(key, out string nowHash))
            {
                Debug.LogError($"[HIDDEN_MODIFIER_DIFF] missing_unit key={key}");
                continue;
            }

            if (baseHash != nowHash)
            {
                Debug.LogError($"[HIDDEN_MODIFIER_DIFF] unit_changed key={key} baseline={baseHash} now={nowHash}");
            }
        }

        // New units added?
        foreach (var kv in now.unitHashes)
        {
            if (!_baseline.unitHashes.ContainsKey(kv.Key))
                Debug.LogError($"[HIDDEN_MODIFIER_DIFF] new_unit key={kv.Key}");
        }
    }

    // Hook helpers (call these from match flow)
    public void OnMatchStart()
    {
        CaptureBaseline("match_start");
    }

    public void OnDeployValidated(int turnId)
    {
        if (!validateOnEachDeploy) return;
        ValidateNow($"deploy_turn_{turnId}");
    }

    public void OnMatchEnd()
    {
        if (!validateOnMatchEnd) return;
        ValidateNow("match_end");
    }

    private void AutoCollectIfEmpty()
    {
        if (watchedUnits != null && watchedUnits.Count > 0)
            return;

        watchedUnits = new List<UnitData>(32);

        // Attempt to pull from SelectedDeck if present
        try
        {
            if (SelectedDeck.deck != null)
            {
                for (int i = 0; i < SelectedDeck.deck.Count; i++)
                {
                    var u = SelectedDeck.deck[i];
                    if (u != null && !watchedUnits.Contains(u))
                        watchedUnits.Add(u);
                }
            }
        }
        catch { }

        // Attempt to pull from any spawner debugDeck in scene
        var spawner = FindFirstObjectByType<PlayerTroopSpawnerVR>();
        if (spawner != null && spawner.debugDeck != null)
        {
            for (int i = 0; i < spawner.debugDeck.Count; i++)
            {
                var u = spawner.debugDeck[i];
                if (u != null && !watchedUnits.Contains(u))
                    watchedUnits.Add(u);
            }
        }
    }
}