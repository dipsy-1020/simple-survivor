using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public int damage = 10; // 劍的基礎傷害

    // 當有「碰撞體」進入劍的「觸發區域 (Is Trigger)」時觸發
    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查進入的東西是不是敵人 (透過 Tag)
        if (other.CompareTag("Enemy"))
        {
            // 嘗試從敵人身上抓取 EnemyHealth 腳本
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            // 如果怪物身上有血量腳本，就讓它扣血
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // 這裡可以預留位置，之後加打擊特效或音效
                Debug.Log("劍砍中了 " + other.name);
            }
        }
    }
}