using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float moveSpeed = 1.5f;    // Boss 平常的移動速度 (偏慢，給玩家壓迫感)

    [Header("招式一：八方彈幕")]
    public GameObject projectilePrefab; // 直接共用蝙蝠的那個音波子彈！
    public float shootCooldown = 5f;    // 每 5 秒放一次彈幕
    private float shootTimer;

    [Header("招式二：召喚眷屬")]
    public GameObject minionPrefab;     // 拖入你的蜘蛛或蝙蝠 Prefab
    public float summonCooldown = 8f;   // 每 8 秒召喚一次小怪
    private float summonTimer;

    private bool isAttacking = false;   // 判斷 Boss 是否正在放技能 (施法時不動)

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 如果 Boss 正在放技能，就先不要執行移動和計時
        if (isAttacking) return;

        // 1. 緩慢無情地逼近玩家
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // ✨ 新增：Boss 轉向邏輯
        // 判斷玩家在 Boss 的右邊還是左邊
        if (player.position.x > transform.position.x)
        {
            // 玩家在右邊：面向右
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (player.position.x < transform.position.x)
        {
            // 玩家在左邊：面向左
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // 2. 技能計時器
        shootTimer += Time.deltaTime;
        summonTimer += Time.deltaTime;

        // 3. 判斷是否該放招了 (優先判定召喚，再判定射擊)
        if (summonTimer >= summonCooldown)
        {
            StartCoroutine(SummonRoutine());
            summonTimer = 0f; // 重置冷卻
        }
        else if (shootTimer >= shootCooldown)
        {
            StartCoroutine(ShootRingRoutine());
            shootTimer = 0f; // 重置冷卻
        }
    }

    // --- 招式一邏輯：環形彈幕 ---
    IEnumerator ShootRingRoutine()
    {
        isAttacking = true; // 鎖定狀態，Boss 停下腳步

        // 施法前搖 (稍微停頓 0.5 秒讓玩家有預警)
        yield return new WaitForSeconds(0.5f);

        Debug.Log("💀 Boss 釋放八方彈幕！");
        int bulletCount = 8; // 發射 8 顆子彈
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            // 計算 8 個方向的角度
            float angle = i * angleStep;
            Vector2 bulletDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            // 生成子彈
            GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // 呼叫我們之前寫好的 Initialize 給子彈方向
            EnemyProjectile projScript = bullet.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                // 抓取 Boss 自身的攻擊力 (假設 Boss 也有掛 EnemyDamage)
                EnemyDamage bossDmg = GetComponent<EnemyDamage>();
                int damageValue = (bossDmg != null) ? bossDmg.damage : 20; // 預設給 20 滴血

                // ✨ 把方向跟傷害值一起傳進去！
                projScript.Initialize(bulletDir, damageValue);
            }
        }

        // 施法後搖 (放完招式後休息 0.5 秒)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false; // 解除鎖定，繼續追人
    }

    // --- 招式二邏輯：召喚眷屬 ---
    IEnumerator SummonRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);

        Debug.Log("💀 Boss 召喚了小怪！");

        // 在 Boss 身旁隨機位置召喚 3 隻小怪
        for (int i = 0; i < 3; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f; // 半徑 2 內的隨機點
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
}