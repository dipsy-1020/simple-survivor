using UnityEngine;
using System.Collections.Generic;

public class OrbitalWeaponController : MonoBehaviour
{
    [Header("環繞劍設定")]
    public GameObject swordPrefab;
    public int swordCount = 1;
    public float rotationSpeed = 180f;

    // 新增：用來記錄目前的長度倍率
    [HideInInspector] public float swordScaleMultiplier = 1f;

    private List<GameObject> activeSwords = new List<GameObject>();

    void Start() { GenerateSwords(); }

    void Update()
    {
        if (activeSwords.Count == 0) return;

        foreach (var sword in activeSwords)
        {
            if (sword != null)
            {
                sword.transform.position = transform.position;
                sword.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void UpdateSwordCount(int newCount)
    {
        swordCount = newCount;
        GenerateSwords();
    }

    // 新增：套用大小變化的方法
    public void ApplyScale()
    {
        foreach (var sword in activeSwords)
        {
            if (sword != null)
            {
                // 只放大圖片，不影響樞紐節點
                SpriteRenderer sr = sword.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.transform.localScale = new Vector3(swordScaleMultiplier, swordScaleMultiplier, 1f);
                }
            }
        }
    }

    void GenerateSwords()
    {
        foreach (var sword in activeSwords) { if (sword != null) Destroy(sword); }
        activeSwords.Clear();

        if (swordPrefab == null) return;

        float angleStep = 360f / swordCount;

        for (int i = 0; i < swordCount; i++)
        {
            GameObject newSword = Instantiate(swordPrefab, transform.position, Quaternion.identity);
            newSword.transform.rotation = Quaternion.Euler(0, 0, i * angleStep);
            activeSwords.Add(newSword);
        }

        // 生成後立刻套用目前的長度設定
        ApplyScale();
    }
}