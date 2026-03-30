using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f; // 移動速度，可以在 Unity 面板中隨時調整

    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // 抓取角色身上的組件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 確保重力不會把主角往下拖（俯視角遊戲不需要重力）
        rb.gravityScale = 0f;
        // 鎖定 Z 軸旋轉，避免角色撞到怪物或牆壁時像車禍一樣翻滾
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 獲取玩家輸入
        // 使用 GetAxisRaw 會直接回傳 -1, 0 或 1，這樣起步跟停止會很俐落，沒有「滑冰感」
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 處理角色翻轉：往左走時翻轉圖片，往右走時恢復
        if (movement.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        // 實際移動邏輯放在 FixedUpdate，確保物理運算平滑
        // movement.normalized 可以防止玩家同時按「上+右」時，斜向移動速度變快的問題
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}