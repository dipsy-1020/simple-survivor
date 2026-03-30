using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // ✨ 使用 List 必備

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("受傷無敵時間")]
    public float invincibilityDuration = 1f;
    private float invincibilityTimer;

    private DamageFlash damageFlash;

    [Header("UI 與死亡設定")]
    public Transform healthUIContainer; // ✨ 愛心們的「容器」(就是那個 Horizontal Layout Group)
    public GameObject heartPrefab; // ✨ 單一顆愛心的模具 (Prefab)
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public int healthPerHeart = 10;

    public GameObject tombstonePrefab;

    // ✨ 用 List 自動裝生成的愛心，再也不用手動拉陣列了！
    private List<Image> heartImages = new List<Image>();

    void Start()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<DamageFlash>();

        // 遊戲開始時，程式全自動幫你排好所有愛心！
        InitializeHealthUI();
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && invincibilityTimer <= 0)
        {
            // ✨ 嘗試抓取怪物身上的 EnemyDamage 腳本
            EnemyDamage enemyDmg = collision.gameObject.GetComponent<EnemyDamage>();

            // 如果怪物身上有這個腳本，就讀取它的攻擊力；如果沒有，就預設扣 10 滴
            int damageAmount = (enemyDmg != null) ? enemyDmg.damage : 10;

            TakeDamage(damageAmount);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;

        Debug.Log("主角被咬了！剩餘血量：" + currentHealth);

        if (damageFlash != null) damageFlash.CallFlash();

        UpdateHealthUI();

        if (currentHealth <= 0) Die();
    }

    // --- ✨ 全自動生成愛心的魔法 ✨ ---
    void InitializeHealthUI()
    {
        // 算出總共需要幾顆心 (例如 100 / 10 = 10顆)
        int totalHearts = maxHealth / healthPerHeart;

        for (int i = 0; i < totalHearts; i++)
        {
            // 生成一顆心，並直接指定它的父物件為 healthUIContainer
            GameObject newHeart = Instantiate(heartPrefab, healthUIContainer);

            // 把這顆心身上的 Image 元件抓出來，存進我們的 List 裡備用
            Image heartImage = newHeart.GetComponent<Image>();
            heartImage.sprite = fullHeart; // 預設滿血
            heartImages.Add(heartImage);
        }
    }

    // --- 更新 UI ---
    void UpdateHealthUI()
    {
        int currentHeartCount = currentHealth / healthPerHeart;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHeartCount)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    // ✨ 新增：專門用來處理回血與更新 UI 的公開方法
    public void Heal(int healAmount)
    {
        // 增加血量，但不能超過最大血量 (Mathf.Min 會取兩者中較小的那個)
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        Debug.Log("玩家補血了！目前血量：" + currentHealth);

        // ✨ 補完血後，立刻呼叫更新 UI！
        UpdateHealthUI();
    }

    void Die()
    {
        Debug.Log("主角陣亡！");
        if (tombstonePrefab != null) Instantiate(tombstonePrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);

        // ✨ 新增這行：找出 GameManager 並顯示結算畫面
        FindObjectOfType<GameManager>().ShowGameOver();
    }
}