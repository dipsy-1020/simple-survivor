using UnityEngine;
using UnityEngine.UI; // ✨ 記得引入 UI 模組

public class PlayerExperience : MonoBehaviour
{
    [Header("經驗值設定")]
    public int currentExp = 0;
    public int maxExp = 10; // 升級所需的經驗值

    [Header("UI 綁定")]
    public Image expBarFill; // ✨ 綁定填滿條

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
    }

    void LevelUp()
    {
        currentExp -= maxExp; // 扣除消耗，保留溢出
        maxExp = Mathf.RoundToInt(maxExp * 1.5f); // 下一級需求變高

        // 升級後重新刷新 UI
        UpdateExpBarUI();

        // ✨ 新增這行：呼叫 AudioManager 播放升級聲音
        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradeMenu();
        }
    }
}