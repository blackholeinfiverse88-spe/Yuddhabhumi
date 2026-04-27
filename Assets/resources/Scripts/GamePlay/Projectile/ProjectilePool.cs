using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Settings")]
    public Projectile projectilePrefab;
    public int initialSize = 10;

    private Queue<Projectile> pool = new Queue<Projectile>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewProjectile();
        }
    }

    private Projectile CreateNewProjectile()
    {
        Projectile p = Instantiate(projectilePrefab, transform);
        p.gameObject.SetActive(false);
        p.SetPool(this);
        pool.Enqueue(p);
        return p;
    }

    public Projectile GetProjectile(Vector3 position, Quaternion rotation)
    {
        Projectile p = pool.Count > 0 ? pool.Dequeue() : CreateNewProjectile();

        p.transform.position = position;
        p.transform.rotation = rotation;
        p.gameObject.SetActive(true);

        return p;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }
}
