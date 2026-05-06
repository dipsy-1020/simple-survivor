using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // 單例模式：讓升級系統可以隨時呼叫它塞入新怪物
    public static EnemySpawner instance;

    [Header("生成設定")]
    [Tooltip("這是一個純白色的基礎怪物 Prefab，身上不掛任何大腦(AI)，只掛血量與剛體")]
    public GameObject baseMonsterPrefab;
    public float spawnInterval = 2f;
    private float spawnTimer;
    public Transform[] spawnPoints;

    [Header("當前怪物陣容 (卡池)")]
    // 這裡存放目前波次會出現的所有怪物食譜
    public List<MonsterData> activeMonsterRoster = new List<MonsterData>();

    [Tooltip("第一波的預設基礎怪物食譜，請在面板拖曳進來")]
    public MonsterData initialMonster;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 確保第一波有怪可以生
        if (initialMonster != null && activeMonsterRoster.Count == 0)
        {
            activeMonsterRoster.Add(initialMonster);
        }
    }

    void Update()
    {
        if (activeMonsterRoster.Count == 0 || baseMonsterPrefab == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // 1. 從卡池隨機抽一張食譜
        MonsterData chosenData = activeMonsterRoster[Random.Range(0, activeMonsterRoster.Count)];

        // 2. 隨機選一個生成點
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 3. 生成沒有靈魂的純白色基礎模型
        GameObject newEnemy = Instantiate(baseMonsterPrefab, sp.position, Quaternion.identity);

        // 4. 灌入食譜資料：改顏色與大小
        SpriteRenderer sr = newEnemy.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = chosenData.monsterColor;
        newEnemy.transform.localScale = Vector3.one * chosenData.scaleMultiplier;

        // 寫入血量
        EnemyHealth health = newEnemy.GetComponent<EnemyHealth>();
        if (health != null) health.maxHealth = chosenData.maxHealth;

        // 5. 根據食譜掛載大腦與攻擊模組
        switch (chosenData.brainType)
        {
            case MonsterData.AIType.StraightChaser:
                EnemyController ec = newEnemy.AddComponent<EnemyController>();
                ec.moveSpeed = chosenData.moveSpeed;
                if (chosenData.useMeleeAttack) newEnemy.AddComponent<MeleeAttackModule>();
                break;

            case MonsterData.AIType.SpiderCharge:
                SpiderAI spider = newEnemy.AddComponent<SpiderAI>();
                spider.normalSpeed = chosenData.moveSpeed;
                break;

            case MonsterData.AIType.BatKite:
                BatAI bat = newEnemy.AddComponent<BatAI>();
                bat.moveSpeed = chosenData.moveSpeed;
                // 注意：這裡先預設掛載，之後要讓蝙蝠能射擊，還需要把子彈 Prefab 寫入發射模組
                break;
        }
    }
}