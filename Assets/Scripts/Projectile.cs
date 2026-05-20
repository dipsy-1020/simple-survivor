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
    private int bounceRemaining = 3; // 彈射次數上限

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
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
                if (enemy != null) enemy.TakeDamage(damage);

                // --- 彈射邏輯 ---
                if (UpgradeManager.instance.canRicochet && bounceRemaining > 0)
                {
                    bounceRemaining--;
                    RedirectToNextEnemy(other.transform);
                }
                else
                {
                    Destroy(gameObject); // 沒彈射次數了才銷毀
                }
            }
            else if (targetTag == "Player")
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null) player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    void RedirectToNextEnemy(Transform currentEnemy)
    {
        // 尋找周圍 8 單位內的敵人
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 8f);
        Transform nextTarget = null;
        float shortest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // 必須是敵人，且不能是剛打到的那隻
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

        // 如果有找到下一個目標，轉向飛過去
        if (nextTarget != null)
        {
            moveDirection = (nextTarget.position - transform.position).normalized;
        }
        else
        {
            Destroy(gameObject); // 周圍沒敵人了，直接銷毀
        }
    }
}