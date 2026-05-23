using UnityEngine;

public class DashAI : MonoBehaviour
{
    [Header("移動設定")]
    public float normalSpeed = 2.5f;

    [Header("傷害設定")]
    public int normalDamage = 10;       // 普攻傷害
    public int dashDamage = 20;         // 新增：衝撞傷害
    public float damageCooldown = 1f;
    private float lastDamageTime;

    [Header("衝刺技能")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 4f;

    private float dashTimer;
    private float dashCooldownTimer;
    private bool isDashing = false;
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
        dashCooldownTimer = dashCooldown;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float dirX = player.position.x - transform.position.x;
        if (dirX != 0) sr.flipX = dirX < 0;

        if (isDashing)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * normalSpeed * Time.fixedDeltaTime);
            dashCooldownTimer -= Time.fixedDeltaTime;

            if (dashCooldownTimer <= 0)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashDirection = direction;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // --- 重點修改：根據狀態決定傷害 ---
                int finalDamage = isDashing ? dashDamage : normalDamage;
                ph.TakeDamage(finalDamage);
                lastDamageTime = Time.time;
            }
        }
    }
}