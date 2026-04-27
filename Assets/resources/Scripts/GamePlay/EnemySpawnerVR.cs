using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

public enum AIAggressionState
{
    Normal,
    Aggressive
}

public class EnemySpawnerVR : MonoBehaviour
{
    /* ───────────────────── CONFIG ───────────────────── */

    [Header("Enemy Troop Prefabs")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Energy Costs")]
    public List<float> energyCosts = new List<float>();

    [Header("Enemy Energy")]
    public EnemyEnergySystem enemyEnergy;

    [Header("Spawn Control")]
    public int maxEnemiesAlive = 3;
    public float decisionInterval = 2.5f;
    public float initialDelay = 5f;

    [Header("Difficulty Tuning")]
    [Range(0.3f, 1f)]
    public float spawnSpeedMultiplier = 0.7f;

    [Header("Aggression Settings")]
    public float aggressiveDecisionMultiplier = 0.6f;
    public int aggressiveExtraUnits = 1;

    [Header("AI Debug")]
    public bool enableSpawnLogging = true;

    /* ───────────────────── RUNTIME ───────────────────── */

    private float decisionTimer;
    private float elapsedTime;

    private AIAggressionState aggressionState = AIAggressionState.Normal;
    private BaseHealth enemyBase;

    private List<AISpawnLog> spawnLogs = new List<AISpawnLog>();

    /* ───────────────────── UNITY ───────────────────── */

    private void Start()
    {
        GameObject baseObj = GameObject.FindWithTag("EnemyBase");
        if (baseObj != null)
            enemyBase = baseObj.GetComponent<BaseHealth>();
    }

    private void Update()
    {
        if (enemyEnergy == null || enemyBase == null)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < initialDelay)
            return;

        UpdateAggressionState();

        int allowedEnemies = maxEnemiesAlive;
        float interval = decisionInterval;

        if (aggressionState == AIAggressionState.Aggressive)
        {
            allowedEnemies += aggressiveExtraUnits;
            interval *= aggressiveDecisionMultiplier;
        }

        if (CountEnemies() >= allowedEnemies)
            return;

        decisionTimer += Time.deltaTime * spawnSpeedMultiplier;

        if (decisionTimer < interval)
            return;

        decisionTimer = 0f;
        AttemptStrategicSpawn();
    }

    /* ───────────────────── AGGRESSION ───────────────────── */

    private void UpdateAggressionState()
    {
        float healthRatio = enemyBase.currentHealth / enemyBase.maxHealth;
        aggressionState = (healthRatio <= 0.1f)
            ? AIAggressionState.Aggressive
            : AIAggressionState.Normal;
    }

    /* ───────────────────── AI DECISION ───────────────────── */

    private void AttemptStrategicSpawn()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Count == 0)
            return;

        int index = DecideSpawnIndex();

        if (index < 0 || index >= enemyPrefabs.Count)
            return;

        float cost = energyCosts.Count > index ? energyCosts[index] : 0f;

        if (!enemyEnergy.TrySpend(cost))
            return;

        Transform spawnPoint = spawnPoints[index % spawnPoints.Count];

        SpawnEnemy(enemyPrefabs[index], spawnPoint);

        if (enableSpawnLogging)
            Debug.Log($"[AI SPAWN] Index:{index} Energy:{enemyEnergy.currentEnergy:F1}");
    }

    private int DecideSpawnIndex()
    {
        // Simple example decision:
        // pick random troop (replace with your AI logic later)
        return UnityEngine.Random.Range(0, enemyPrefabs.Count);
    }

    /* ───────────────────── SPAWN ───────────────────── */

    private void SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        if (!prefab || !spawnPoint)
            return;

        GameObject troopGO =
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

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
            tc.team = Team.Enemy;
    }

    /* ───────────────────── UTIL ───────────────────── */

    private int CountEnemies()
    {
        Troop[] troops = FindObjectsByType<Troop>(FindObjectsSortMode.None);

        int count = 0;

        foreach (var t in troops)
        {
            if (!t) continue;

            TeamComponent tc = t.GetComponent<TeamComponent>();
            if (tc && tc.team == Team.Enemy)
                count++;
        }

        return count;
    }
}
