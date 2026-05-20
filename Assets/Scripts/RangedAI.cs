using UnityEngine;

public class RangedAI : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;
    public float stopDistance = 6f;

    [Header("射擊設定")]
    public GameObject projectilePrefab;
    public float fireRate = 2f;
    private float fireTimer;

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

        // 隨時面朝玩家
        float dirX = player.position.x - transform.position.x;
        if (dirX != 0) sr.flipX = dirX < 0;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        fireTimer -= Time.deltaTime;

        if (distanceToPlayer <= stopDistance && fireTimer <= 0)
        {
            ShootPlayer();
            fireTimer = fireRate;
        }
    }

    void ShootPlayer()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.targetTag = "Player";
            proj.Initialize(direction);
        }
    }
}