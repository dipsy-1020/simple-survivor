using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // 引入 Linq 方便隨機抽籤

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    [Header("UI 綁定")]
    public GameObject upgradePanel;
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    [Header("能力狀態")]
    public bool hasOrbital = false;
    public bool hasFlyingSword = false;
    public int flyingSwordBurstCount = 1;
    public int flyingSwordCountPerShot = 1;
    public bool canRicochet = false;
    public float orbitalLifestealChance = 0f;

    void Awake() { instance = this; }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);

        // 1. 動態建立當下符合條件的「可用升級池」
        List<string> currentAvailableUpgrades = GetAvailableUpgrades();

        // 2. 從可用池中隨機抽出最多 3 個「不重複」的選項
        // (如果可選的少於3個，有幾個就抽幾個)
        List<string> selectedOptions = currentAvailableUpgrades.OrderBy(x => Random.value).Take(3).ToList();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < selectedOptions.Count)
            {
                optionButtons[i].gameObject.SetActive(true); // 顯示按鈕
                string choice = selectedOptions[i];
                optionTexts[i].text = choice;

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => ApplyUpgrade(choice));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false); // 如果選項不夠 3 個，隱藏多餘的按鈕
            }
        }
    }

    // 核心邏輯：判斷前置條件
    List<string> GetAvailableUpgrades()
    {
        List<string> pool = new List<string>();

        // 共通保底選項
        pool.Add("恢復 30% 血量");

        // --- 環繞劍條件 ---
        if (!hasOrbital)
        {
            pool.Add("獲得環繞劍");
        }
        else
        {
            pool.Add("增加環繞劍轉速");
            pool.Add("增加環繞劍數量");
            pool.Add("增加環繞劍長度");
            if (orbitalLifestealChance < 0.5f) pool.Add("增加環繞劍吸血"); // 吸血上限 50%
        }

        // --- 飛劍條件 ---
        if (!hasFlyingSword)
        {
            pool.Add("獲得飛劍");
        }
        else
        {
            pool.Add("增加飛劍數量");
            pool.Add("增加飛劍發射次數");
            pool.Add("增加飛劍攻速");
            if (!canRicochet) pool.Add("獲得飛劍彈射"); // 彈射只要拿一次就好
        }

        return pool;
    }

    void ApplyUpgrade(string upgradeName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        var orbital = player.GetComponent<OrbitalWeaponController>();
        var shooter = player.GetComponent<AutoShooter>();
        var health = player.GetComponent<PlayerHealth>();

        switch (upgradeName)
        {
            case "恢復 30% 血量":
                if (health != null) health.Heal(Mathf.RoundToInt(health.maxHealth * 0.3f));
                break;

            case "獲得環繞劍":
                hasOrbital = true;
                if (orbital != null) orbital.enabled = true;
                break;
            case "增加環繞劍轉速":
                if (orbital != null) orbital.rotationSpeed += 50f;
                break;
            case "增加環繞劍數量":
                if (orbital != null) orbital.UpdateSwordCount(orbital.swordCount + 1);
                break;
            case "增加環繞劍長度":
                if (orbital != null)
                {
                    orbital.swordScaleMultiplier += 0.3f; // 每次長度增加 30%
                    orbital.ApplyScale();
                }
                break;
            case "增加環繞劍吸血":
                orbitalLifestealChance += 0.05f;
                break;

            case "獲得飛劍":
                hasFlyingSword = true;
                if (shooter != null) shooter.enabled = true;
                break;
            case "增加飛劍數量":
                flyingSwordCountPerShot++;
                break;
            case "增加飛劍發射次數":
                flyingSwordBurstCount++;
                break;
            case "增加飛劍攻速":
                if (shooter != null) shooter.fireRate = Mathf.Max(0.1f, shooter.fireRate * 0.8f);
                break;
            case "獲得飛劍彈射":
                canRicochet = true;
                break;
        }

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}