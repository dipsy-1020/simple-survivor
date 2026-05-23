using UnityEngine;

public class OrbitalWeapon : MonoBehaviour
{
    public int damage = 15;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // ®M¥Î­¿²v
                float multiplier = UpgradeManager.instance != null ? UpgradeManager.instance.globalDamageMultiplier : 1f;
                int finalDamage = Mathf.RoundToInt(damage * multiplier);

                enemy.TakeDamage(finalDamage);

                // §l¦åÅÞ¿è
                if (UpgradeManager.instance != null && UpgradeManager.instance.orbitalLifestealPercent > 0)
                {
                    float healRaw = finalDamage * UpgradeManager.instance.orbitalLifestealPercent;
                    int actualHeal = Mathf.FloorToInt(healRaw);
                    if (Random.value < (healRaw - actualHeal)) actualHeal += 1;

                    if (actualHeal > 0)
                    {
                        PlayerHealth player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
                        if (player != null) player.Heal(actualHeal);
                    }
                }
            }
        }
    }
}