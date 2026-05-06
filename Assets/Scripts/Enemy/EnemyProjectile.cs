using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 5f;
    public float lifeTime = 5f;

    private Vector2 flyDirection;

    // 讓 ProjectileAttackModule 呼叫，用來設定飛行方向
    public void Initialize(Vector2 direction)
    {
        flyDirection = direction.normalized;

        // 子彈生成後，確保它幾秒後會自動消失，避免塞爆記憶體
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 每一幀朝著設定好的方向等速飛行
        transform.Translate(flyDirection * speed * Time.deltaTime, Space.World);
    }
}