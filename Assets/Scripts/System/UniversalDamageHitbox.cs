using UnityEngine;

public class UniversalDamageHitbox : MonoBehaviour
{
    [Header("傷害設定")]
    public int damage = 10;
    public GameObject damageTextPrefab;
    public string targetTag = "Player";
    public bool destroyOnHit = false;

    public int pierceCount = 0;

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
            if (pierceCount > 0)
            {
                pierceCount--;

                // ✨ 新增：穿透成功時，讓子彈盲飛甩尾！
                EnemyProjectile proj = GetComponent<EnemyProjectile>();
                if (proj != null)
                {
                    // 讓它穿透後盲飛 0.25 秒再回頭，這個數字越大，繞回來的弧度（圈圈）就越大！
                    proj.ResetHomingDelay(0.25f);
                }
            }
            else
            {
                Destroy(gameObject); // 穿透耗盡，安息吧
            }
        }
    }
}