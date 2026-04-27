using UnityEngine;
using System.Collections.Generic;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance;

    [Header("Projectile Pools")]
    public ProjectilePool archerPool;
    public ProjectilePool tankPool;

    private Dictionary<ProjectileType, ProjectilePool> poolMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        poolMap = new Dictionary<ProjectileType, ProjectilePool>
        {
            { ProjectileType.Archer, archerPool },
            { ProjectileType.Tank, tankPool }
        };
    }

    public Projectile GetProjectile(
        ProjectileType type,
        Vector3 position,
        Quaternion rotation
    )
    {
        if (!poolMap.ContainsKey(type))
        {
            Debug.LogError($"No pool registered for projectile type: {type}");
            return null;
        }

        return poolMap[type].GetProjectile(position, rotation);
    }

    public void ReturnProjectile(Projectile projectile)
    {
        if (!poolMap.ContainsKey(projectile.projectileType))
        {
            Destroy(projectile.gameObject);
            return;
        }

        poolMap[projectile.projectileType].ReturnProjectile(projectile);
    }
}
