using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("掉落設定")]
    public GameObject expGemPrefab; // 拖入你的經驗寶石 Prefab

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 加這行：播放打擊音效
        if (AudioManager.instance != null) AudioManager.instance.PlayHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 死亡時生成經驗寶石
        if (expGemPrefab != null)
        {
            Instantiate(expGemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}