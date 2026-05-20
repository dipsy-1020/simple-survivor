using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        // 改為呼叫 GameManager
        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"玩家受到傷害: {damage}！ 剩餘血量: {currentHealth}");

        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"玩家恢復: {amount}！ 目前血量: {currentHealth}");

        if (GameManager.instance != null) GameManager.instance.UpdateHPUI(currentHealth, maxHealth);
    }

    void Die()
    {
        Debug.Log("玩家死亡！");
        if (GameManager.instance != null) GameManager.instance.ShowGameOver();
    }
}