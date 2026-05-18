using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 5f;
    public float lifeTime = 5f;

    [Header("追蹤導航設定")]
    public float homingTurnSpeed = 10f; // 轉向靈敏度
    public float homingDelay = 0.25f;  // 剛射出時的延遲

    private float currentAge = 0f;
    private Transform homingTarget;
    private Vector2 flyDirection;

    // ✨ 新增：穿透時的導航失靈計時器
    private float disableHomingTimer = 0f;

    public void Initialize(Vector2 direction, Transform target = null)
    {
        flyDirection = direction.normalized;
        homingTarget = target;

        float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        Destroy(gameObject, lifeTime);
    }

    // ✨ 新增：給 UniversalDamageHitbox 呼叫的核心方法！
    // 當穿透發生時，強制讓計時器歸零，並讓導航失靈一段時間
    public void ResetHomingDelay(float duration)
    {
        disableHomingTimer = duration;
    }

    void Update()
    {
        currentAge += Time.deltaTime;

        // ✨ 倒數失靈時間
        if (disableHomingTimer > 0)
        {
            disableHomingTimer -= Time.deltaTime;
        }

        // ✨ 只有在「開局延遲結束」且「沒有處於穿透失靈狀態」時，才進行導航轉向！
        if (currentAge >= homingDelay && disableHomingTimer <= 0)
        {
            if (homingTarget != null && homingTarget.gameObject.activeInHierarchy)
            {
                Vector2 desiredDirection = (homingTarget.position - transform.position).normalized;
                flyDirection = Vector3.Slerp(flyDirection, desiredDirection, homingTurnSpeed * Time.deltaTime).normalized;

                float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        }

        // 往前飛行
        transform.Translate(flyDirection * speed * Time.deltaTime, Space.World);
    }
}