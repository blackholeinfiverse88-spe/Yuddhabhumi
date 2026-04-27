using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeamComponent))]
public class Troop : MonoBehaviour
{
    /* ───────── IDENTITY ───────── */
    [Header("Identity")]
    public TroopType troopType;
    public TroopClassProfile classProfile;

    /* ───────── STATS ───────── */
    [Header("Stats")]
    public float maxHealth = 50f;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    /* ───────── MOVEMENT ───────── */
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 720f;

    /* ───────── PROJECTILE ───────── */
    [Header("Projectile")]
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 15f;

    /* ───────── BASE ───────── */
    [Header("Base")]
    public float baseAttackDistance = 2.5f;
    public float tankFireRange = 10f;

    [Header("VFX Settings")]
    public GameObject spellVFXPrefab; // Drag your VFX prefab here in Inspector
    public Transform spellSpawnPoint; // Drag Maria's hand/staff bone here


    [Header("Audio")]
    public AudioClip strikeSound; // Assign your "Sword Hit" or "Strike" clip here
    [Range(0, 1)] public float strikeVolume = 0.8f;

    /* ───────── UI ───────── */
    public GameObject healthBarPrefab;

    [Header("Giant Effects")]
    public AudioClip roarSound;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    [Header("Support/Witch Settings")]
    public float supportRadius = 5f;
    public float healAmount = 5f; // Amount per tick
    public float areaDamage = 5f; // Amount per tick
    public GameObject manualMoveTarget; // Assign in Inspector for custom endpoint
    private GameObject activeSpellVFX;

    public enum SupportType { None, HealthRegen, HealthDecrease }

    public SupportType supportSubType;

    AttackPoint assignedPoint;

    bool hasReachedTargetPoint; 
    bool isSpawning = true; // New variable to block movement during roar

    /* ───────── RUNTIME ───────── */
    float currentHealth;
    float attackTimer;

    private Coroutine brainCoroutine;
    private int troopLayerMask;

    NavMeshAgent agent;
    TeamComponent team;
    Animator animator;

    BaseHealth targetBase;
    Transform baseAttackPoint;
    Troop enemyTarget;
    Vector3 lastDestination;

    HealthBar healthBar;

    public bool isDead { get; private set; }
    bool isDying;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        team = GetComponent<TeamComponent>();
        animator = GetComponentInChildren<Animator>();

        agent.speed = moveSpeed;
        agent.acceleration = 20f;
        agent.angularSpeed = 0f;
        agent.updateRotation = false;
        agent.autoBraking = false;
        agent.stoppingDistance = 0f;
        lastDestination = Vector3.positiveInfinity;
    }

    void Start()
    {
        currentHealth = maxHealth;
        AssignBaseTargets();
        SpawnHealthBar();
        
        troopLayerMask = 1 << LayerMask.NameToLayer("Troops");

        // --- AUTO-FIND DESTINATION FOR PREFABS ---
        if (troopType == TroopType.Support && manualMoveTarget == null)
        {
            // Player's Witches look for Enemy side points, Enemy Witches look for Player side points
            string searchTag = "";
            if (supportSubType == SupportType.HealthRegen)
                searchTag = (team.team == Team.Player) ? "EnemyRegenPoint" : "PlayerRegenPoint";
            else if (supportSubType == SupportType.HealthDecrease)
                searchTag = (team.team == Team.Player) ? "EnemyDrainPoint" : "PlayerDrainPoint";

            if (!string.IsNullOrEmpty(searchTag))
            {
                manualMoveTarget = GameObject.FindWithTag(searchTag);
            }
        }

        if (troopType == TroopType.Giant || troopType == TroopType.DemonKnight)
        {
            StartCoroutine(HandleSpawnSequence());
        }
        else
        {
            isSpawning = false;
            Invoke(nameof(SafeInitialMove), 0.1f);
        }

        brainCoroutine = StartCoroutine(TroopBrainTick());
    }

    IEnumerator TroopBrainTick()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));

        while (!isDead && !isDying)
        {
            if (!isSpawning && classProfile != null)
            {
                if (troopType == TroopType.Support) UpdateSupportLogic(); // <-- New Branch
                else if (troopType == TroopType.Tank) UpdateTankLogic();
                else if (troopType == TroopType.DemonKnight || troopType == TroopType.Giant) UpdateBaseBreakerLogic();
                else UpdateCombatLogic();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void UpdateSupportLogic()
    {
        // 0. Safety Check
        if (isDead || isDying) 
        {
            if (activeSpellVFX != null) Destroy(activeSpellVFX);
            return;
        }

        // 1. SCAN FOR TARGETS (Ally or Enemy)
        Collider[] hits = Physics.OverlapSphere(transform.position, supportRadius, troopLayerMask);
        bool foundTarget = false;
        Troop closestEnemy = null;
        float minEnemyDist = float.MaxValue;

        foreach (var hit in hits)
        {
            Troop t = hit.GetComponentInParent<Troop>();
            if (t && t != this && !t.isDead && !t.isDying)
            {
                // --- REGEN WITCH LOGIC ---
                if (supportSubType == SupportType.HealthRegen && t.team.team == this.team.team)
                {
                    if (t.currentHealth < t.maxHealth)
                    {
                        foundTarget = true;
                        t.Heal(t.maxHealth); // Full regain
                    }
                }
                // --- DECREASE WITCH LOGIC ---
                else if (supportSubType == SupportType.HealthDecrease && t.team.team != this.team.team)
                {
                    foundTarget = true;
                    t.TakeDamage(areaDamage * 0.1f); // Health decrease

                    // Track closest for rotation
                    float d = Vector3.Distance(transform.position, t.transform.position);
                    if (d < minEnemyDist) { minEnemyDist = d; closestEnemy = t; }
                }
            }
        }

        // 2. STATE MACHINE (Priority: Cast > Move to manualMoveTarget > Idle)
        if (foundTarget)
        {
            // --- STATE: CASTING ---
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            animator.SetBool("IsAttacking", true);
            animator.SetBool("IsMoving", false);

            if (closestEnemy != null) FaceTarget(closestEnemy.transform);

            if (activeSpellVFX == null && spellVFXPrefab != null)
            {
                activeSpellVFX = Instantiate(spellVFXPrefab, transform.position, Quaternion.identity, transform);
            }
        }
        else
        {
            // --- STATE: NOT CASTING ---
            animator.SetBool("IsAttacking", false);
            if (activeSpellVFX != null) Destroy(activeSpellVFX);

            // Now uses the manualMoveTarget found via Tag in Start()
            if (manualMoveTarget != null)
            {
                float distToPoint = Vector3.Distance(transform.position, manualMoveTarget.transform.position);

                if (distToPoint > agent.stoppingDistance + 0.5f)
                {
                    // --- STATE: MOVING ---
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(manualMoveTarget.transform.position);
                    }
                    animator.SetBool("IsMoving", true);
                }
                else
                {
                    // --- STATE: IDLE AT DESTINATION ---
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                    }
                    animator.SetBool("IsMoving", false);
                    
                    // Look at the enemy base while idling
                    if (targetBase != null) FaceTarget(targetBase.transform);
                }
            }
        }
    }

    // Add this helper method to your TakeDamage area
    public void Heal(float amount)
    {
        if (isDead || isDying) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthBar?.SetHealth(currentHealth, maxHealth);
    }

    IEnumerator HandleSpawnSequence()
    {
        isSpawning = true;
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;

        // Use Play() instead of SetTrigger if the transition is failing
        animator?.Play("Mutant Roaring"); 

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

        if (roarSound != null)
            AudioSource.PlayClipAtPoint(roarSound, transform.position, 1.0f);

        yield return new WaitForSeconds(2.0f); 

        isSpawning = false; 

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = false;
            
        SafeInitialMove();
    }

    void SafeInitialMove()
    {
        if (baseAttackPoint != null && classProfile != null)
            ForceMoveToBase();
    }

    void Update()
    {
        if (isSpawning || isDead || isDying || classProfile == null)
            return;

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        HandleRotation();
        UpdateAnimation();
        
        // REMOVED logic branches from here - they are now in the Coroutine!

        // DEBUG LINE: Draws a blue line to the target so you can see if detection works
    if (enemyTarget != null)
    {
        Debug.DrawLine(transform.position + Vector3.up, enemyTarget.transform.position + Vector3.up, Color.blue);
    }
    }

    

    void HandleRotation()
{
    Vector3 targetDirection = Vector3.zero;

    // MODE 1: Rotate toward movement velocity if moving
    if (agent.enabled && agent.velocity.sqrMagnitude > 0.1f)
    {
        targetDirection = agent.velocity.normalized;
    }
    // MODE 2: Rotate toward enemy if attacking/standing still
    else if (enemyTarget != null)
    {
        targetDirection = (enemyTarget.transform.position - transform.position).normalized;
    }
    // MODE 3: Rotate toward base if near it
    else if (targetBase != null)
    {
        targetDirection = (targetBase.transform.position - transform.position).normalized;
    }

    if (targetDirection != Vector3.zero)
    {
        targetDirection.y = 0; // Keep the troop upright
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Use a high speed for snappy but smooth turns
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
}


    private string currentBehavior = "Idle"; // For smart logging
   void UpdateCombatLogic()
{
    if (enemyTarget == null)
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0)
        {
            ScanForEnemies();
            scanTimer = 0.2f; 
        }

        if (enemyTarget == null) 
        { 
            // Base Attack Logic
            if (targetBase != null && baseAttackPoint != null)
            {
                float distToBase = Vector3.Distance(transform.position, baseAttackPoint.position);
                if (distToBase <= baseAttackDistance)
                {
                    // DISABLE AVOIDANCE to prevent jittering at the base
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero; // Stops the "sliding" into the base
                    }

                    FaceTarget(targetBase.transform);
                    TryAttackBase();
                    return; 
                }
            }
            
            // Re-enable avoidance when moving back to base
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            ForceMoveToBase(); 
            return; 
        }
    }

    // --- Enemy Target Logic ---
    if (enemyTarget.isDead || enemyTarget.isDying)
    {
        enemyTarget = null;
        // Re-enable avoidance so we can move around other troops again
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed; 
        }
        ResetCombatAnimation();
        return;
    }

    float distToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);
    
    if (distToTarget <= attackRange)
    {
        // Keep avoidance on so they don't overlap, but use Low Quality for performance
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) 
        {
            // SOFT STOP: Set speed to 0 and friction away the remaining velocity
            agent.speed = 0; 
            agent.velocity = Vector3.Lerp(agent.velocity, Vector3.zero, Time.deltaTime * 10f);
        }

        AttackEnemy(); 
    }
    else
    {
        // CHASE MODE: Restore speed and avoidance
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed; // Ensure speed is reset to default
        }

        ChaseEnemy();
        animator.SetBool("IsAttacking", false);
    }
}
// This replaces heavy Debug.Logs that cause lag
void SetSmartLog(string state)
{
    if (currentBehavior != state)
    {
        currentBehavior = state;
        // Debug.Log($"[Troop {gameObject.name}] Status: {state}"); // Uncomment only for deep debugging
    }
}
    void AttackEnemy()
{
    if (!enemyTarget || enemyTarget.isDead || enemyTarget.isDying) return;

    // 1. Face target
    FaceTarget(enemyTarget.transform);

    // 2. Slow movement to "Shuffle" speed instead of stopping
    if (troopType == TroopType.Melee && agent.isActiveAndEnabled && agent.isOnNavMesh)
    {
        // If we are already in range, keep speed at 0. 
        // If we just entered range, this allows a tiny bit of momentum.
        float distToEnemy = Vector3.Distance(transform.position, enemyTarget.transform.position);
        agent.speed = (distToEnemy <= attackRange) ? 0f : moveSpeed * 0.3f;
    }

    // 3. Attack Cooldown
    if (attackTimer > 0f) return;

    attackTimer = attackCooldown;
    animator?.SetBool("IsAttacking", true);

    if (troopType == TroopType.Melee)
    {
        enemyTarget.TakeDamage(attackDamage);
        if(GameFlowManager.Instance != null) GameFlowManager.Instance.AddDamage(attackDamage);
    }
    else
    {
        FireProjectile(enemyTarget.transform);
    }
}

    float scanTimer = 0f;
    void ScanForEnemies()
{
    // We scan everything in the radius. No layer mask = no layer errors.
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, classProfile.aggroRadius);
    
    float closestDist = float.MaxValue;
    Troop bestTarget = null;

    foreach (var hit in hitColliders)
    {
        // This is the fix for the "Layer Change" - find the script on the parent
        Troop t = hit.GetComponentInParent<Troop>(); 
        
        // Don't target yourself, dead units, or teammates
        if (!t || t == this || t.isDead || t.isDying || t.team.team == this.team.team) continue;

        float d = Vector3.Distance(transform.position, t.transform.position);
        if (d < closestDist)
        {
            closestDist = d;
            bestTarget = t;
        }
    }
    enemyTarget = bestTarget;
}
    void UpdateTankLogic()
    {
        if (!targetBase || !baseAttackPoint)
        {
            ForceMoveToBase();
            return;
        }

        float dist = Vector3.Distance(transform.position, baseAttackPoint.position);

        if (dist <= tankFireRange)
        {
            FaceTarget(targetBase.transform);
            TryAttackBase();
        }
        else
        {
            ForceMoveToBase();
        }
    }

    void UpdateBaseBreakerLogic()
    {
        if (!targetBase || !baseAttackPoint)
        {
            ForceMoveToBase();
            return;
        }

        float dist = Vector3.Distance(transform.position, baseAttackPoint.position);

        // This handles both the approach and the state once reached
        if (hasReachedTargetPoint || dist <= baseAttackDistance)
    {
        if (!hasReachedTargetPoint) 
        {
            // Instead of disabling the agent, we just park it
            if (agent.isActiveAndEnabled && agent.isOnNavMesh) 
            {
                agent.speed = 0;
                agent.velocity = Vector3.zero;
            }
            // Optional: Move them to the exact point, but keep agent enabled
            transform.position = baseAttackPoint.position;
            hasReachedTargetPoint = true;
        }

        FaceTarget(targetBase.transform);
        TryAttackBase(); 
    }
        else
        {
            ForceMoveToBase();
        }
    }


    void TryAttackBase()
    {
        if (attackTimer > 0f || !targetBase)
            return;

        attackTimer = attackCooldown;
        animator?.SetBool("IsAttacking", true);

        // Check if this is a melee heavy unit
        if (troopType == TroopType.Melee || 
            troopType == TroopType.DemonKnight || 
            troopType == TroopType.Giant)
        {
            // Stop movement to play attack animation
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            
            // DEAL THE DAMAGE
            targetBase.TakeDamage(attackDamage);

            // Record damage for the end-game result screen
            if(GameFlowManager.Instance != null) 
                GameFlowManager.Instance.AddDamage(attackDamage);
        }
        else
        {
            // Handle Ranged or Tank projectiles
            FireProjectile(targetBase.transform);
        }
    }
    void ForceMoveToBase()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (baseAttackPoint == null) return;
        if (classProfile == null) return;

        agent.isStopped = false;

        if (Vector3.Distance(lastDestination, baseAttackPoint.position) >
            classProfile.destinationUpdateThreshold)
        {
            lastDestination = baseAttackPoint.position;
            agent.SetDestination(lastDestination);
        }
    }

    void ChaseEnemy()
{
    if (!enemyTarget) return;

    // Default to the enemy's body
    Vector3 targetPos = enemyTarget.transform.position;

    // SMART TARGETING: If it's a Giant, target the point he's occupying at the base
    if ((enemyTarget.troopType == TroopType.Giant || enemyTarget.troopType == TroopType.DemonKnight || enemyTarget.troopType == TroopType.Tank) 
        && enemyTarget.baseAttackPoint != null)
    {
        targetPos = enemyTarget.baseAttackPoint.position;
    }

    if (agent.isStopped) agent.isStopped = false;

    // Update destination if the target spot has moved (or if we just locked on)
    float distToLastDest = Vector3.Distance(lastDestination, targetPos);
    if (distToLastDest > 0.5f)
    {
        lastDestination = targetPos;
        agent.SetDestination(targetPos);
    }
}

    


    void FireProjectile(Transform target)
    {
        if (!projectileSpawnPoint || !ProjectilePoolManager.Instance || !target)
            return;

        Projectile p = ProjectilePoolManager.Instance.GetProjectile(
            troopType == TroopType.Tank ? ProjectileType.Tank : ProjectileType.Archer,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation
        );

        p?.Initialize(target, attackDamage, projectileSpeed);
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isDying) return;

        currentHealth -= amount;
        healthBar?.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            if (troopType == TroopType.Melee || 
                troopType == TroopType.Giant || 
                troopType == TroopType.DemonKnight)
            {
                StartCoroutine(MeleeDeath());
            }
            else
            {
                InstantDeath();
            }
        }
    }

    IEnumerator MeleeDeath()
    {
        isDying = true;
        
        if (assignedPoint != null)
        {
            assignedPoint.Release();
        }

        agent.enabled = false;
        animator?.SetTrigger("IsDead"); 

        yield return new WaitForSeconds(5.4f);

        isDead = true;
        Destroy(gameObject);
    }

    void InstantDeath()
    {
        isDead = true;
        Destroy(gameObject);
    }

    void FaceTarget(Transform t)
    {
        if (!t) return;

        Vector3 look = t.position;
        look.y = transform.position.y;
        transform.LookAt(look);
    }

    void UpdateAnimation()
    {
        if (!animator || isDead || isDying) return;

        // The Support logic manages its own animator parameters
        if (troopType == TroopType.Support) return;

        bool isMoving = agent.enabled && agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    void ResetCombatAnimation()
    {
        if (!animator) return;
        animator.SetBool("IsAttacking", false);
    }

    void AssignBaseTargets()
    {
        string targetTag = "";

        if (troopType == TroopType.Support)
        {
            if (supportSubType == SupportType.HealthRegen)
            {
                // Regen Witch goes deep towards enemy base
                targetTag = (team.team == Team.Player) ? "EnemyRegenPoint" : "PlayerRegenPoint";
            }
            else if (supportSubType == SupportType.HealthDecrease)
            {
                // Decrease Witch stays back near its own team's side
                targetTag = (team.team == Team.Player) ? "EnemyDrainPoint" : "PlayerDrainPoint";
            }
        }
        else
        {
            // Standard Melee/Giant logic
            targetTag = (team.team == Team.Player) ? "EnemyAttackPoint" : "PlayerAttackPoint";
        }

        // Standard Occupancy Logic
        GameObject[] points = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject p in points)
        {
            AttackPoint ap = p.GetComponent<AttackPoint>();
            if (ap != null && !ap.isOccupied)
            {
                assignedPoint = ap;
                assignedPoint.Occupy(this);
                baseAttackPoint = assignedPoint.transform;
                
                targetBase = (team.team == Team.Player) ? 
                    GameObject.FindWithTag("EnemyBase")?.GetComponent<BaseHealth>() : 
                    GameObject.FindWithTag("PlayerBase")?.GetComponent<BaseHealth>();
                return;
            }
        }
        
        // Fallback if no specific points are found
        if (points.Length > 0) baseAttackPoint = points[0].transform;
    }

    void SpawnHealthBar()
    {
        if (!healthBarPrefab) return;

        GameObject canvas = GameObject.FindWithTag("WorldCanvas");
        if (!canvas) return;

        GameObject hb = Instantiate(healthBarPrefab, canvas.transform);
        healthBar = hb.GetComponent<HealthBar>();
        healthBar.target = transform;
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    private void OnDrawGizmosSelected()
{
    if (classProfile != null)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, classProfile.aggroRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, classProfile.aggroRadius * 1.5f); // Disengage range
    }

    if (troopType == TroopType.Support)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, supportRadius);
        }
}

    public void TriggerAttackVFX()
    {
        if (spellVFXPrefab != null && spellSpawnPoint != null)
        {
            // Instantiate the effect
            GameObject vfx = Instantiate(spellVFXPrefab, spellSpawnPoint.position, spellSpawnPoint.rotation);
            
            // Optional: Destroy it after 2 seconds so it doesn't clutter the scene
            Destroy(vfx, 2f);
        }
    }

    // This function will be called by your Animation Event
    public void PlayStrikeSound()
    {
        if (strikeSound != null)
        {
            // Plays the sound in 3D space at the character's location
            AudioSource.PlayClipAtPoint(strikeSound, transform.position, strikeVolume);
        }
    }
}