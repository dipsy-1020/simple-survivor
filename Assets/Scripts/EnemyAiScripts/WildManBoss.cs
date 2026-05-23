using UnityEngine;

public class WildManBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2.5f;

    [Header("傷害設定")]
    public int touchDamage = 15;        // 普攻傷害
    public int dashDamage = 30;         // 新增：衝撞技能傷害！
    public float touchCooldown = 1f;
    private float lastTouchTime;

    [Header("衝刺技能")]
    public float dashCooldown = 4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.5f;

    private float skillTimer;
    private bool isDashing;
    private Vector2 dashDirection;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        skillTimer = dashCooldown;
    }

    void Update()
    {
        if (player == null) return;

        if (!isDashing)
        {
            skillTimer -= Time.deltaTime;
            if (skillTimer <= 0) StartDash();
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            if (direction.x != 0) sr.flipX = direction.x < 0;
        }
    }

    void StartDash()
    {
        isDashing = true;
        sr.color = new Color(1f, 0.4f, 0.4f);
        dashDirection = (player.position - transform.position).normalized;
        Invoke("EndDash", dashDuration);
    }

    void EndDash()
    {
        isDashing = false;
        sr.color = Color.white;
        skillTimer = dashCooldown;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastTouchTime + touchCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // --- 重點修改：根據狀態決定傷害 ---
                int finalDamage = isDashing ? dashDamage : touchDamage;
                ph.TakeDamage(finalDamage);
                lastTouchTime = Time.time;
            }
        }
    }
}