using UnityEngine;
using System.Collections;

public class DashSkill : MonoBehaviour
{
    [Header("技能參數")]
    public float dashForce = 20f;      // 衝撞的爆發力道
    public float dashDuration = 0.3f;  // 衝撞持續時間
    public float prepTime = 0.5f;      // 施放前的蓄力警告時間
    public float cooldown = 5f;        // 技能冷卻時間

    private bool isDashing = false;
    private bool isCooldown = false;
    private Rigidbody2D rb;
    private MonoBehaviour defaultMovement;

    // ✨ 這裡就是剛剛報錯的地方：宣告材質球變數必須放在這裡！
    private Material myMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 抓取 Sprite Renderer 身上的材質球
        if (GetComponent<SpriteRenderer>() != null)
        {
            myMaterial = GetComponent<SpriteRenderer>().material;
        }

        // 使用現代泛型寫法抓取移動腳本
        if (GetComponent<EnemyController>() != null)
            defaultMovement = GetComponent<EnemyController>();

        if (GetComponent<PlayerController>() != null)
            defaultMovement = GetComponent<PlayerController>();
    }

    public void TryDash(Vector3 targetPosition)
    {
        if (!isDashing && !isCooldown)
        {
            StartCoroutine(DashRoutine(targetPosition));
        }
    }

    IEnumerator DashRoutine(Vector3 targetPosition)
    {
        isDashing = true;
        isCooldown = true;

        // 1. 準備階段：暫停移動，原地煞車
        if (defaultMovement != null) defaultMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // ✨ 開啟純白閃爍 (蓄力警告！)
        if (myMaterial != null) myMaterial.SetFloat("_FlashAmount", 1.0f);

        Vector2 dashDirection = (targetPosition - transform.position).normalized;

        // 等待蓄力時間
        yield return new WaitForSeconds(prepTime);

        // ✨ 衝撞開始，把材質球調回原色
        if (myMaterial != null) myMaterial.SetFloat("_FlashAmount", 0.0f);

        // 2. 爆發階段
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashDuration);

        // 3. 結束階段
        rb.linearVelocity = Vector2.zero;
        if (defaultMovement != null) defaultMovement.enabled = true;
        isDashing = false;

        // 4. 冷卻階段
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }
}