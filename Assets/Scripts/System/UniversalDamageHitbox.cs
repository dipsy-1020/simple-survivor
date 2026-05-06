using UnityEngine;

/// <summary>
/// 萬用傷害觸發器模組。
/// 可以掛在任何需要造成傷害的物件上（子彈、敵人的手、主角的劍）。
/// 透過 targetTag 決定它能傷害誰。
/// </summary>
public class UniversalDamageHitbox : MonoBehaviour
{
    [Header("傷害設定")]
    public int damage = 10;

    [Tooltip("填入要攻擊的對象標籤，例如 'Player' 或 'Enemy'")]
    public string targetTag = "Player"; // 預設攻擊玩家，掛在飛劍上時請改為 "Enemy"

    [Header("行為設定")]
    [Tooltip("如果打中目標後，這個物件是否要自我毀滅？（子彈通常要勾，劍或怪物的身體不勾）")]
    public bool destroyOnHit = false;

    // 處理 Trigger 觸發 (適用於子彈、飛劍等設為 isTrigger 的碰撞體)
    private void OnTriggerEnter2D(Collider2D other)
    {
        DealDamage(other.gameObject);
    }

    // 處理物理碰撞 (適用於實體怪物撞擊)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    // 將持續傷害整合在一起，減少重複代碼
    private void OnTriggerStay2D(Collider2D other)
    {
        // 若需要持續傷害（例如怪物的身體一直黏著主角），可在此呼叫
        // 但通常為了避免瞬間扣太多血，被攻擊方會實作「無敵幀 (I-frame)」
        DealDamage(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    // 核心傷害判定邏輯
    private void DealDamage(GameObject targetObj)
    {
        // 檢查對方是否為我們設定的目標標籤
        if (targetObj.CompareTag(targetTag))
        {
            // 嘗試獲取目標身上的血量系統 (這裡要兼容玩家和怪物的血量腳本)
            // 未來為了更完美的模組化，可以將 PlayerHealth 和 EnemyHealth 抽象出一個 IDamageable 介面
            // 但目前我們可以用簡單的 if 判斷來處理

            if (targetTag == "Player")
            {
                PlayerHealth playerHp = targetObj.GetComponent<PlayerHealth>();
                if (playerHp != null)
                {
                    // 這裡假設 PlayerHealth 有一個公開的 TakeDamage 方法
                    // 你需要確認你原來的 PlayerHealth.cs 裡是否有 public void TakeDamage(int damage)
                    playerHp.TakeDamage(damage);
                    HitResolution();
                }
            }
            else if (targetTag == "Enemy")
            {
                EnemyHealth enemyHp = targetObj.GetComponent<EnemyHealth>();
                if (enemyHp != null)
                {
                    // 這裡假設 EnemyHealth 有一個公開的 TakeDamage 方法
                    enemyHp.TakeDamage(damage);
                    HitResolution();
                }
            }
        }
    }

    // 處理打中後的後續動作 (例如子彈自我毀滅)
    private void HitResolution()
    {
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}