using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeleeAttackModule))] // ✨ 綁定近戰模組
public class SpiderAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float normalSpeed = 2f;
    public float chargeDistance = 6f; // 距離多近時開始蓄力

    [Header("技能時間軸")]
    public float prepTime = 0.5f;     // 施放前的蓄力警告時間
    public float cooldown = 3f;       // 衝撞完的喘息時間

    private MeleeAttackModule meleeModule;
    private bool isAttacking = false; // 是否正在走攻擊流程

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 抓取身上的近戰模組
        meleeModule = GetComponent<MeleeAttackModule>();
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chargeDistance)
        {
            // 進入攻擊範圍，大腦開始執行攻擊狀態機
            StartCoroutine(AttackStateRoutine());
        }
        else
        {
            // 距離不夠，繼續緩慢追擊玩家
            transform.position = Vector2.MoveTowards(transform.position, player.position, normalSpeed * Time.deltaTime);
        }
    }

    IEnumerator AttackStateRoutine()
    {
        isAttacking = true;

        // 1. 停頓蓄力 (給玩家預警)
        // 這裡可以加上 Sprite 閃爍變紅的視覺提示
        yield return new WaitForSeconds(prepTime);

        if (player != null)
        {
            // 2. 鎖定方向，大腦下令突進！
            Vector2 dashDirection = (player.position - transform.position).normalized;
            meleeModule.Attack(dashDirection);
        }

        // 3. 原地喘息冷卻
        // 等待衝刺時間加上額外的冷卻時間
        yield return new WaitForSeconds(cooldown);

        isAttacking = false;
    }
}