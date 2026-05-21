using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    [Header("UI 綁定")]
    public GameObject upgradePanel;
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    [Header("飛劍狀態")]
    public int flyingSwordBurstCount = 1;
    public int flyingSwordCountPerShot = 1;
    public int flyingSwordBounceCount = 0;

    [Header("環繞劍狀態")]
    public float orbitalLifestealChance = 0f;

    [Header("系統設定")]
    public int maxUpgradeLevel = 5; // 每個強化技能的最高等級

    // --- 內部等級追蹤 (0代表還沒升級過) ---
    private int orbitalSpeedLv = 0;
    private int orbitalCountLv = 0;
    private int orbitalScaleLv = 0;
    private int orbitalLifestealLv = 0;

    private int swordCountLv = 0;
    private int swordBurstLv = 0;
    private int swordSpeedLv = 0;
    private int swordBounceLv = 0;

    void Awake() { instance = this; }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);

        List<string> currentAvailableUpgrades = GetAvailableUpgrades();
        // 打亂順序並抽出最多 3 個
        List<string> selectedOptions = currentAvailableUpgrades.OrderBy(x => Random.value).Take(3).ToList();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < selectedOptions.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                string choice = selectedOptions[i];

                // 讓按鈕顯示帶有「等級」的文字
                optionTexts[i].text = GetUpgradeDisplayText(choice);

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => ApplyUpgrade(choice));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false); // 選項不足則隱藏
            }
        }
    }

    // 建立升級池：滿級的能力就不會再被加進來抽籤了！
    List<string> GetAvailableUpgrades()
    {
        List<string> pool = new List<string>();

        // --- 無限期的保底選項 (避免全部滿級後沒東西選) ---
        pool.Add("恢復 30% 血量");
        pool.Add("提升最大血量");

        // --- 環繞劍 ---
        if (orbitalSpeedLv < maxUpgradeLevel) pool.Add("增加環繞劍轉速");
        if (orbitalCountLv < maxUpgradeLevel) pool.Add("增加環繞劍數量");
        if (orbitalScaleLv < maxUpgradeLevel) pool.Add("增加環繞劍長度");
        if (orbitalLifestealLv < maxUpgradeLevel) pool.Add("增加環繞劍吸血");

        // --- 飛劍 ---
        if (swordCountLv < maxUpgradeLevel) pool.Add("增加飛劍數量");
        if (swordBurstLv < maxUpgradeLevel) pool.Add("增加飛劍發射次數");
        if (swordSpeedLv < maxUpgradeLevel) pool.Add("增加飛劍攻速");
        if (swordBounceLv < maxUpgradeLevel) pool.Add("增加飛劍彈射次數");

        return pool;
    }

    // 負責處理 UI 要顯示的文字 (例如 "增加飛劍數量 (Lv.2)")
    string GetUpgradeDisplayText(string choice)
    {
        switch (choice)
        {
            case "增加環繞劍轉速": return $"{choice} (Lv.{orbitalSpeedLv + 1})";
            case "增加環繞劍數量": return $"{choice} (Lv.{orbitalCountLv + 1})";
            case "增加環繞劍長度": return $"{choice} (Lv.{orbitalScaleLv + 1})";
            case "增加環繞劍吸血": return $"{choice} (Lv.{orbitalLifestealLv + 1})";

            case "增加飛劍數量": return $"{choice} (Lv.{swordCountLv + 1})";
            case "增加飛劍發射次數": return $"{choice} (Lv.{swordBurstLv + 1})";
            case "增加飛劍攻速": return $"{choice} (Lv.{swordSpeedLv + 1})";
            case "增加飛劍彈射次數": return $"{choice} (Lv.{swordBounceLv + 1})";

            default: return choice; // 其他沒有等級的選項 (例如補血)
        }
    }

    // 實際給予能力與數值強化
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
            case "提升最大血量":
                if (health != null)
                {
                    health.maxHealth += 20;
                    health.Heal(20); // 順便補回增加的血量
                }
                break;

            case "增加環繞劍轉速":
                orbitalSpeedLv++;
                if (orbital != null) orbital.rotationSpeed += 35f;
                break;
            case "增加環繞劍數量":
                orbitalCountLv++;
                if (orbital != null) orbital.UpdateSwordCount(orbital.swordCount + 1);
                break;
            case "增加環繞劍長度":
                orbitalScaleLv++;
                if (orbital != null)
                {
                    orbital.swordScaleMultiplier += 0.2f;
                    orbital.ApplyScale();
                }
                break;
            case "增加環繞劍吸血":
                orbitalLifestealLv++;
                orbitalLifestealChance += 0.05f; // Lv5 = 有 25% 會有吸血效果
                break;

            case "增加飛劍數量":
                swordCountLv++;
                flyingSwordCountPerShot++;
                break;
            case "增加飛劍發射次數":
                swordBurstLv++;
                flyingSwordBurstCount++;
                break;
            case "增加飛劍攻速":
                swordSpeedLv++;
                if (shooter != null) shooter.fireRate = Mathf.Max(0.1f, shooter.fireRate * 0.8f);
                break;
            case "增加飛劍彈射次數":
                swordBounceLv++;
                flyingSwordBounceCount++;
                break;
        }

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}