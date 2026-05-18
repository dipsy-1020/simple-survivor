using UnityEngine;
using System.Collections;

/// <summary>
/// 近戰衝刺攻擊模組
/// 負責提供方向，透過 Rigidbody2D 給予瞬間物理速度來進行突進。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // 系統防呆：掛載此腳本時，會自動要求並加上 Rigidbody2D
public class MeleeAttackModule : MonoBehaviour
{
    [Header("衝刺技能設定")]
    public float dashForce = 20f;       // 衝刺的爆發速度 (數值越大衝越快)
    public float dashDuration = 0.3f;   // 衝刺持續的時間 (秒)

    private Rigidbody2D rb;
    private bool isDashing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 執行近戰衝刺攻擊 (由大腦 AI 腳本呼叫)
    /// </summary>
    /// <param name="direction">攻擊突進的方向向量</param>
    public void Attack(Vector2 direction)
    {
        // 確保不會在衝刺期間重複觸發
        if (!isDashing)
        {
            StartCoroutine(DashRoutine(direction.normalized));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        isDashing = true;

        // 1. ✨ 紀錄怪物平時走路時，那顆「高摩擦輪胎」的線性阻力 (舊版 Unity 叫 drag)
        float originalDrag = rb.linearDamping;

        // 2. ✨ 突進開始！瞬間把阻力歸零 (變成 0)，解除物理煞車對衝刺距離的束縛
        rb.linearDamping = 0f;

        // 3. 傳遞速度 (linearVelocity 如果也報錯，可以改成 velocity，如果沒報錯就不用動)
        rb.linearVelocity = direction * dashForce;

        // 4. 維持衝刺狀態直到時間結束
        yield return new WaitForSeconds(dashDuration);

        // 5. ✨ 衝刺時間結束！把原本的高阻力還給它，讓它觸發輪胎摩擦，瞬間煞死煞車！
        rb.linearDamping = originalDrag;
        rb.linearVelocity = Vector2.zero;

        isDashing = false;
    }
}