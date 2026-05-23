using UnityEngine;

public class WizardBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;
    public float keepDistance = 6f;

    [Header("碰撞傷害")]
    public int touchDamage = 30;
    public float touchCooldown = 1f;
    private float lastTouchTime;

    [Header("扇形彈幕技能")]
    public GameObject projectilePrefab;
    public float fireCooldown = 3f;
    public int projectileCount = 5;
    public float spreadAngle = 60f;

    // 👇 新增：子彈專屬傷害
    public int bulletDamage = 25;
    private float fireTimer;

    [Header("瞬移防身")]
    public float teleportCooldown = 5f;
    public float triggerTeleportDistance = 3f;
    public Vector2 fieldMin;
    public Vector2 fieldMax;
    private float tpTimer;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        fireTimer = fireCooldown;
        tpTimer = teleportCooldown;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 directionToPlayer = player.position - transform.position;
        if (directionToPlayer.x != 0) sr.flipX = directionToPlayer.x < 0;

        float distance = directionToPlayer.magnitude;

        if (distance > keepDistance + 0.5f)
        {
            rb.MovePosition(rb.position + directionToPlayer.normalized * speed * Time.fixedDeltaTime);
        }
        else if (distance < keepDistance - 0.5f)
        {
            rb.MovePosition(rb.position - directionToPlayer.normalized * speed * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        if (player == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            FireSpread();
            fireTimer = fireCooldown;
        }

        tpTimer -= Time.deltaTime;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= triggerTeleportDistance && tpTimer <= 0)
        {
            TeleportAway();
            tpTimer = teleportCooldown;
        }
    }

    void FireSpread()
    {
        if (projectilePrefab == null) return;

        Vector2 baseDirection = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle - (spreadAngle / 2f);
        float angleStep = spreadAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 bulletDir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.targetTag = "Player";
                // 👇 重點修復：賦予子彈設定好的傷害
                proj.damage = bulletDamage;
                proj.Initialize(bulletDir);
            }
        }
    }

    void TeleportAway()
    {
        float rx = Random.Range(fieldMin.x, fieldMax.x);
        float ry = Random.Range(fieldMin.y, fieldMax.y);
        transform.position = new Vector2(rx, ry);
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