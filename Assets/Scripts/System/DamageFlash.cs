using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DamageFlash : MonoBehaviour
{
    [Header("閃爍設定")]
    [ColorUsage(true, true)] // 允許選擇 HDR 顏色（如果之後要加 Post-Processing 特效會用到）
    public Color flashColor = Color.white; // 閃爍的顏色 (預設白色)
    public float flashDuration = 0.15f; // 閃爍的時間 (秒)

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine; // 用來記錄閃爍的協程

    void Start()
    {
        // 抓取精靈圖元件
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 記住原本的圖片顏色
        originalColor = spriteRenderer.color;
    }

    // 這是對外的公開方法，供 Health 腳本呼叫
    public void CallFlash()
    {
        // 如果原本就在閃爍，先停止舊的閃爍，確保新的閃爍完整執行
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        // 啟動閃爍協程
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    // 閃爍的協程 (Coroutines)
    // 協程是 Unity 處理「時間差」的最佳工具
    private IEnumerator FlashRoutine()
    {
        // 1. 把圖片變成白色
        spriteRenderer.color = flashColor;

        // 2. 等待設定的時間
        yield return new WaitForSeconds(flashDuration);

        // 3. 把圖片變回原本的顏色
        spriteRenderer.color = originalColor;

        flashCoroutine = null; // 結束後清空記錄
    }
}