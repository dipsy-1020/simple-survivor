using UnityEngine;
using UnityEngine.UI; // ✨ 必須使用此命名空間
using TMPro; // ✨ 記得引入

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("受傷無敵時間")]
    public float invincibilityDuration = 0.2f; // ✨ 縮短到 0.2 秒！1 秒真的太久了
    private float invincibilityTimer;

    private DamageFlash damageFlash;

    [Header("UI 設定 (一般血條)")]
    // ✨ 這裡將 Slider 改成了 Image，跟經驗條保持完全一樣的模式
    public Image healthBarFill;
    public TextMeshProUGUI healthText; // 用來顯示具體數字的文字框

    public GameObject tombstonePrefab;

    void Start()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<DamageFlash>();

        // ✨ 遊戲一開始就強制刷新文字與 UI
        UpdateHealthUI();
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0) return;

        // ✨ 動態難度：每增加 1 波，怪物傷害提升 30%！
        int actualDamage = damage;
        if (GameManager.instance != null)
        {
            float multiplier = 1f + ((GameManager.instance.currentWave - 1) * 0.3f);
            actualDamage = Mathf.RoundToInt(damage * multiplier);
        }

        currentHealth -= actualDamage;
        invincibilityTimer = invincibilityDuration;

        if (damageFlash != null) damageFlash.CallFlash();
        UpdateHealthUI();

        if (currentHealth <= 0) Die();
    }

    // ✨ 現在更新 UI 變超級簡單，計算比例給 fillAmount 就好！
    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // 將當前血量除以最大血量，算出 0.0 ~ 1.0 的小數比例
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        // 更新文字，例如 "HP: 80 / 100"
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth} / {maxHealth}";
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

        // 直接呼叫單例
        if (GameManager.instance != null) GameManager.instance.GameOver();
    }
}