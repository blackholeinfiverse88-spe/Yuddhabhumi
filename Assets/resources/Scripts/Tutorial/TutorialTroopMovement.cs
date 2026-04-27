using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TutorialTroopMovement : MonoBehaviour
{
    [Header("Combat Stats")]
    public float attackRange = 2.5f;
    public float damage = 25f;
    public float attackCooldown = 1.0f;

    private NavMeshAgent agent;
    private BaseHealth targetBase;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find the base
        GameObject baseObj = GameObject.FindWithTag("EnemyBase");
        if (baseObj != null)
        {
            targetBase = baseObj.GetComponent<BaseHealth>();
        }
        else
        {
            Debug.LogError("Tutorial Brain: I can't find anything exactly tagged 'EnemyBase'!");
        }
    }

    void Update()
    {
        // Stop moving if the base is destroyed or missing
        if (targetBase == null || targetBase.currentHealth <= 0)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            return;
        }

        // --- NEW SAFETY CHECK ---
        // If we are falling from the spawner and haven't touched the blue floor yet, wait!
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Tutorial Troop: Waiting to touch the NavMesh...");
            return; 
        }

        // We are on the floor! Check distance to base.
        float distance = Vector3.Distance(transform.position, targetBase.transform.position);
        
        if (distance <= attackRange)
        {
            agent.isStopped = true; // Arrived!

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                targetBase.TakeDamage(damage);
                lastAttackTime = Time.time;
                Debug.Log("Tutorial Troop: WHACK! Base took damage.");
            }
        }
        else
        {
            // Keep walking
            agent.isStopped = false;
            agent.SetDestination(targetBase.transform.position);
        }
    }
}