using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    [Header("飛劍設定")]
    public float speed = 25f;       // 飛出去的速度 (稍微調快一點更帥)
    public float returnSpeed = 35f; // 飛回來的速度 (通常召回會比較快)
    public float maxFlightDistance = 15f; // 劍飛多遠後會強制折返

    private Transform player;
    private Vector3 targetDirection; // 鎖定的飛行方向 (不再鎖定實體物件)
    private Vector3 startPosition;   // 記錄起飛位置，用來算距離

    // 定義飛劍的三個生命階段
    private enum SwordState { FlyingOut, Returning, Done }
    private SwordState currentState = SwordState.FlyingOut;

    void Start()
    {
        // 抓取主角座標，準備之後飛回來用
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        startPosition = transform.position;

        // 一出鞘，立刻掃描最近的敵人，但這次我們只取「方向」
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
                // 已經回到主角身邊，通知 UpgradeManager 把實體劍亮起來，然後自己銷毀
                Destroy(gameObject);
                break;
        }
    }

    void FindTargetDirection()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        // 如果附近有敵人，就鎖定他的「方向」；如果沒敵人，就隨便往前飛
        if (nearestEnemy != null)
        {
            targetDirection = (nearestEnemy.transform.position - transform.position).normalized;
            // 轉向對準敵人
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
        else
        {
            targetDirection = transform.up; // 預設往前
        }
    }

    void FlyOutward()
    {
        // ✨ 魔法 1：不鎖定座標，而是朝著剛才算好的「方向」一直飛 (貫穿效果)
        transform.position += targetDirection * speed * Time.deltaTime;

        // 計算飛了多遠
        float traveledDistance = Vector3.Distance(startPosition, transform.position);

        // 如果飛得夠遠了，或者飛出場外太遠，就進入「折返」階段
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

        // ✨ 魔法 2：像磁鐵一樣，高速飛回主角身邊
        transform.position = Vector3.MoveTowards(transform.position, player.position, returnSpeed * Time.deltaTime);

        // 轉向對準主角
        Vector3 dir = player.position - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        // 如果距離主角非常近了，就視為「已回鞘」
        if (Vector3.Distance(transform.position, player.position) < 0.5f)
        {
            currentState = SwordState.Done;
        }
    }
}