using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("掉落與回饋設定")]
    public GameObject expGemPrefab;
    public int expDropAmount = 10;
    public GameObject damageTextPrefab;

    [Header("補血道具掉落")]
    public GameObject healItemPrefab;
    public float healDropChance = 0.05f;

    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (AudioManager.instance != null) AudioManager.instance.PlayHit();
        if (gameObject.activeInHierarchy) StartCoroutine(DamageFlash());

        if (damageTextPrefab != null)
        {
            GameObject textObj = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
            DamageText dt = textObj.GetComponent<DamageText>();
            if (dt != null) dt.Setup(damage);
        }

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        if (sr != null)
        {
            sr.color = new Color(1f, 0.5f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }

    void Die()
    {
        // 1. 先骰機率決定是否掉落補血道具
        if (healItemPrefab != null && Random.value <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
        // 2. 如果沒掉補血，就正常掉落經驗寶石
        else if (expGemPrefab != null)
        {
            int gemCount = 1;

            // 強制大噴發邏輯：只要經驗值大於 30，每 20 經驗值就多噴一顆寶石 (上限限制在 12 顆避免畫面卡頓)
            if (expDropAmount >= 30)
            {
                gemCount = Mathf.Clamp(expDropAmount / 20, 3, 12);
            }

            int expPerGem = expDropAmount / gemCount;
            int remainder = expDropAmount % gemCount; // 把除不盡的經驗值保留下來

            for (int i = 0; i < gemCount; i++)
            {
                // 如果掉多顆，給個半徑 2.0 的隨機位置讓它們散開
                Vector3 offset = (gemCount > 1) ? (Vector3)(Random.insideUnitCircle * 2f) : Vector3.zero;
                GameObject gem = Instantiate(expGemPrefab, transform.position + offset, Quaternion.identity);

                ExpGem gemScript = gem.GetComponent<ExpGem>();
                if (gemScript != null)
                {
                    // 把餘數全部加給第一顆寶石，確保總經驗值絕對正確
                    gemScript.expValue = expPerGem + (i == 0 ? remainder : 0);
                }
            }
        }

        Destroy(gameObject);
    }
}