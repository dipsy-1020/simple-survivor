using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;

    [Header("生成設定")]
    public GameObject baseMonsterPrefab;
    public float spawnInterval = 2f;
    private float spawnTimer;
    public Transform[] spawnPoints;

    [Header("當前怪物陣容 (卡池)")]
    public List<MonsterData> activeMonsterRoster = new List<MonsterData>();
    public MonsterData initialMonster;

    // ✨ 新增：用來記錄場上是不是已經有 Boss 了，避免每 2 秒生一隻
    private GameObject activeBoss;

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

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            MonsterData chosenData = activeMonsterRoster[Random.Range(0, activeMonsterRoster.Count)];

            // ✨ 防呆：如果抽卡池抽到 Boss，但場上已經有一隻 Boss 活著了，就取消這次生成
            if (chosenData.tier == MonsterData.MonsterTier.Boss && activeBoss != null)
            {
                return; // 等下一秒再隨機抽別的小怪
            }

            SpawnSpecificEnemy(chosenData);
            spawnTimer = 0f;
        }
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
        // ✨ 動態難度膨脹 (溫和版)
        // 血量：每波增加 10% / 速度：每波增加 2%
        // ==========================================
        float healthMultiplier = 1f;
        float speedMultiplier = 1f;

        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;
            healthMultiplier = 1f + ((currentWave - 1) * 0.10f);
            speedMultiplier = 1f + ((currentWave - 1) * 0.02f);
        }

        EnemyHealth health = newEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.maxHealth = Mathf.RoundToInt(chosenData.maxHealth * healthMultiplier);
            health.currentHealth = health.maxHealth;

            if (chosenData.tier == MonsterData.MonsterTier.Boss)
            {
                health.isBoss = true;
                activeBoss = newEnemy; // ✨ 記錄這隻就是目前的 Boss！
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
                break;

            case MonsterData.AIType.BatKite:
                BatAI bat = newEnemy.AddComponent<BatAI>();
                bat.moveSpeed = finalMoveSpeed;

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
                }
                break;
        }
    }
}