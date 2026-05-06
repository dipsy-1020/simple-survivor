using UnityEngine;

[RequireComponent(typeof(ProjectileAttackModule))] // ✨ 系統防呆：掛這腳本時，會自動幫你加上發射模組
public class BatAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float moveSpeed = 3f;
    public float stoppingDistance = 5f; // 停下來射擊的距離

    [Header("左右搖擺設定")]
    public float swaySpeed = 2f;
    public float swayAmount = 1.5f;
    private float swayTimer;

    [Header("攻擊設定")]
    public int attackDamage = 10;
    public float fireRate = 2f; // 幾秒射一次
    private float fireTimer;

    // ✨ 宣告我們剛剛做好的「遠程發射手」
    private ProjectileAttackModule attackModule;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        swayTimer = Random.Range(0f, 10f); // 錯開每隻蝙蝠的搖擺節奏

        // 抓取身上的發射模組
        attackModule = GetComponent<ProjectileAttackModule>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stoppingDistance)
        {
            // 距離不夠，繼續追擊玩家
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // 距離夠了，開始放風箏：原地左右搖擺
            swayTimer += Time.deltaTime;
            float swayOffset = Mathf.Sin(swayTimer * swaySpeed) * swayAmount * Time.deltaTime;
            transform.Translate(new Vector3(swayOffset, 0, 0));

            // 射擊冷卻計時
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    void Shoot()
    {
        // 計算朝向玩家的方向向量
        Vector2 direction = (player.position - transform.position).normalized;

        // ✨ 大腦下令開火！把方向、傷害、要攻擊的標籤 ("Player") 交給模組去處理
        attackModule.Fire(direction, attackDamage, "Player");
    }
}