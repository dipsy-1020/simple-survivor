using UnityEngine;

public class MeleeAI : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 3f;

    [Header("傷害設定")]
    public int damage = 10;
    public float damageCooldown = 1f;
    private float lastDamageTime;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // 面朝向玩家
        if (direction.x != 0) sr.flipX = direction.x < 0;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}