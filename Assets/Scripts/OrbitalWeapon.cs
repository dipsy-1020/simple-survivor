using UnityEngine;

public class OrbitalWeapon : MonoBehaviour
{
    [Header("傷害設定")]
    public int damage = 15;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // --- 吸血邏輯 ---
                // Random.value 會產生 0.0 ~ 1.0 的隨機數
                if (Random.value < UpgradeManager.instance.orbitalLifestealChance)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        PlayerHealth ph = player.GetComponent<PlayerHealth>();
                        if (ph != null) ph.Heal(5); // 觸發吸血，固定補 5 滴血
                    }
                }
            }
        }
    }
}