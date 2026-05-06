using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("身份設定")]
    public bool isBoss = false;

    [Header("敵人血量設定")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("掉落物設定")]
    public GameObject gemPrefab;
    // ✨ 新增這行：讓你在 Inspector 可以選擇這隻怪掉哪種寶石
    public Gem.GemTier dropTier = Gem.GemTier.Small;
    public int gemDropCount = 1;
    public float scatterRadius = 0.8f;

    [Header("✨ 新增：視覺特效設定")]
    // ✨ 新增：用來裝我們剛剛捏好的粒子預製物
    public GameObject deathEffectPrefab;

    private DamageFlash damageFlash;

    void Start()
    {
        currentHealth = maxHealth;
        damageFlash = GetComponent<DamageFlash>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (damageFlash != null) damageFlash.CallFlash();

        // ✨ 新增這行：呼叫 AudioManager 播放打擊音效！
        if (AudioManager.instance != null) AudioManager.instance.PlayHitSound();

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // ✨ 新增核心魔法：在怪物死掉的座標，生成死亡粒子特效！
        if (deathEffectPrefab != null)
        {
            // 在怪物的當前位置 (transform.position) 生成粒子
            // 這裡不需要旋轉 (Quaternion.identity)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

            // ⚠️ 註：我們不需要寫 Destroy(particle)，因為我們在粒子系統裡設了 Stop Action = Destroy！
        }
        if (gemPrefab != null)
        {
            for (int i = 0; i < gemDropCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
                Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                // ✨ 把生出來的寶石先存進一個變數裡
                GameObject droppedGem = Instantiate(gemPrefab, dropPosition, Quaternion.identity);

                // ✨ 抓取它身上的 Gem 腳本，並依照我們設定的階級幫它「變身」！
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
            // ✨ 修改這裡：直接呼叫單例
            if (GameManager.instance != null) GameManager.instance.Victory();
        }
    }
}