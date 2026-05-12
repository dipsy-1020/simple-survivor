using UnityEngine;

[RequireComponent(typeof(ProjectileAttackModule))]
public class PlayerAutoShoot : MonoBehaviour
{
    [Header("射擊設定")]
    public float fireRate = 1.5f;
    public int baseSwordDamage = 15;
    public int projectileCount = 1;
    public float spreadAngle = 15f;
    public int pierceCount = 0;
    public int bounceCount = 0; // ✨ 新增記錄彈射次數

    [Header("索敵設定")]
    public float detectionRadius = 8f;

    private float fireTimer;
    private ProjectileAttackModule attackModule;

    void Start() { attackModule = GetComponent<ProjectileAttackModule>(); }

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate) { ShootNearestEnemy(); fireTimer = 0f; }
    }

    void ShootNearestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < shortestDistance) { shortestDistance = distance; nearestEnemy = hit.gameObject; }
            }
        }

        if (nearestEnemy != null)
        {
            Vector2 baseDirection = (nearestEnemy.transform.position - transform.position).normalized;
            int totalDamage = baseSwordDamage;
            if (UpgradeManager.instance != null) totalDamage += UpgradeManager.instance.extraSwordDamage;

            float startAngle = -spreadAngle * (projectileCount - 1) / 2f;
            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngleOffset = startAngle + (i * spreadAngle);
                Vector2 fireDirection = Quaternion.Euler(0, 0, currentAngleOffset) * baseDirection;

                // ✨ 將 bounceCount 一起發射出去！
                attackModule.Fire(fireDirection, totalDamage, "Enemy", pierceCount, bounceCount);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}