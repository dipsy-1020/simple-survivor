using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ProjectileAttackModule))]
public class BossAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float moveSpeed = 1.5f;

    [Header("攻擊設定")]
    public int bulletDamage = 20;
    public float attackInterval = 4f; // 每幾秒放一次招

    private ProjectileAttackModule attackModule;
    private bool isAttacking = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        attackModule = GetComponent<ProjectileAttackModule>();

        // 開始 Boss 的攻擊循環
        StartCoroutine(BossRoutine());
    }

    void Update()
    {
        if (player == null) return;

        // 自動翻轉面向玩家
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);

        // 如果沒在攻擊施法中，就緩慢逼近玩家
        if (!isAttacking)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator BossRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            yield return StartCoroutine(EightWayShootRoutine());
        }
    }

    IEnumerator EightWayShootRoutine()
    {
        isAttacking = true;

        // 施法前搖：停頓一下給玩家預警
        yield return new WaitForSeconds(0.5f);

        // 八方彈幕數學運算
        int projectiles = 8;
        float angleStep = 360f / projectiles;
        float angle = 0f;

        for (int i = 0; i < projectiles; i++)
        {
            // 利用三角函數計算出 8 個方向的 X, Y 向量
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY);

            // ✨ 大腦下令開火！
            attackModule.Fire(direction, bulletDamage, "Player");

            angle += angleStep;
        }

        // 施法後搖
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
}