using UnityEngine;
using UnityEngine.UI; // ✨ 記得引入 UI 模組
using TMPro; // ✨ 記得引入

public class PlayerExperience : MonoBehaviour
{
    [Header("經驗值設定")]
    public int currentExp = 0;
    public int maxExp = 10; // 升級所需的經驗值

    [Header("UI 綁定")]
    public Image expBarFill; // ✨ 綁定填滿條
    public TextMeshProUGUI levelText; // ✨ 新增：顯示目前等級或經驗值

    private int currentLevel = 1; // ✨ 新增記錄等級

    private UpgradeManager upgradeManager;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();

        // 遊戲開始時，刷新一次經驗條比例
        UpdateExpBarUI();
    }

    // ✨ 名字改回 AddExp，完美對接你的 Gem.cs！
    public void AddExp(int amount)
    {
        currentExp += amount;

        // 每次吃到經驗，刷新 UI
        UpdateExpBarUI();

        if (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    void UpdateExpBarUI()
    {
        if (expBarFill != null)
        {
            // 計算百分比並更新 Image 的 Fill Amount
            expBarFill.fillAmount = (float)currentExp / maxExp;
        }

        // ✨ 更新文字，例如 "Lv. 5"
        if (levelText != null)
        {
            levelText.text = $"Lv. {currentLevel}";
        }
    }

    void LevelUp()
    {
        // 1. 扣除升級所需的經驗，並增加下一級的門檻
        currentExp -= maxExp;
        maxExp = Mathf.RoundToInt(maxExp * 1.5f); // 讓下一級更難升
        currentLevel++; // ✨ 升級加 1

        // 2. ✨ 呼叫 UpgradeManager 直接給予三圍微升級 (不暫停遊戲！)
        UpgradeManager um = FindObjectOfType<UpgradeManager>();
        if (um != null)
        {
            um.ApplyMicroUpgrade();
        }

        // 3. 更新 UI
        // UpdateExpUI();
    }
}