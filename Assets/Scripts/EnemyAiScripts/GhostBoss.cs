using UnityEngine;
using System.Collections;

public class GhostBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;

    [Header("碰撞傷害")]
    public int touchDamage = 15;
    public float touchCooldown = 1f;
    private float lastTouchTime;

    [Header("瞬移技能")]
    public float teleportCooldown = 5f; // 每幾秒瞬移一次
    private float skillTimer;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        skillTimer = teleportCooldown;
    }

    void Update()
    {
        if (player == null) return;

        skillTimer -= Time.deltaTime;
        if (skillTimer <= 0)
        {
            StartCoroutine(Teleport());
            skillTimer = teleportCooldown;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        if (direction.x != 0) sr.flipX = direction.x < 0;
    }

    IEnumerator Teleport()
    {
        // 1. 身體變半透明準備消失
        Color c = sr.color;
        c.a = 0.3f;
        sr.color = c;

        yield return new WaitForSeconds(0.6f); // 消失滯空時間

        // 2. 瞬移到距離玩家 3.5 單位的隨機位置
        if (player != null)
        {
            Vector2 randomOffset = Random.insideUnitCircle.normalized * 3.5f;
            transform.position = (Vector2)player.position + randomOffset;
        }

        // 3. 恢復實體
        c.a = 1f;
        sr.color = c;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastTouchTime + touchCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(touchDamage); lastTouchTime = Time.time; }
        }
    }
}