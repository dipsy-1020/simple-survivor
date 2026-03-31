using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("怪物預製物清單")]
    public GameObject basicEnemy; // 普通小怪
    public GameObject eliteEnemy; // 紫色菁英怪 (稍微硬一點、數量少)
    public GameObject BatEnemy;  // 蝙蝠
    public GameObject bossPrefab; // 紅色大 Boss

    [Header("生成範圍設定")]
    public float minMapX = -20f;
    public float maxMapX = 20f;
    public float minMapY = -20f;
    public float maxMapY = 20f;
    public float spawnRadius = 15f;

    [Header("關卡時間軸 (秒)")]
    public float phase2Time = 180f; // 第 3 分鐘：壓力開始增加
    public float phase3Time = 360f; // 第 6 分鐘：菁英與衝刺怪加入
    public float bossTime = 540f;   // 第 9 分鐘：Boss 降臨

    private float timer = 0f;
    private bool bossSpawned = false;
    private Transform player;
    private GameManager gameManager;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (player == null || gameManager == null) return;

        float currentTime = gameManager.gameTime;

        // --- 🏆 終極階段：Boss 降臨判定 ---
        if (currentTime >= bossTime && !bossSpawned)
        {
            SpawnBoss();
            return; // 讓這一幀先去生 Boss
        }

        // --- ⏳ 日常生成階段 ---
        timer += Time.deltaTime;

        // 取得當下的生怪間隔時間 (越後面越短)
        float currentInterval = GetSpawnInterval(currentTime);

        if (timer >= currentInterval)
        {
            SpawnNormalEnemies(currentTime);
            timer = 0f;
        }
    }

    // 核心魔法 1：根據時間，平滑縮短生怪間隔 (使用 Mathf.Lerp)
    float GetSpawnInterval(float time)
    {
        // 0 ~ 3 分鐘：間隔從 1.5 秒縮短到 1.0 秒
        if (time < phase2Time) return Mathf.Lerp(1.5f, 1.0f, time / phase2Time);

        // 3 ~ 6 分鐘：間隔從 1.0 秒縮短到 0.5 秒 (怪開始變多)
        if (time < phase3Time) return Mathf.Lerp(1.0f, 0.5f, (time - phase2Time) / (phase3Time - phase2Time));

        // 6 ~ 10 分鐘：間隔從 0.5 秒縮短到 0.2 秒 (怪海成型！)
        if (time < bossTime) return Mathf.Lerp(0.5f, 0.2f, (time - phase3Time) / (bossTime - phase3Time));

        // Boss 戰期間：稍微放緩生怪速度 (1.5秒一隻)，讓玩家專心單挑 Boss
        return 1.5f;
    }

    // 核心魔法 2：根據時間，決定抽出哪一種怪
    void SpawnNormalEnemies(float time)
    {
        GameObject enemyToSpawn = basicEnemy; // 預設都是普通小怪

        // 第 3 分鐘後：20% 機率混入菁英怪
        if (time >= phase2Time && time < phase3Time)
        {
            if (Random.value < 0.2f) enemyToSpawn = eliteEnemy;
        }
        // 第 6 分鐘後：增加會衝刺的怪物
        else if (time >= phase3Time)
        {
            float rand = Random.value;
            if (rand < 0.1f) enemyToSpawn = BatEnemy; // 10% 衝刺怪
            else if (rand < 0.3f) enemyToSpawn = eliteEnemy; // 20% 菁英怪
        }

        if (enemyToSpawn != null) InstantiateEnemy(enemyToSpawn);
    }

    void InstantiateEnemy(GameObject prefab)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = player.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

        // 限制在牆壁內
        spawnPos.x = Mathf.Clamp(spawnPos.x, minMapX, maxMapX);
        spawnPos.y = Mathf.Clamp(spawnPos.y, minMapY, maxMapY);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        InstantiateEnemy(bossPrefab);
        Debug.Log("⚠️ 10分鐘已到，終極 Boss 降臨！");
    }
}