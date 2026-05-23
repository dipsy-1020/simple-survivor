using UnityEngine;

public class HealItem : MonoBehaviour
{
    [Header("回血設定")]
    public int healAmount = 20;

    [Header("磁鐵吸附設定")]
    public float pickupRadius = 3.5f; // 玩家靠近多近會觸發吸附
    public float flySpeed = 12f;      // 飛向玩家的速度

    private Transform player;
    private bool isFlying = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 如果還沒起飛，不斷偵測與玩家的距離
        if (!isFlying)
        {
            if (Vector2.Distance(transform.position, player.position) <= pickupRadius)
            {
                isFlying = true;
            }
        }
        else
        {
            // 一旦觸發起飛，無視距離，直接高速衝向玩家
            transform.position = Vector2.MoveTowards(transform.position, player.position, flySpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 碰到玩家才執行補血並銷毀
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(healAmount);
                // 這裡未來如果想加吃補血的音效，可以取消註解這行：
                // if (AudioManager.instance != null) AudioManager.instance.PlayPickup(); 
                Destroy(gameObject);
            }
        }
    }
}