using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空間

public class DamageText : MonoBehaviour
{
    [Header("飄移動畫設定")]
    public float moveSpeed = 2f;         // 向上飄的速度
    public float destroyTime = 0.6f;     // 幾秒後消失
    public Vector3 randomizeOffset = new Vector3(0.5f, 0.5f, 0); // 出現時稍微隨機偏移，避免數字全擠在一起

    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;

    void Awake()
    {
        // 抓取身上的 TextMeshPro 組件 (用於世界空間的文字)
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();

        if (textMesh != null) textColor = textMesh.color;
    }

    // 這個方法讓別的腳本可以傳入「要顯示多少傷害」
    public void Setup(int damageAmount)
    {
        if (textMesh != null) textMesh.text = damageAmount.ToString();

        // 稍微隨機打亂出現的位置
        transform.position += new Vector3(
            Random.Range(-randomizeOffset.x, randomizeOffset.x),
            Random.Range(-randomizeOffset.y, randomizeOffset.y),
            0
        );

        // 設定時間到自動銷毀
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 1. 持續向上飄移
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. 處理文字淡出 (Alpha 值從 1 變成 0)
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            textColor.a = Mathf.Lerp(1f, 0f, timer / destroyTime);
            textMesh.color = textColor;
        }
    }
}