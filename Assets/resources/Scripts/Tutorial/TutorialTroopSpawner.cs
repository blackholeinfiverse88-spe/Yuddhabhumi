using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TutorialTroopSpawner : MonoBehaviour
{
    [Header("Energy System")]
    public TutorialEnergySystem energySystem;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Fixed Tutorial Deck")]
    [Tooltip("Drag the exact same 3 UnitData files here that you put in the TutorialUIManager.")]
    public List<UnitData> tutorialDeck;

    [Header("Tutorial Locks")]
    [Tooltip("Keep this unchecked. We will unlock it when they reach Section 2.")]
    public bool isSpawningAllowed = false;

    // --- NEW: Call this from the Section 2 Trigger ---
    public void UnlockSpawning()
    {
        isSpawningAllowed = true;
        Debug.Log("Tutorial: Troop spawning is now unlocked!");
    }

    // Connected to UI Buttons (0, 1, 2)
    public void SpawnUnit(int deckIndex)
    {
        // 1. Check if they are allowed to spawn yet
        if (!isSpawningAllowed)
        {
            Debug.Log("Tutorial: Spawning blocked. Player is not in Section 2 yet.");
            return;
        }

        if (tutorialDeck == null || tutorialDeck.Count == 0) return;
        if (deckIndex < 0 || deckIndex >= tutorialDeck.Count) return;

        UnitData unit = tutorialDeck[deckIndex];

        // 2. Spend Elixir locally
        if (energySystem != null && !energySystem.TrySpend(unit.cost))
            return;

        if (unit.prefab == null)
        {
            Debug.LogError("UnitData prefab missing in Tutorial Spawner!");
            return;
        }

        // 3. Pick Spawn Point
        Transform spawnPoint = spawnPoints[deckIndex % spawnPoints.Length];

        // 4. Spawn the Troop
        GameObject troopGO = Instantiate(
            unit.prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // 5. Snap to NavMesh
        if (NavMesh.SamplePosition(
                troopGO.transform.position,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
        {
            troopGO.transform.position = hit.position;
        }

        // 6. Ensure Player Team
        TeamComponent tc = troopGO.GetComponent<TeamComponent>();
        if (tc != null)
            tc.team = Team.Player;
    }
}