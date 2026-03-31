using UnityEngine;
using System.Collections;

public class BatAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float moveSpeed = 3f;      // 追逐速度
    public float stoppingDistance = 5f; // ✨ 關鍵：距離玩家多少時停下 (遠程攻擊距離)

    [Header("左右搖擺設定")]
    public float swaySpeed = 2f;      // 搖擺的頻率
    public float swayAmount = 1.5f;   // 搖擺的幅度
    private float swayTimer;

    [Header("攻擊設定")]
    public GameObject projectilePrefab; // 子彈 Prefab
    public Transform firePoint;         // 子彈生成點
    public float fireRate = 2f;         // 幾秒射一次
    private float fireTimer;

    void Start()
    {
        // 自動找玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 隨機初始搖擺時間，避免所有蝙蝠動作同步
        swayTimer = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (player == null) return;

        // 1. 計算與玩家的距離
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. 行為樹邏輯
        if (distanceToPlayer > stoppingDistance)
        {
            // 🛑 狀態 A：距離太遠，飛向玩家
            MoveTowardsPlayer();
        }
        else
        {
            // 🎯 狀態 B：距離夠了，停下並開始搖擺與射擊
            SwayMovement();
            HandleShooting();
        }
    }

    // 狀態 A：直線飛向玩家
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // (選做) 處理圖片水平翻轉，面向玩家
        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // 狀態 B：在原地左右搖擺 (使用 Sin 波)
    void SwayMovement()
    {
        swayTimer += Time.deltaTime;

        // 計算垂直於玩家方向的向量 (用來做水平搖擺)
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        // 計算正交向量 (垂直 dirToPlayer)
        // (x, y) 的垂直向量是 (-y, x)
        Vector2 perpendicularDir = new Vector2(-dirToPlayer.y, dirToPlayer.x);

        // 使用 Sin 波計算搖擺偏移量
        float swayOffset = Mathf.Sin(swayTimer * swaySpeed) * swayAmount;

        // 應用搖擺移動 (只在 perpendicularDir 方向移動)
        transform.Translate(perpendicularDir * swayOffset * Time.deltaTime, Space.World);
    }

    // 狀態 B：定時射擊
    void HandleShooting()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f; // 重置 CD
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // 1. 先抓取蝙蝠自己身上的 EnemyDamage 腳本
        EnemyDamage myDmg = GetComponent<EnemyDamage>();
        int batDmgValue = (myDmg != null) ? myDmg.damage : 5; // 如果沒掛腳本，預設給 5

        // 2. 生成子彈
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 3. 計算方向
        Vector2 fireDirection = (player.position - firePoint.position).normalized;

        // 4. 初始化子彈時，把蝙蝠的傷害值傳進去！
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(fireDirection, batDmgValue);
        }
    }

    // (選做) 在 Unity 編輯器裡畫出攻擊範圍，方便除錯
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}