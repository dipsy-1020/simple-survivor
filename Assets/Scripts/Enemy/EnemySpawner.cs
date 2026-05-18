using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;

    [Header("✨ 地圖邊界限制 (防止怪生在牆外)")]
    public bool clampToMap = true;
    public Vector2 minMapBounds = new Vector2(-20f, -20f); // 根據你實際的地圖大小填寫
    public Vector2 maxMapBounds = new Vector2(20f, 20f);

    [Header("✨ 同屏數量上限")]
    public int maxNormal = 80;  // ✨ 下調數量
    public int maxElite = 10;
    public int maxBoss = 5;

    [HideInInspector] public int currentNormal = 0;
    [HideInInspector] public int currentElite = 0;
    [HideInInspector] public int currentBoss = 0;

    [Header("生成設定")]
    public GameObject baseMonsterPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 15f; // ✨ 離玩家多遠生成 (設在畫面外邊緣)
    private float spawnTimer;

    [Header("當前怪物陣容 (卡池)")]
    public List<MonsterData> activeMonsterRoster = new List<MonsterData>();
    public MonsterData initialMonster;

    private GameObject activeBoss;

    private List<MonsterData> bossesToSpawn = new List<MonsterData>();

    // ==========================================
    // ✨ 補上遺漏的變數：用來記錄要強制降臨的 Boss
    // ==========================================
    private MonsterData finalBossToSpawn;
    private bool finalBossHasSpawned = false;

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        if (initialMonster != null && activeMonsterRoster.Count == 0)
        {
            activeMonsterRoster.Add(initialMonster);
        }
    }

    void Update()
    {
        if (baseMonsterPrefab == null) return;

        // ✨ 處理 Boss 降臨 (不再限制只能有一隻)
        if (bossesToSpawn.Count > 0)
        {
            foreach (var boss in bossesToSpawn) SpawnSpecificEnemy(boss);
            bossesToSpawn.Clear();
        }

        if (activeMonsterRoster.Count == 0) return;

        // ==========================================
        // ✨ 補回遺失的程式碼：波次難度縮減與計時器
        // ==========================================
        float currentSpawnInterval = spawnInterval;
        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;
            int maxWaves = GameManager.instance.maxWaves; // 總波次 (10波)

            // ✨ 完美線性流暢加速公式：
            // 每升一波，就穩定減少「固定比例」的生怪間隔，直到最後一波達到最極限
            // 假設 spawnInterval 是 2f，到了第 10 波會被壓到 0.2f 左右（數值你可以根據體感微調）
            float reductionPerWave = 0.15f;
            currentSpawnInterval = Mathf.Max(0.05f, spawnInterval - ((currentWave - 1) * reductionPerWave));
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            MonsterData chosenData = activeMonsterRoster[Random.Range(0, activeMonsterRoster.Count)];

            // ✨ 人口普查：如果抽到的怪物階級已經達到上限，這幀就跳過不生！
            if (chosenData.tier == MonsterData.MonsterTier.Normal && currentNormal >= maxNormal) return;
            if (chosenData.tier == MonsterData.MonsterTier.Elite && currentElite >= maxElite) return;
            if (chosenData.tier == MonsterData.MonsterTier.Boss && currentBoss >= maxBoss) return;

            SpawnSpecificEnemy(chosenData);
            spawnTimer = 0f; // 只有成功生出怪物，才重置計時器
        }
    }

    public void RegisterBoss(MonsterData bossData)
    {
        bossesToSpawn.Add(bossData);
    }

    void SpawnSpecificEnemy(MonsterData chosenData)
    {
        // 1. 動態生成位置
        Vector3 spawnPos = Vector3.zero;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            spawnPos = playerObj.transform.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

            // ✨ 新增：如果開啟邊界限制，強制把生成點卡在牆內！
            if (clampToMap)
            {
                spawnPos.x = Mathf.Clamp(spawnPos.x, minMapBounds.x, maxMapBounds.x);
                spawnPos.y = Mathf.Clamp(spawnPos.y, minMapBounds.y, maxMapBounds.y);
            }
        }

        // 2. 生成怪物實體
        GameObject newEnemy = Instantiate(baseMonsterPrefab, spawnPos, Quaternion.identity);

        // ==========================================
        // ✨ 以下是被你不小心刪掉的外觀與 AI 綁定代碼，我全補回來了！
        // ==========================================
        SpriteRenderer sr = newEnemy.GetComponent<SpriteRenderer>();
        if (sr != null && chosenData.monsterSprite != null)
        {
            sr.sprite = chosenData.monsterSprite;
            sr.color = chosenData.monsterColor;
        }
        newEnemy.transform.localScale = Vector3.one * chosenData.scaleMultiplier;

        float healthMultiplier = 1f;
        float speedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;
            healthMultiplier = 1f + ((currentWave - 1) * 0.40f);
            speedMultiplier = 1f + ((currentWave - 1) * 0.05f);
        }

        EnemyHealth health = newEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.myTier = chosenData.tier;
            if (chosenData.tier == MonsterData.MonsterTier.Normal) currentNormal++;
            else if (chosenData.tier == MonsterData.MonsterTier.Elite) currentElite++;
            else if (chosenData.tier == MonsterData.MonsterTier.Boss) currentBoss++;

            health.maxHealth = Mathf.RoundToInt(chosenData.maxHealth * healthMultiplier);
            health.currentHealth = health.maxHealth;

            if (chosenData.tier == MonsterData.MonsterTier.Boss)
            {
                health.isBoss = true;
            }
        }

        float finalMoveSpeed = chosenData.moveSpeed * speedMultiplier;

        switch (chosenData.brainType)
        {
            case MonsterData.AIType.StraightChaser:
                EnemyController ec = newEnemy.AddComponent<EnemyController>();
                ec.moveSpeed = finalMoveSpeed;
                if (chosenData.useMeleeAttack) newEnemy.AddComponent<MeleeAttackModule>();
                break;

            case MonsterData.AIType.SpiderCharge:
                SpiderAI spider = newEnemy.AddComponent<SpiderAI>();
                spider.normalSpeed = finalMoveSpeed;
                spider.chargeDistance = chosenData.chargeDistance;
                spider.prepTime = chosenData.chargePrepTime;
                spider.cooldown = chosenData.chargeCooldown;
                break;

            case MonsterData.AIType.BatKite:
                BatAI bat = newEnemy.AddComponent<BatAI>();
                bat.moveSpeed = finalMoveSpeed;
                bat.stoppingDistance = chosenData.stoppingDistance;
                bat.fireRate = chosenData.fireRate;
                bat.attackDamage = chosenData.rangedAttackDamage;

                ProjectileAttackModule pam = newEnemy.GetComponent<ProjectileAttackModule>();
                if (pam != null)
                {
                    pam.projectilePrefab = chosenData.projectilePrefab;
                    pam.firePoint = newEnemy.transform;
                }

                if (chosenData.tier == MonsterData.MonsterTier.Boss)
                {
                    Destroy(bat);
                    BossAI bossBrain = newEnemy.AddComponent<BossAI>();
                    bossBrain.moveSpeed = finalMoveSpeed;
                    bossBrain.attackInterval = chosenData.bossAttackInterval;
                    bossBrain.bulletDamage = chosenData.bossBulletDamage;
                }
                break;
        }
    }
}