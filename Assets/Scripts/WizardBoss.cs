using UnityEngine;

public class WizardBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 2f;
    public float keepDistance = 6f; // 想要跟玩家保持的距離

    [Header("扇形彈幕技能")]
    public GameObject projectilePrefab; // 拖入怪物的子彈 Prefab
    public float fireCooldown = 3f;
    public int projectileCount = 5;     // 一次發射幾發
    public float spreadAngle = 60f;     // 扇形散佈的總角度
    private float fireTimer;

    [Header("瞬移防身 (選填)")]
    [Tooltip("如果玩家太靠近，幾秒能瞬移一次")]
    public float teleportCooldown = 5f;
    public float triggerTeleportDistance = 3f; // 玩家靠多近會觸發
    public Vector2 fieldMin; // 場地左下邊界 (同 Spawner 設定)
    public Vector2 fieldMax; // 場地右上邊界
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

        // 隨時面朝玩家
        Vector2 directionToPlayer = player.position - transform.position;
        if (directionToPlayer.x != 0) sr.flipX = directionToPlayer.x < 0;

        float distance = directionToPlayer.magnitude;

        // 保持距離邏輯：如果大於安全距離就靠近，小於就後退
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

        // 1. 彈幕射擊計時
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            FireSpread();
            fireTimer = fireCooldown;
        }

        // 2. 瞬移計時與觸發
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

        // 計算朝向玩家的中心基準角度
        Vector2 baseDirection = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // 計算每一發子彈的角度間隔
        float startAngle = baseAngle - (spreadAngle / 2f);
        float angleStep = spreadAngle / (projectileCount - 1); // 減1是為了平均分配在邊界

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 bulletDir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.targetTag = "Player"; // 確保打在玩家身上
                proj.Initialize(bulletDir);
            }
        }
    }

    void TeleportAway()
    {
        // 隨機傳送到場地內的新位置
        float rx = Random.Range(fieldMin.x, fieldMax.x);
        float ry = Random.Range(fieldMin.y, fieldMax.y);
        transform.position = new Vector2(rx, ry);
        // 未來可以加上瞬移特效
    }
}