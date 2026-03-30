using UnityEngine;

public class RotatingWeapon : MonoBehaviour
{
    [Header("旋轉設定")]
    public float rotationSpeed = 180f; // 每秒旋轉的角度（正數順時針，負數逆時針）

    void Update()
    {
        // 核心魔法！
        // transform.Rotate(0, 0, 角度) 是讓物件繞著 Z 軸（2D 平面的旋轉軸）旋轉。
        // rotationSpeed * Time.deltaTime 確保在任何電腦上旋轉速度都一致。
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}