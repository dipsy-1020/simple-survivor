using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [Header("經驗值設定")]
    public int expValue = 10;

    [Header("磁鐵吸附設定")]
    public float magnetRadius = 5f;      // 玩家靠近多少距離會觸發
    public float repelSpeed = 6f;        // 彈開時的速度
    public float repelDuration = 0.25f;  // 彈開的持續時間 (秒)
    public float absorbSpeed = 8f;       // 吸向玩家的初始速度

    private Transform playerTransform;
    private PlayerExp playerExp;

    // 狀態機：0 = 躺在地上, 1 = 被觸發正在彈開, 2 = 急速吸入
    private int state = 0;
    private float stateTimer = 0f;

    void Start()
    {
        // 直接抓取玩家主體，不再依賴 Magnet 子物件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerExp = player.GetComponent<PlayerExp>();
        }
    }

    void Update()
    {
        if (playerTransform == null || playerExp == null) return;

        // 計算寶石與玩家核心的精確距離
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (state == 0)
        {
            // [狀態 0] 靜止等待：玩家進入範圍，觸發「排斥」狀態
            if (distance <= magnetRadius)
            {
                state = 1;
                stateTimer = 0f;
            }
        }
        else if (state == 1)
        {
            // [狀態 1] 短暫排斥：往玩家的「反方向」推出去
            stateTimer += Time.deltaTime;

            Vector2 repelDir = (transform.position - playerTransform.position).normalized;
            transform.position += (Vector3)repelDir * repelSpeed * Time.deltaTime;

            // 讓排斥速度隨時間遞減，營造出「煞車停在半空中」的動態感
            repelSpeed = Mathf.Lerp(repelSpeed, 0, Time.deltaTime * 10f);

            // 排斥時間結束，進入「吸入」狀態
            if (stateTimer >= repelDuration)
            {
                state = 2;
            }
        }
        else if (state == 2)
        {
            // [狀態 2] 急速吸入：朝著玩家飛過去
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, absorbSpeed * Time.deltaTime);

            // 速度越來越快，產生強烈的吸附感
            absorbSpeed += Time.deltaTime * 20f;

            // 碰到玩家，給經驗並銷毀
            if (distance < 0.4f)
            {
                if (distance < 0.4f)
                {
                    playerExp.AddExp(expValue);

                    // 加這行：播放吸取音效
                    if (AudioManager.instance != null) AudioManager.instance.PlayExp();

                    Destroy(gameObject);
                }

                playerExp.AddExp(expValue);
                Destroy(gameObject);
            }
        }
    }
}