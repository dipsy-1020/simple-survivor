using UnityEngine;
using UnityEngine.UI; // ✨ 必須使用此命名空間

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("受傷無敵時間")]
    public float invincibilityDuration = 1f;
    private float invincibilityTimer;

    private DamageFlash damageFlash;

    [Header("UI 設定 (一般血條)")]
    public Slider healthSlider; // ✨ 直接拖入 UI 上的 Slider 元件

    public GameObject tombstonePrefab;

    void Start()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<DamageFlash>();

        // 初始化血條數值
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    // 碰撞扣血邏輯 (維持不變，但記得確保怪物 Collider 不是 Trigger)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && invincibilityTimer <= 0)
        {
            EnemyDamage enemyDmg = collision.gameObject.GetComponent<EnemyDamage>();
            int damageAmount = (enemyDmg != null) ? enemyDmg.damage : 10;
            TakeDamage(damageAmount);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;

        if (damageFlash != null) damageFlash.CallFlash();

        UpdateHealthUI(); // ✨ 更新長條血條

        if (currentHealth <= 0) Die();
    }

    // ✨ 現在更新 UI 變超級簡單，直接把數字給 Slider 就好！
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
    }

    void Die()
    {
        if (tombstonePrefab != null) Instantiate(tombstonePrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
        FindObjectOfType<GameManager>().ShowGameOver();
    }
}