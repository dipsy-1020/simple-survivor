using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 5f;
    public float lifeTime = 5f;
    private Vector2 flyDirection;

    public void Initialize(Vector2 direction)
    {
        flyDirection = direction.normalized;
        float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(flyDirection * speed * Time.deltaTime, Space.World);
    }

    // ✨ 新增：彈射尋敵雷達
    public void BounceToNearestEnemy(GameObject ignoreEnemy)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 10f); // 彈射索敵半徑 10 公尺
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider2D hit in hitColliders)
        {
            // 找敵人，且不能是剛剛打中的那隻(防止原地卡死)
            if (hit.CompareTag("Enemy") && hit.gameObject != ignoreEnemy)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = hit.gameObject;
                }
            }
        }

        if (nearestEnemy != null)
        {
            // 找到新目標，重新設定方向！
            Vector2 newDirection = (nearestEnemy.transform.position - transform.position).normalized;
            Initialize(newDirection);
        }
    }
}