using UnityEngine;

public class CyclopsBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 1.5f; // 比普通怪慢，營造沉重感

    [Header("碰撞傷害")]
    public int touchDamage = 20;
    public float touchCooldown = 1f;
    private float lastTouchTime;

    [Header("震地技能 (AoE)")]
    public float smashCooldown = 5f;    // 幾秒放一次技能
    public float chargeTime = 1f;       // 停下腳步蓄力幾秒
    public float smashRadius = 4f;      // 震地範圍半徑
    public int smashDamage = 40;        // 震地傷害

    private float skillTimer;
    private bool isCharging = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        skillTimer = smashCooldown;
    }

    void Update()
    {
        if (player == null) return;

        // 技能計時器
        if (!isCharging)
        {
            skillTimer -= Time.deltaTime;
            if (skillTimer <= 0)
            {
                StartSmash();
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null || isCharging) return; // 蓄力時不准移動

        // 追擊玩家並轉身
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        if (direction.x != 0) sr.flipX = direction.x < 0;
    }

    void StartSmash()
    {
        isCharging = true;
        // 這裡未來可以加入改變圖片顏色或播放動畫的邏輯，提示玩家快跑！
        sr.color = Color.red;
        Invoke("ExecuteSmash", chargeTime); // 延遲執行震地
    }

    void ExecuteSmash()
    {
        // 恢復原本顏色
        sr.color = Color.white;

        // 偵測範圍內的所有物件
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, smashRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(smashDamage);
            }
        }

        // 技能結束，重新計時
        isCharging = false;
        skillTimer = smashCooldown;
    }

    // 身體碰撞傷害
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastTouchTime + touchCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(touchDamage);
                lastTouchTime = Time.time;
            }
        }
    }

    // 畫出震地範圍供開發時調整
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, smashRadius);
    }
}