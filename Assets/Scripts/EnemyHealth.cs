using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("掉落與回饋設定")]
    public GameObject expGemPrefab;
    public int expDropAmount = 10;      // 新增：這隻怪掉落多少經驗值？ (可以在 Unity 裡單獨調)
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
        if (healItemPrefab != null && Random.value <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
        else if (expGemPrefab != null)
        {
            // 生成寶石，並把個別的經驗值傳給寶石
            GameObject gem = Instantiate(expGemPrefab, transform.position, Quaternion.identity);
            ExpGem gemScript = gem.GetComponent<ExpGem>();
            if (gemScript != null)
            {
                gemScript.expValue = expDropAmount; // 核心修改！
            }
        }
        Destroy(gameObject);
    }
}