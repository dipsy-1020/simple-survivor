using UnityEngine;
using UnityEngine.EventSystems; // ✨ 需要引入事件系統來接收點擊
using TMPro;

// 強制要求這個物件身上一定要有 TextMeshProUGUI 組件
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLinkClicker : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    // 當滑鼠點擊這個文字框時，會觸發這個函式
    public void OnPointerClick(PointerEventData eventData)
    {
        // ✨ 核心魔法：請 TMP 幫忙算一下，滑鼠點擊的座標有沒有剛好碰到 <link> 標籤？
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, eventData.pressEventCamera);

        // 如果回傳值不是 -1，代表真的點到了連結！
        if (linkIndex != -1)
        {
            // 抓出我們寫在 <link="網址"> 裡面的那串網址
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();

            // 呼叫作業系統的預設瀏覽器打開這個網址
            Application.OpenURL(url);
        }
    }
}