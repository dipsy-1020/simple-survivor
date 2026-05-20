using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelConfig
{
    public string levelName;
    public float levelDuration;
    public float spawnInterval;
    public List<GameObject> enemyPool;

    [Header("Boss 設定 (選填)")]
    public GameObject bossPrefab;
    public float bossSpawnTime;
    [HideInInspector] public bool bossSpawned = false;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("所有關卡資料庫")]
    public List<LevelConfig> levels;
    // 不再是從 0 遞增，而是鎖定玩家選的那一關
    private int currentLevelIndex = 0;

    [Header("場地生怪邊界")]
    public Vector2 fieldMin;
    public Vector2 fieldMax;

    private float spawnTimer;
    private float levelTimer;
    private bool isLevelFinished = false; // 防止重複觸發勝利

    void Start()
    {
        // 讀取主選單傳過來的編號 (預設為 0 = Easy)
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);

        // 防呆機制：如果傳來的數字超過你設定的關卡數量，就強制玩第一關
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count)
        {
            currentLevelIndex = 0;
        }

        Debug.Log($"🚀 載入關卡設定：{levels[currentLevelIndex].levelName}");
    }

    void Update()
    {
        // 如果沒設定關卡，或是這關已經打完了，就不做事
        if (levels.Count == 0 || isLevelFinished) return;

        // 鎖定當前關卡資料
        LevelConfig currentLevel = levels[currentLevelIndex];
        levelTimer += Time.deltaTime;

        // 1. 處理 Boss 生成
        if (currentLevel.bossPrefab != null && !currentLevel.bossSpawned && levelTimer >= currentLevel.bossSpawnTime)
        {
            SpawnEntity(currentLevel.bossPrefab);
            currentLevel.bossSpawned = true;
            Debug.Log("⚠️ Boss 出現了！");
        }

        // 2. 判斷關卡是否結束 (時間到 = 勝利)
        if (levelTimer >= currentLevel.levelDuration)
        {
            LevelComplete();
            return; // 結束了就直接 return，不要再往下生怪了
        }

        // 3. 處理小怪生成
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentLevel.spawnInterval)
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
        if (prefab == null) return;
        float randomX = Random.Range(fieldMin.x, fieldMax.x);
        float randomY = Random.Range(fieldMin.y, fieldMax.y);
        Instantiate(prefab, new Vector2(randomX, randomY), Quaternion.identity);
    }

    void LevelComplete()
    {
        isLevelFinished = true;
        Debug.Log("🎉 關卡時間結束，恭喜通關！");

        // 呼叫 GameManager 顯示勝利面板
        if (GameManager.instance != null)
        {
            GameManager.instance.ShowVictory();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((fieldMin.x + fieldMax.x) / 2, (fieldMin.y + fieldMax.y) / 2, 0);
        Vector3 size = new Vector3(fieldMax.x - fieldMin.x, fieldMax.y - fieldMin.y, 1);
        Gizmos.DrawWireCube(center, size);
    }
}