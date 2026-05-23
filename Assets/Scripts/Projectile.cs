using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("飛行設定")]
    public float speed = 10f;
    public float lifeTime = 3f;
    public float rotationOffset = -90f;

    [Header("傷害設定")]
    public int damage = 15;
    public string targetTag = "Enemy";

    private Vector2 moveDirection;
    private int bounceRemaining; // 不再寫死 3 次

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // 讀取當前升級面板允許的彈射次數
        if (UpgradeManager.instance != null)
        {
            bounceRemaining = UpgradeManager.instance.flyingSwordBounceCount;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            if (targetTag == "Enemy")
            {
                EnemyHealth enemy = other.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    // 套用倍率
                    float multiplier = UpgradeManager.instance != null ? UpgradeManager.instance.globalDamageMultiplier : 1f;
                    int finalDamage = Mathf.RoundToInt(damage * multiplier);
                    enemy.TakeDamage(finalDamage);
                }

                if (bounceRemaining > 0)
                {
                    bounceRemaining--;
                    RedirectToNextEnemy(other.transform);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else if (targetTag == "Player")
            {
                // 怪物的子彈打玩家，不套用加成
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null) player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    void RedirectToNextEnemy(Transform currentEnemy)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 8f);
        Transform nextTarget = null;
        float shortest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.transform != currentEnemy)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < shortest)
                {
                    shortest = dist;
                    nextTarget = hit.transform;
                }
            }
        }

        if (nextTarget != null)
        {
            moveDirection = (nextTarget.position - transform.position).normalized;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}