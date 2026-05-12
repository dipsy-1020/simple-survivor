using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("寶石設定")]
    public int expValue = 10;
    public float moveSpeed = 15f;

    private Transform targetPlayer;

    // ✨ 新增：定義寶石的階級種類
    public enum GemTier { Small, Medium, Large }

    // ✨ 新增：初始化寶石的方法 (生成時由怪物呼叫)
    public void Initialize(GemTier tier)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // ✨ 統一設定所有寶石的大小，不再隨階級改變 (這裡預設為 0.5 倍大小，你可以自己微調)
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        switch (tier)
        {
            case GemTier.Small:
                expValue = 10;
                sr.color = Color.cyan;   // 小藍寶石
                break;
            case GemTier.Medium:
                expValue = 50;
                sr.color = Color.red;    // 中紅寶石
                break;
            case GemTier.Large:
                expValue = 200;
                sr.color = Color.yellow; // 大金寶石
                break;
        }
    }

    public void StartFlying(Transform playerTransform)
    {
        targetPlayer = playerTransform;
    }

    void Update()
    {
        if (targetPlayer != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExperience playerExp = other.GetComponent<PlayerExperience>();
            if (playerExp != null)
            {
                playerExp.AddExp(expValue);

                // ✨ 新增這行：呼叫 AudioManager 播放吃寶石聲音
                if (AudioManager.instance != null) AudioManager.instance.PlayGem();

                Destroy(gameObject);
            }
        }
    }
}