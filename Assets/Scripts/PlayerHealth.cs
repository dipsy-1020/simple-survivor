using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    private int currentHealth;

    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"玩家受到傷害: {damage}！ 剩餘血量: {currentHealth}");

        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);

        // 觸發受傷閃爍
        if (gameObject.activeInHierarchy) StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- 玩家受傷閃爍 (紅色) ---
    IEnumerator DamageFlash()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sr.color = Color.white;
        }
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