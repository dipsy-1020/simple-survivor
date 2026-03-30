using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟隨目標")]
    public Transform target; // 把你的騎士拉到這裡

    [Header("平滑程度")]
    public float smoothSpeed = 5f; // 數值越小，跟隨越有「延遲的滑順感」

    [Header("位置偏移")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Z軸一定要是負的，攝影機才看得到2D畫面

    // 攝影機的移動建議放在 LateUpdate 裡面
    // 這樣可以確保主角在 Update 移動完之後，攝影機才跟上去，畫面才不會抖動
    void LateUpdate()
    {
        if (target != null)
        {
            // 計算攝影機應該要去的位置（主角位置 + 偏移量）
            Vector3 desiredPosition = target.position + offset;

            // 使用 Lerp (線性插值) 讓攝影機平滑地朝目標位置移動
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // 更新攝影機位置
            transform.position = smoothedPosition;
        }
    }
}