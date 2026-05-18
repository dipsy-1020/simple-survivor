using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 5f;
    public float lifeTime = 5f;

    // ✨ 追蹤導航設定
    public float homingTurnSpeed = 10f; // 轉向靈敏度 (數值越低轉越大圈，數值越高越像死追)
    private Transform homingTarget;

    private Vector2 flyDirection;

    // ✨ 接收 target 參數
    public void Initialize(Vector2 direction, Transform target = null)
    {
        flyDirection = direction.normalized;
        homingTarget = target; // 鎖定目標！

        float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ==========================================
        // ✨ 追蹤導航核心邏輯
        // ==========================================
        // 確保目標還活著，並且沒有被摧毀 (activeInHierarchy)
        if (homingTarget != null && homingTarget.gameObject.activeInHierarchy)
        {
            Vector2 desiredDirection = (homingTarget.position - transform.position).normalized;

            // 使用 Slerp (球面線性插值) 讓飛劍「平滑地」轉向，而不是瞬間折角
            flyDirection = Vector3.Slerp(flyDirection, desiredDirection, homingTurnSpeed * Time.deltaTime).normalized;

            // 更新劍的圖片面向，讓劍尖永遠朝向飛行方向
            float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // 往前飛行
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