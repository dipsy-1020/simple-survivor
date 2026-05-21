using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelConfig
{
    public string levelName;
    public float levelDuration;

    [Header("普通怪設定")]
    public float spawnInterval;
    public List<GameObject> enemyPool;

    [Header("菁英怪設定 (選填)")]
    public float eliteSpawnInterval = 15f; // 例如每 15 秒才出一隻菁英怪
    public List<GameObject> elitePool;     // 把過氣的 Boss 拖進這個池子裡
    [HideInInspector] public float eliteTimer; // 內部計時用

    [Header("Boss 設定 (選填)")]
    public GameObject bossPrefab;
    public float bossSpawnTime;
    [HideInInspector] public bool bossSpawned = false;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("所有關卡資料庫")]
    public List<LevelConfig> levels;
    private int currentLevelIndex = 0;

    [Header("生怪優化設定")]
    public Vector2 fieldMin;
    public Vector2 fieldMax;
    public float safeRadius = 6f;       // 絕對不會生在距離玩家 6 單位以內的範圍
    public float minSpawnInterval = 0.4f; // 生怪極限速度 (防止加速到最後當機)

    private float spawnTimer;
    private float levelTimer;
    private bool isLevelFinished = false;

    private Transform playerTransform;

    void Start()
    {
        // 自動抓取場上的玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count) currentLevelIndex = 0;

        Debug.Log($"🚀 載入關卡設定：{levels[currentLevelIndex].levelName}");
    }

    void Update()
    {
        if (levels.Count == 0 || isLevelFinished) return;

        LevelConfig currentLevel = levels[currentLevelIndex];
        levelTimer += Time.deltaTime;

        // 1. Boss 生成邏輯
        if (currentLevel.bossPrefab != null && !currentLevel.bossSpawned && levelTimer >= currentLevel.bossSpawnTime)
        {
            SpawnEntity(currentLevel.bossPrefab);
            currentLevel.bossSpawned = true;
        }

        // 2. 判斷勝利
        if (levelTimer >= currentLevel.levelDuration)
        {
            LevelComplete();
            return;
        }

        // 3. 菁英怪生成邏輯 (獨立冷卻時間)
        if (currentLevel.elitePool != null && currentLevel.elitePool.Count > 0)
        {
            currentLevel.eliteTimer += Time.deltaTime;
            if (currentLevel.eliteTimer >= currentLevel.eliteSpawnInterval)
            {
                GameObject eliteToSpawn = currentLevel.elitePool[Random.Range(0, currentLevel.elitePool.Count)];
                SpawnEntity(eliteToSpawn);
                currentLevel.eliteTimer = 0f; // 重置菁英計時器
            }
        }

        // 4. 普通小怪生成邏輯 (溫和加速機制)
        float timeScale = levelTimer / currentLevel.levelDuration;
        float currentDynamicInterval = Mathf.Lerp(currentLevel.spawnInterval, minSpawnInterval, timeScale);

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentDynamicInterval)
        {
            if (currentLevel.enemyPool.Count > 0)
            {
                GameObject enemyToSpawn = currentLevel.enemyPool[Random.Range(0, currentLevel.enemyPool.Count)];
                SpawnEntity(enemyToSpawn);
            }
            spawnTimer = 0f;
        }
    }

    void SpawnEntity(GameObject prefab)
    {
        if (prefab == null || playerTransform == null) return;

        Vector2 spawnPos = Vector2.zero;
        bool validPos = false;
        int attempts = 0;

        // 嘗試尋找安全地點 (最多試 15 次防止無窮迴圈當機)
        while (!validPos && attempts < 15)
        {
            float randomX = Random.Range(fieldMin.x, fieldMax.x);
            float randomY = Random.Range(fieldMin.y, fieldMax.y);
            spawnPos = new Vector2(randomX, randomY);

            // 檢查是否離玩家太近
            if (Vector2.Distance(spawnPos, playerTransform.position) >= safeRadius)
            {
                validPos = true;
            }
            attempts++;
        }

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    void LevelComplete()
    {
        isLevelFinished = true;
        if (GameManager.instance != null) GameManager.instance.ShowVictory();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((fieldMin.x + fieldMax.x) / 2, (fieldMin.y + fieldMax.y) / 2, 0);
        Vector3 size = new Vector3(fieldMax.x - fieldMin.x, fieldMax.y - fieldMin.y, 1);
        Gizmos.DrawWireCube(center, size);
    }
}