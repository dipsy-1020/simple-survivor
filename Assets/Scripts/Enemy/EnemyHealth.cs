using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("身份設定")]
    public bool isBoss = false;
    // ✨ 新增：記錄自己的階級，方便死掉時扣除計數
    [HideInInspector] public MonsterData.MonsterTier myTier;
    private bool isShuttingDown = false; // 防呆：避免關閉遊戲時報錯

    [Header("敵人血量設定")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("防雙判無敵時間")]
    public float invincibilityDuration = 0.2f; // ✨ 核心修復：0.2 秒內不會受到重複傷害
    private float invincibilityTimer = 0f;

    [Header("掉落物設定")]
    public GameObject gemPrefab;
    public Gem.GemTier dropTier = Gem.GemTier.Small;
    public int gemDropCount = 1;
    public float scatterRadius = 0.8f;

    [Header("視覺特效設定")]
    public GameObject deathEffectPrefab;

    private DamageFlash damageFlash;

    void Start()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<DamageFlash>();
    }

    // ✨ 新增 Update 來倒數無敵時間
    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        // ✨ 核心修復：如果還在無敵時間內，直接跳出，拒絕雙判！
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration; // 刷新無敵時間

        if (damageFlash != null) damageFlash.CallFlash();

        if (AudioManager.instance != null) AudioManager.instance.PlayHitSound();

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (gemPrefab != null)
        {
            for (int i = 0; i < gemDropCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
                Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                GameObject droppedGem = Instantiate(gemPrefab, dropPosition, Quaternion.identity);
                Gem gemScript = droppedGem.GetComponent<Gem>();
                if (gemScript != null)
                {
                    gemScript.Initialize(dropTier);
                }
            }
        }

        Destroy(gameObject);

        if (isBoss)
        {
            if (GameManager.instance != null) GameManager.instance.Victory();
        }
    }

    // ✨ 新增這兩個方法在腳本最底下
    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    void OnDestroy()
    {
        if (isShuttingDown || EnemySpawner.instance == null) return;

        // 當怪物死亡或被波次清場時，把自己的數量從生成器中扣除
        if (myTier == MonsterData.MonsterTier.Normal) EnemySpawner.instance.currentNormal--;
        else if (myTier == MonsterData.MonsterTier.Elite) EnemySpawner.instance.currentElite--;
        else if (myTier == MonsterData.MonsterTier.Boss) EnemySpawner.instance.currentBoss--;
    }
}