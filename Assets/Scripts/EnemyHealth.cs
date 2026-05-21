using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("血量設定")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("掉落與回饋設定")]
    public GameObject expGemPrefab;
    public GameObject damageTextPrefab;

    [Header("補血道具掉落")]
    public GameObject healItemPrefab;   // 拖入你的補血道具 Prefab
    public float healDropChance = 0.05f; // 5% 機率掉落補血

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
        // 隨機骰一個 0.0 ~ 1.0 的數字，判斷是否掉落補血
        if (healItemPrefab != null && Random.value <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
        else if (expGemPrefab != null)
        {
            Instantiate(expGemPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}