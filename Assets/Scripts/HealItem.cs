using UnityEngine;

public class HealItem : MonoBehaviour
{
    [Header("回血設定")]
    public int healAmount = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 只要玩家碰到，就補血並銷毀
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.Heal(healAmount);

                // 如果你有吃補血的音效，可以加在這裡
                // if (AudioManager.instance != null) AudioManager.instance.PlayHeal();

                Destroy(gameObject);
            }
        }
    }
}