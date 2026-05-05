using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PlayerTroopSpawnerVR : MonoBehaviour
{
    [Header("Energy")]
    public PlayerEnergySystem energySystem;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Deck")]
    public List<UnitData> debugDeck;

    public void SpawnUnit(UnitData unit)
    {
        // SAARTHI ENFORCEMENT (MANDATORY)
        // If SpawnUnit is called directly (bypassing TantraExecutionNode), block it.
        if (!SaarthiExecutionContext.IsAuthorized)
        {
            Debug.LogError("[SAARTHI_BLOCK] SpawnUnit blocked: no execution token context (bypass attempt).");
            return;
        }

        if (unit == null)
            return;

        if (!energySystem.TrySpend(unit.cost))
            return;

        if (unit.prefab == null)
        {
            Debug.LogError("UnitData prefab missing!");
            return;
        }

        Transform spawnPoint;

        if (spawnPoints != null && spawnPoints.Length > 0)
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = transform;

        GameObject troopGO = Instantiate(
            unit.prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (NavMesh.SamplePosition(
                troopGO.transform.position,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
        {
            troopGO.transform.position = hit.position;
        }

        TeamComponent tc = troopGO.GetComponent<TeamComponent>();
        if (tc != null)
            tc.team = Team.Player;
    }
}