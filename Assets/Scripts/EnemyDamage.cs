using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("傷害設定")]
    public int damage = 10;          // 撞到玩家扣多少血
    public float damageCooldown = 1f; // 咬一口之後，間隔幾秒才能再咬第二口

    private float lastDamageTime;

    // 當怪物碰到玩家時觸發 (如果你的怪物是用 Collider2D 沒有勾 IsTrigger)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }

    // 當怪物碰到玩家時觸發 (如果你的怪物是用 Collider2D 且有勾 IsTrigger)
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}