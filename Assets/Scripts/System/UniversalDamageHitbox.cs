using UnityEngine;

public class UniversalDamageHitbox : MonoBehaviour
{
    [Header("傷害設定")]
    public int damage = 10;
    public GameObject damageTextPrefab;
    public string targetTag = "Player";
    public bool destroyOnHit = false;

    public int pierceCount = 0;
    public int bounceCount = 0; // ✨ 新增：彈射次數

    private void OnTriggerEnter2D(Collider2D other) { DealDamage(other.gameObject); }
    private void OnCollisionEnter2D(Collision2D collision) { DealDamage(collision.gameObject); }

    private void DealDamage(GameObject targetObj)
    {
        if (targetObj.CompareTag(targetTag))
        {
            if (targetTag == "Player")
            {
                PlayerHealth playerHp = targetObj.GetComponent<PlayerHealth>();
                if (playerHp != null) { playerHp.TakeDamage(damage); HitResolution(targetObj); }
            }
            else if (targetTag == "Enemy")
            {
                EnemyHealth enemyHp = targetObj.GetComponent<EnemyHealth>();
                if (enemyHp != null)
                {
                    enemyHp.TakeDamage(damage);
                    if (damageTextPrefab != null)
                    {
                        GameObject textObj = Instantiate(damageTextPrefab, targetObj.transform.position, Quaternion.identity);
                        DamageText dmgText = textObj.GetComponent<DamageText>();
                        if (dmgText != null) dmgText.Setup(damage);
                    }
                    HitResolution(targetObj);
                }
            }
        }
    }

    private void HitResolution(GameObject hitObj)
    {
        if (destroyOnHit)
        {
            // ✨ 穿透優先！
            if (pierceCount > 0)
            {
                pierceCount--;
            }
            // ✨ 穿透沒了，才檢查彈射！
            else if (bounceCount > 0)
            {
                bounceCount--;
                EnemyProjectile proj = GetComponent<EnemyProjectile>();
                if (proj != null) proj.BounceToNearestEnemy(hitObj); // 呼叫彈射轉向
            }
            else
            {
                Destroy(gameObject); // 兩者都沒了，安息吧
            }
        }
    }
}