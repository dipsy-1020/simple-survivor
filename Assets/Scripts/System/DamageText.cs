using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("文字設定")]
    public TextMeshPro textMesh; // 綁定文字組件
    public float floatSpeed = 2f; // 往上飄的速度
    public float destroyTime = 1f; // 多久後消失
    public Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0); // 隨機偏移範圍，避免數字全疊在一起

    void Start()
    {
        // 1. 設定隨機初始偏移
        transform.position += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y),
            0f);

        // 2. 設定幾秒後自動銷毀
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 3. 每一幀讓數字往上飄
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 4. (選配) 讓數字漸漸變透明
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a -= Time.deltaTime / destroyTime; // 根據時間扣除 Alpha 值
            textMesh.color = color;
        }
    }

    // 提供給外部呼叫的方法，用來設定顯示的數字
    public void Setup(int damageAmount)
    {
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
        }
    }
}