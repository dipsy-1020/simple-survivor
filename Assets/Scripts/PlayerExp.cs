using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("等級設定")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int maxExp = 100;

    void Start()
    {
        if (GameManager.instance != null) GameManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);
    }

    public void AddExp(int amount)
    {
        currentExp += amount;

        if (GameManager.instance != null) GameManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);

        if (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        currentExp -= maxExp;
        maxExp = Mathf.RoundToInt(maxExp * 1.5f);

        if (GameManager.instance != null) GameManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);

        void LevelUp()
        {
            currentLevel++;
            currentExp -= maxExp;
            maxExp = Mathf.RoundToInt(maxExp * 1.5f);

            if (GameManager.instance != null) GameManager.instance.UpdateExpUI(currentExp, maxExp, currentLevel);

            // 加這行：播放升級音效
            if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();

            if (UpgradeManager.instance != null) UpgradeManager.instance.ShowUpgradeMenu();
        }

        if (UpgradeManager.instance != null)
        {
            UpgradeManager.instance.ShowUpgradeMenu();
        }
    }
}