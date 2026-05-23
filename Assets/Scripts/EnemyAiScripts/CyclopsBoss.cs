using UnityEngine;

public class CyclopsBoss : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 1.5f;

    [Header("碰撞傷害")]
    public int touchDamage = 20;
    public float touchCooldown = 1f;
    private float lastTouchTime;

    [Header("震地技能 (AoE)")]
    public float smashCooldown = 5f;
    public float chargeTime = 1f;
    public float smashRadius = 4f;
    public int smashDamage = 40;

    private float skillTimer;
    private float currentChargeTimer;
    private bool isCharging = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private LineRenderer warningCircle;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        skillTimer = smashCooldown;

        SetupWarningCircle();
    }

    void SetupWarningCircle()
    {
        warningCircle = gameObject.AddComponent<LineRenderer>();
        warningCircle.startWidth = 0.15f;
        warningCircle.endWidth = 0.15f;
        warningCircle.positionCount = 51;
        warningCircle.useWorldSpace = false;
        warningCircle.loop = true;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        warningCircle.material = mat;
        warningCircle.startColor = new Color(1f, 0f, 0f, 0.8f);
        warningCircle.endColor = new Color(1f, 0f, 0f, 0.8f);

        warningCircle.enabled = false;
    }

    // 畫圓圈的邏輯
    void DrawCircle(float radius)
    {
        if (warningCircle == null) return;
        float angle = 0f;
        for (int i = 0; i <= 50; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            warningCircle.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isCharging)
        {
            skillTimer -= Time.deltaTime;
            if (skillTimer <= 0)
            {
                StartSmash();
            }
        }
        else
        {
            // 動態放大紅圈
            currentChargeTimer -= Time.deltaTime;
            float progress = 1f - (currentChargeTimer / chargeTime); // 0.0 到 1.0
            DrawCircle(smashRadius * progress);

            if (currentChargeTimer <= 0)
            {
                ExecuteSmash();
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null || isCharging) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        if (direction.x != 0) sr.flipX = direction.x < 0;
    }

    void StartSmash()
    {
        isCharging = true;
        currentChargeTimer = chargeTime;
        sr.color = Color.red;

        if (warningCircle != null) warningCircle.enabled = true;
    }

    void ExecuteSmash()
    {
        sr.color = Color.white;
        if (warningCircle != null) warningCircle.enabled = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, smashRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(smashDamage);
            }
        }

        isCharging = false;
        skillTimer = smashCooldown;
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