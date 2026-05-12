using UnityEngine;

public class ProjectileAttackModule : MonoBehaviour
{
    [Header("發射設定")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // ✨ 新增接收 bounceCount 參數
    public void Fire(Vector2 direction, int damage, string targetTag, int pierceCount = 0, int bounceCount = 0)
    {
        if (projectilePrefab == null || firePoint == null) return;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();
        if (projScript != null) projScript.Initialize(direction);

        UniversalDamageHitbox hitbox = bullet.GetComponent<UniversalDamageHitbox>();
        if (hitbox != null)
        {
            hitbox.damage = damage;
            hitbox.targetTag = targetTag;
            hitbox.destroyOnHit = true;
            hitbox.pierceCount = pierceCount;
            hitbox.bounceCount = bounceCount; // ✨ 賦予彈射次數
        }
    }
}