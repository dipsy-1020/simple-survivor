using UnityEngine;

public class ProjectileAttackModule : MonoBehaviour
{
    [Header("發射設定")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // ✨ 在括號最後面新增：Transform target = null
    public void Fire(Vector2 direction, int damage, string targetTag, int pierceCount = 0, int bounceCount = 0, Transform target = null)
    {
        if (projectilePrefab == null || firePoint == null) return;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();
        // ✨ 把 target 傳遞給子彈！
        if (projScript != null) projScript.Initialize(direction, target);

        UniversalDamageHitbox hitbox = bullet.GetComponent<UniversalDamageHitbox>();
        if (hitbox != null)
        {
            hitbox.damage = damage;
            hitbox.targetTag = targetTag;
            hitbox.destroyOnHit = true;
            hitbox.pierceCount = pierceCount;
            hitbox.bounceCount = bounceCount;
        }
    }
}