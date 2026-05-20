using UnityEngine;
using System.Collections; // 必須引入這個才能使用協程

public class AutoShooter : MonoBehaviour
{
    [Header("發射設定")]
    public GameObject projectilePrefab;
    public float fireRate = 1.5f;
    public float detectionRadius = 8f;

    private float fireTimer;

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            StartCoroutine(FireBurstRoutine());
            fireTimer = 0f;
        }
    }

    // 使用協程處理「連發 (Burst)」的時間差
    IEnumerator FireBurstRoutine()
    {
        // 讀取升級面板的數值
        int burstCount = UpgradeManager.instance.flyingSwordBurstCount;
        int multiShot = UpgradeManager.instance.flyingSwordCountPerShot;

        for (int b = 0; b < burstCount; b++)
        {
            Transform target = GetNearestEnemy();
            if (target == null) break; // 畫面沒敵人就停止發射

            // 基準角度 (朝向敵人)
            Vector2 baseDir = (target.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            // 扇形發射邏輯：計算每把劍的角度
            float spreadAngle = 15f * (multiShot - 1); // 每多一把劍，扇形擴大 15 度
            float startAngle = baseAngle - (spreadAngle / 2f);

            for (int i = 0; i < multiShot; i++)
            {
                float currentAngle = startAngle + (i * 15f);
                Vector2 dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

                GameObject sword = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Projectile proj = sword.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.targetTag = "Enemy";
                    proj.Initialize(dir);
                }
            }

            yield return new WaitForSeconds(0.1f); // 每次連發間隔 0.1 秒
        }
    }

    // 將索敵邏輯獨立出來，讓協程可以重複呼叫
    Transform GetNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Transform nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearest = hit.transform;
                }
            }
        }
        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}