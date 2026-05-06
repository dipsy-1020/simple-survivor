using UnityEngine;

/// <summary>
/// 萬用遠程發射模組 (行為層)。
/// 只負責「生成子彈並賦予方向與傷害」，不包含任何冷卻時間或索敵判斷 (交由 AI 大腦負責)。
/// </summary>
public class ProjectileAttackModule : MonoBehaviour
{
    [Header("發射設定")]
    [Tooltip("要發射的子彈 Prefab (記得上面要掛 UniversalDamageHitbox 和移動腳本)")]
    public GameObject projectilePrefab;

    [Tooltip("子彈的生成位置")]
    public Transform firePoint;

    /// <summary>
    /// 執行射擊 (由大腦呼叫)
    /// </summary>
    /// <param name="direction">子彈飛行的方向 (常態化向量)</param>
    /// <param name="damage">這發子彈的傷害</param>
    /// <param name="targetTag">要攻擊的目標標籤 ("Player" 或 "Enemy")</param>
    public void Fire(Vector2 direction, int damage, string targetTag)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("發射模組缺少 Prefab 或 FirePoint！");
            return;
        }

        // 1. 生成子彈實體
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 2. 賦予子彈移動方向
        // 根據我們現有的 EnemyProjectile 腳本，我們呼叫它的 Initialize 來傳遞方向
        EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            // 將原本 EnemyProjectile 裡的 Initialize 方法稍作修改，讓它只管接收方向與速度
            projScript.Initialize(direction);
        }

        // 3. 動態設定這發子彈的「萬用傷害觸發器」
        // 這就是模組化的魔法：如果玩家發射，標籤就會被設為 "Enemy"；如果是蝙蝠發射，就會是 "Player"
        UniversalDamageHitbox hitbox = bullet.GetComponent<UniversalDamageHitbox>();
        if (hitbox != null)
        {
            hitbox.damage = damage;
            hitbox.targetTag = targetTag;
            hitbox.destroyOnHit = true; // 確保遠程子彈打中目標後會自我毀滅
        }
    }
}