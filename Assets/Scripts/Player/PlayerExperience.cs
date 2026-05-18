using UnityEngine;
using UnityEngine.UI; // ✨ 記得引入 UI 模組
using TMPro; // ✨ 記得引入

public class PlayerExperience : MonoBehaviour
{
    [Header("經驗值設定")]
    public int currentExp = 0;
    public int maxExp = 100; // 升級所需的經驗值

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

        // ✨ 核心修復 1：使用 while 迴圈，如果一次吃太多經驗，可以連續觸發多次升級！
        while (currentExp >= maxExp)
        {
            LevelUp();
        }

        // ✨ 核心修復 2：把更新 UI 放在 LevelUp 結算「之後」！
        // 這樣如果經驗剛好滿了被扣除，血條就會立刻顯示為 0 (或殘餘經驗)，不會卡在 100% 滿的狀態。
        UpdateExpBarUI();
    }

    void UpdateExpBarUI()
    {
        if (expBarFill != null)
        {
            // 計算百分比並更新 Image 的 Fill Amount (進度條的視覺比例不變)
            expBarFill.fillAmount = (float)currentExp / maxExp;
        }

        // ✨ 核心修改：顯示具體的充能數值 (例如 "充能進度：450 / 500")
        if (levelText != null)
        {
            levelText.text = $"充能進度：{currentExp} / {maxExp}";
        }
    }

    void LevelUp()
    {
        currentExp -= maxExp;

        // ✨ 核心修復：使用「線性加法」取代「乘法」，並設定天花板
        if (maxExp < 3000)
        {
            maxExp += 100; // 每次升級，下一級只需要「多吃小寶石」的量
        }
        else
        {
            maxExp = 3000; // 等級再高，升級門檻也永遠固定在 500 經驗值
        }

        currentLevel++;

        UpgradeManager um = FindObjectOfType<UpgradeManager>();
        if (um != null)
        {
            um.ApplyMicroUpgrade();
        }
    }
}