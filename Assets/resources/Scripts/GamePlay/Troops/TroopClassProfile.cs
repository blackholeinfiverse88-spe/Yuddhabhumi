using UnityEngine;

public abstract class TroopClassProfile : ScriptableObject
{
    [Header("Identity")]
    public string className;

    [Header("Core Stats")]
    public float maxHealth = 50f;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Tooltip("How far target must move before NavMesh destination updates")]
    public float destinationUpdateThreshold = 0.5f;

    [Header("Aggro")]
    public float aggroRadius = 60f;

    [Header("Behavior Flags")]
    public bool canChaseEnemies = true;
    public bool prefersBaseOverTroops = false;

    // We use 'abstract' if every troop MUST define its own chase logic
    public abstract bool ShouldChaseEnemy(float distanceToEnemy);

    // We use 'virtual' so we can provide a DEFAULT logic that works for most troops
    public virtual bool ShouldDisengage(float distanceToEnemy)
    {
        // Default logic: Give up if enemy is 50% beyond aggro range
        return distanceToEnemy > (aggroRadius * 1.5f);
    }

    public abstract int GetTargetPriority(Troop enemy);
}