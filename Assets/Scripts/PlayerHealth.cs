using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("無敵幀設定")]
    public float invincibilityTime = 0.5f; // 無敵持續時間 (0.5秒)
    private bool isInvincible = false;     // 目前是否處於無敵狀態

    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        // 【核心防呆】如果玩家正在無敵狀態，直接忽略這次傷害，不扣血！
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"玩家受到傷害: {damage}！ 剩餘血量: {currentHealth}");

        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 觸發受傷閃爍與無敵機制 (確保玩家還活著才觸發)
            if (gameObject.activeInHierarchy) StartCoroutine(DamageFlashAndInvincible());
        }
    }

    // --- 玩家受傷閃爍與無敵幀處理 ---
    IEnumerator DamageFlashAndInvincible()
    {
        isInvincible = true; // 1. 開啟無敵狀態

        // 2. 視覺效果：受傷瞬間變紅
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        if (sr != null) sr.color = Color.white;

        // 3. 繼續等待剩下的無敵時間，確保這段期間內不會再受傷
        float remainingTime = invincibilityTime - 0.15f;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        isInvincible = false; // 4. 無敵時間結束，關閉無敵狀態
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("玩家死亡！");
        if (GameManager.instance != null) GameManager.instance.ShowGameOver();
    }
}