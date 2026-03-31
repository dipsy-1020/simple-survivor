using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 5f;          // 子彈速度
    public int damage ;            // 子彈傷害
    public float lifeTime = 5f;       // 子彈最大存活時間 (防呆用)

    private Vector2 moveDirection;
    private bool isInitialized = false;

    // ✨ 關鍵：由蝙蝠呼叫此 function 來設定子彈的方向
    public void Initialize(Vector2 direction, int incomingDamage) // ✨ 新增參數
    {
        moveDirection = direction.normalized;
        damage = incomingDamage; // ✨ 接收蝙蝠傳過來的傷害值
        isInitialized = true;
        // 旋轉子彈，讓它的前方朝向移動方向 (選做，增加視覺效果)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 存活時間到自動銷毀
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!isInitialized) return;

        // 子彈筆直飛行
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判定撞到玩家
        if (other.CompareTag("Player"))
        {
            // 嘗試抓取玩家的血量腳本 (假設你叫 PlayerHealth)
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // 撞到玩家後子彈銷毀
            Destroy(gameObject);
        }

        // ✨ (選做) 撞到牆壁也銷毀，避免子彈飛出地圖
        // if (other.CompareTag("Wall")) { Destroy(gameObject); }
    }
}