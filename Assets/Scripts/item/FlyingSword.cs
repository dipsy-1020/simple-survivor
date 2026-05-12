using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    [Header("飛劍設定")]
    public float speed = 25f;
    public float returnSpeed = 35f;
    public float maxFlightDistance = 15f;

    [Header("✨ 索敵設定")]
    public float detectionRadius = 10f; // 飛劍出鞘時的鎖敵半徑

    private Transform player;
    private Vector3 targetDirection;
    private Vector3 startPosition;

    private enum SwordState { FlyingOut, Returning, Done }
    private SwordState currentState = SwordState.FlyingOut;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        startPosition = transform.position;
        FindTargetDirection();
    }

    void Update()
    {
        switch (currentState)
        {
            case SwordState.FlyingOut:
                FlyOutward();
                break;
            case SwordState.Returning:
                ReturnToPlayer();
                break;
            case SwordState.Done:
                Destroy(gameObject);
                break;
        }
    }

    void FindTargetDirection()
    {
        // ✨ 核心升級：只抓取「飛劍出發點」半徑圓圈內的所有碰撞體！
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = hit.gameObject;
                }
            }
        }

        // 如果在半徑內有敵人，就鎖定；如果沒有，就維持原本的面朝方向往前盲射
        if (nearestEnemy != null)
        {
            targetDirection = (nearestEnemy.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
        else
        {
            targetDirection = transform.up;
        }
    }

    void FlyOutward()
    {
        transform.position += targetDirection * speed * Time.deltaTime;
        float traveledDistance = Vector3.Distance(startPosition, transform.position);
        if (traveledDistance >= maxFlightDistance)
        {
            currentState = SwordState.Returning;
        }
    }

    void ReturnToPlayer()
    {
        if (player == null)
        {
            currentState = SwordState.Done;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, player.position, returnSpeed * Time.deltaTime);

        Vector3 dir = player.position - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        if (Vector3.Distance(transform.position, player.position) < 0.5f)
        {
            currentState = SwordState.Done;
        }
    }

    // ✨ 編輯器視覺化輔助
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}