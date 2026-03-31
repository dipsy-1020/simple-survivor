using UnityEngine;

public class SpiderAI : MonoBehaviour
{
    [Header("基礎設定")]
    private Transform player;
    public float normalSpeed = 2f;    // 平常走路的速度

    [Header("突進 (Dash) 設定")]
    public float dashRange = 4.5f;    // 距離玩家多近時，開始準備撲過去
    public float dashSpeed = 12f;     // 撲過去的瞬間速度 (非常快)
    public float dashDuration = 0.2f; // 往前撲的時間長度
    public float prepareTime = 0.6f;  // 撲之前「停頓蓄力」的時間 (給玩家反應)
    public float cooldownTime = 1.5f; // 撲完之後，在原地喘息的時間

    // 狀態機：追逐、蓄力準備、衝刺中、冷卻中
    private enum SpiderState { Chasing, Preparing, Dashing, Cooldown }
    private SpiderState currentState = SpiderState.Chasing;

    private float stateTimer = 0f;
    private Vector2 dashDirection; // 紀錄撲過去的方向

    void Start()
    {
        // 一出生就找玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 根據蜘蛛目前的「狀態」決定牠要做什麼
        switch (currentState)
        {
            case SpiderState.Chasing:
                // 1. 追逐狀態：慢慢走向玩家
                transform.position = Vector2.MoveTowards(transform.position, player.position, normalSpeed * Time.deltaTime);

                // 如果距離夠近了，切換到「蓄力準備」狀態
                if (Vector2.Distance(transform.position, player.position) <= dashRange)
                {
                    ChangeState(SpiderState.Preparing);
                }
                break;

            case SpiderState.Preparing:
                // 2. 蓄力狀態：站在原地不動，時間倒數
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    // 蓄力完畢！鎖定玩家現在的方向，準備發射自己
                    dashDirection = (player.position - transform.position).normalized;
                    ChangeState(SpiderState.Dashing);
                }
                break;

            case SpiderState.Dashing:
                // 3. 衝刺狀態：無視玩家位置，朝剛剛鎖定的方向高速突進
                transform.Translate(dashDirection * dashSpeed * Time.deltaTime, Space.World);
                stateTimer -= Time.deltaTime;

                // 衝刺時間結束，切換到「冷卻」狀態
                if (stateTimer <= 0)
                {
                    ChangeState(SpiderState.Cooldown);
                }
                break;

            case SpiderState.Cooldown:
                // 4. 冷卻狀態：站在原地喘息
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    ChangeState(SpiderState.Chasing); // 喘完氣，繼續追！
                }
                break;
        }
    }

    // 用來切換狀態與重置計時器的工具
    void ChangeState(SpiderState newState)
    {
        currentState = newState;
        if (newState == SpiderState.Preparing) stateTimer = prepareTime;
        else if (newState == SpiderState.Dashing) stateTimer = dashDuration;
        else if (newState == SpiderState.Cooldown) stateTimer = cooldownTime;
    }

    // 在編輯器畫出攻擊範圍紅圈，方便你調整距離
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}