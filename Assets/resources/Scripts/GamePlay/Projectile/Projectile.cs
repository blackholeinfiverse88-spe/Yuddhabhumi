using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public ProjectileType projectileType;
    public float speed = 10f;
    public float damage;

    [Header("Hit Effect")]
    public GameObject hitEffect;

    private Transform target;
    private ProjectilePool pool;

    // Called ONCE by pool
    public void SetPool(ProjectilePool projectilePool)
    {
        pool = projectilePool;
    }

    // Called every time projectile is fired
    public void Initialize(Transform target, float damageValue, float projectileSpeed)
    {
        this.target = target;
        damage = damageValue;
        speed = projectileSpeed;

        transform.localScale = Vector3.one;

        if (projectileType == ProjectileType.Tank)
        {
            transform.localScale *= 1.5f;
            damage *= 2f;
            speed *= 0.7f;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        if (target.TryGetComponent(out Troop troop))
            troop.TakeDamage(damage);

        if (target.TryGetComponent(out BaseHealth baseHealth))
            baseHealth.TakeDamage(damage);

        if (hitEffect)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        target = null;

        if (pool != null)
            pool.ReturnProjectile(this);
        else
            Destroy(gameObject);
    }
}
