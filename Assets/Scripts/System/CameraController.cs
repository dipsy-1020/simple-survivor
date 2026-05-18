using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟隨目標")]
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("✨ 邊界限制（以分類父物件為 (0,0) 的地圖範圍）")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-20f, -15f); // 🎯 這裡直接填 20 左右的數值！
    public Vector2 maxBounds = new Vector2(20f, 15f);

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 抓取目標相對於父物件的「局部座標」
        Vector3 desiredLocalPosition = target.localPosition + offset;

        // 2. 在局部空間裡進行平滑插值運算
        Vector3 smoothedLocalPosition = Vector3.Lerp(transform.localPosition, desiredLocalPosition, smoothSpeed * Time.deltaTime);

        // 3. 局部海關檢查：這時候限制的才是你填的 (-20, 20) 牆壁！
        if (useBounds)
        {
            smoothedLocalPosition.x = Mathf.Clamp(smoothedLocalPosition.x, minBounds.x, maxBounds.x);
            smoothedLocalPosition.y = Mathf.Clamp(smoothedLocalPosition.y, minBounds.y, maxBounds.y);
        }

        // 4. 正確指派給 localPosition，讓攝影機乖乖待在分類父物件肚子裡
        transform.localPosition = smoothedLocalPosition;
    }
}