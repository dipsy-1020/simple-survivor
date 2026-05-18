using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;

    [Header("✨ 同屏數量上限")]
    public int maxNormal = 150;  // 滿畫面普通怪
    public int maxElite = 15;    // 最多同時 15 隻菁英怪
    public int maxBoss = 2;      // 理論上一次只會有一隻，設 2 防呆

    [HideInInspector] public int currentNormal = 0;
    [HideInInspector] public int currentElite = 0;
    [HideInInspector] public int currentBoss = 0;

    [Header("生成設定")]
    public GameObject baseMonsterPrefab;
    public float spawnInterval = 2f;
    private float spawnTimer;
    public Transform[] spawnPoints;

    [Header("當前怪物陣容 (卡池)")]
    public List<MonsterData> activeMonsterRoster = new List<MonsterData>();
    public MonsterData initialMonster;

    private GameObject activeBoss;

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
        if (baseMonsterPrefab == null || activeMonsterRoster.Count == 0) return;

        // ✨ 處理 Boss 強制降臨
        if (finalBossToSpawn != null && !finalBossHasSpawned)
        {
            SpawnSpecificEnemy(finalBossToSpawn);
            finalBossHasSpawned = true;
        }

        float currentSpawnInterval = spawnInterval;
        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;
            currentSpawnInterval = Mathf.Max(0.03f, spawnInterval - ((currentWave - 1) * 0.25f));
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

    // ==========================================
    // ✨ 補上遺漏的方法：讓 UpgradeManager 可以呼叫
    // ==========================================
    public void RegisterFinalBoss(MonsterData bossData)
    {
        finalBossToSpawn = bossData;
        finalBossHasSpawned = false;
    }

    void SpawnSpecificEnemy(MonsterData chosenData)
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(baseMonsterPrefab, sp.position, Quaternion.identity);

        SpriteRenderer sr = newEnemy.GetComponent<SpriteRenderer>();
        if (sr != null && chosenData.monsterSprite != null)
        {
            sr.sprite = chosenData.monsterSprite;
            sr.color = chosenData.monsterColor;
        }
        newEnemy.transform.localScale = Vector3.one * chosenData.scaleMultiplier;

        // ==========================================
        // ✨ 陡峭難度膨脹 (高壓版)
        // ==========================================
        float healthMultiplier = 1f;
        float speedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;

            // 血量：每波增加 40% (到了第 10 波，怪物血量會是原本的 4.6 倍！)
            // 速度：每波增加 5% (讓怪物後期像瘋狗一樣黏上來)
            healthMultiplier = 1f + ((currentWave - 1) * 0.40f);
            speedMultiplier = 1f + ((currentWave - 1) * 0.05f);
        }

        EnemyHealth health = newEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            // ✨ 記錄階級，並增加人口計數器
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
                // ✨ 把食譜裡的衝刺參數灌給大腦！
                spider.chargeDistance = chosenData.chargeDistance;
                spider.prepTime = chosenData.chargePrepTime;
                spider.cooldown = chosenData.chargeCooldown;
                break;

            case MonsterData.AIType.BatKite:
                BatAI bat = newEnemy.AddComponent<BatAI>();
                bat.moveSpeed = finalMoveSpeed;
                // ✨ 把食譜裡的遠程參數灌給大腦！
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
                    Destroy(bat); // 移除蝙蝠大腦，換上 Boss 大腦
                    BossAI bossBrain = newEnemy.AddComponent<BossAI>();
                    bossBrain.moveSpeed = finalMoveSpeed;
                    // ✨ 把食譜裡的 Boss 專屬參數灌給大腦！
                    bossBrain.attackInterval = chosenData.bossAttackInterval;
                    bossBrain.bulletDamage = chosenData.bossBulletDamage;
                }
                break;
        }
    }
}