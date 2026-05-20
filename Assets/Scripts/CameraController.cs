using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("目標設定")]
    public Transform target;       // 追蹤的目標 (通常是玩家)
    public float smoothSpeed = 5f; // 鏡頭跟隨的平滑度

    [Header("邊界設定")]
    public Vector2 minBounds;      // 左下角邊界 (例如 x: -10, y: -10)
    public Vector2 maxBounds;      // 右上角邊界 (例如 x: 10, y: 10)

    void LateUpdate() // 鏡頭移動建議放在 LateUpdate，確保玩家先移動完鏡頭才跟上
    {
        if (target == null) return;

        // 1. 取得目標位置 (保持鏡頭原本的 Z 軸，否則畫面會消失)
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // 2. 將目標位置限制在我們設定的邊界範圍內
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

        // 3. 執行平滑移動
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}