using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [Header("敵人設定")]
    public float moveSpeed = 2f; // 怪物通常設定得比主角慢一點，讓玩家有機會走位

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 確保不受重力影響且不會亂滾
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 【關鍵】利用 Tag (標籤) 在場景中自動尋找主角
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("找不到主角！請確認騎士物件的 Tag 是否有設為 'Player'");
        }
    }

    void FixedUpdate()
    {
        // 只要主角還活著，就一直朝他走過去
        if (player != null)
        {
            // 計算從怪物指向主角的方向向量
            Vector2 direction = (player.position - transform.position).normalized;

            // 使用物理引擎移動怪物
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            // 處理怪物的左右翻轉（跟著移動方向轉頭）
            if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}