using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections; // ✨ 記得引入 Coroutine 模組

public enum UpgradeType
{
    SwordCount,
    SwordSpeed,
    SwordDamage,
    HealPlayer,
    SwordSize,
    UltimateSword // 終極進化飛劍
}

[System.Serializable]
public class UpgradeOption
{
    public string upgradeName;
    [TextArea]
    public string description;
    public UpgradeType type;

    public int maxLevel = 0;
    [HideInInspector] public int currentLevel = 0;
    public bool isUltimate = false;
}

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 介面")]
    public GameObject upgradePanel;

    [Header("隨機抽卡 UI 綁定")]
    public Button[] optionButtons;
    public TextMeshProUGUI[] titleTexts;
    public TextMeshProUGUI[] descTexts;

    [Header("升級庫設定")]
    public List<UpgradeOption> upgradePool;

    [Header("武器設定")]
    public GameObject swordHandlePrefab;
    public int currentSwordCount = 0;
    public float rotationSpeed = 180f;
    public float currentSwordScale = 1f;

    [Header("終極進化設定 (萬劍朝宗 - 智慧填充版)")]
    public bool isUltimateUnlocked = false;
    public float attackCooldown = 0.5f; // 每 0.5 秒彈射一把飛劍
    public float reloadTime = 3f;      // 飛出去的劍 3 秒後自動回鞘
    // ✨ 新增這行：讓你可以在面板自由設定幾級解鎖！預設先給 5。
    public int ultimateUnlockLevel = 5;

    private float shootTimer = 0f;
    private int currentAttackIndex = 0; // ✨ 記錄下一個該由哪把劍彈射
    private bool[] isSwordReady;        // ✨ 用來記錄哪把劍在「鞘」中準備好了

    private Transform player;
    private List<GameObject> activeSwords = new List<GameObject>();

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        SpawnSwords(1);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        // ✨ 萬劍朝宗彈射與填充邏輯
        if (isUltimateUnlocked)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= attackCooldown && activeSwords.Count > 0)
            {
                // ✨ 核心魔法：輪流攻擊
                TryLaunchNextSword();
                shootTimer = 0f;
            }
        }

        // ✨ 保持環繞劍陣（預備 reservoir）的旋轉
        foreach (GameObject sword in activeSwords)
        {
            if (sword != null) sword.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    public void ShowUpgradeMenu()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
        RollRandomUpgrades();
    }

    void RollRandomUpgrades()
    {
        List<UpgradeOption> validPool = new List<UpgradeOption>();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        bool canUnlockUltimate = false;
        foreach (var opt in upgradePool)
        {
            // ✨ 把原本寫死的 5，改成我們剛剛設定的變數
            if (opt.type == UpgradeType.SwordCount && opt.currentLevel >= ultimateUnlockLevel)
            {
                canUnlockUltimate = true;
                break;
            }
        }

        foreach (UpgradeOption option in upgradePool)
        {
            if (option.maxLevel > 0 && option.currentLevel >= option.maxLevel) continue;
            if (option.type == UpgradeType.HealPlayer && playerHealth != null && playerHealth.currentHealth >= playerHealth.maxHealth) continue;
            if (option.isUltimate && !canUnlockUltimate) continue;

            validPool.Add(option);
        }

        int optionsToShow = Mathf.Min(3, validPool.Count);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < optionsToShow)
            {
                optionButtons[i].gameObject.SetActive(true);

                int randomIndex = Random.Range(0, validPool.Count);
                UpgradeOption selectedOption = validPool[randomIndex];

                if (selectedOption.maxLevel > 0 && !selectedOption.isUltimate)
                    titleTexts[i].text = selectedOption.upgradeName + " (Lv." + (selectedOption.currentLevel + 1) + ")";
                else
                    titleTexts[i].text = selectedOption.upgradeName;

                descTexts[i].text = selectedOption.description;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => ApplyUpgrade(selectedOption));
                validPool.RemoveAt(randomIndex);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void ApplyUpgrade(UpgradeOption option)
    {
        option.currentLevel++;

        switch (option.type)
        {
            case UpgradeType.SwordCount:
                SpawnSwords(1);
                break;
            case UpgradeType.SwordSpeed:
                rotationSpeed += 30f;
                break;
            case UpgradeType.SwordDamage:
                SwordAttack[] swords = FindObjectsOfType<SwordAttack>();
                foreach (SwordAttack s in swords) s.damage += 3;
                break;
            case UpgradeType.HealPlayer:
                PlayerHealth ph = FindObjectOfType<PlayerHealth>();
                if (ph != null) ph.Heal(20);
                break;
            case UpgradeType.SwordSize:
                currentSwordScale += 0.3f;
                UpdateAllSwordsSize();
                break;
            case UpgradeType.UltimateSword:
                // ✨ 啟動終極大招，保留劍陣不銷毀
                isUltimateUnlocked = true;

                // 初始化彈匣狀態：所有的劍都在鞘中準備好
                InitializeAmmoPool();
                break;
        }

        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();
        ResumeGame();
    }

    // ✨ 萬劍朝宗：嘗試彈射下一把準備好的劍
    void TryLaunchNextSword()
    {
        // 如果 ammo 狀態沒初始化，就跳過
        if (isSwordReady == null || isSwordReady.Length == 0) return;

        // ✨ 智慧填充邏輯：從 currentAttackIndex 開始尋找第一把「準備好」的劍
        int startIndex = currentAttackIndex;
        bool foundReadySword = false;

        do
        {
            // 檢查這把劍是否準備好彈射
            if (isSwordReady[currentAttackIndex])
            {
                // ✨ 核心魔法：彈射並啟動填充協程
                StartCoroutine(LaunchAndReloadRoutine(currentAttackIndex));

                // ✨ 記錄：這把劍飛出去了，不在鞘中
                isSwordReady[currentAttackIndex] = false;
                foundReadySword = true;

                // ✨ 下次從下一把劍開始尋找
                currentAttackIndex = (currentAttackIndex + 1) % currentSwordCount;
                break;
            }

            // 如果沒準備好，繼續找下一把
            currentAttackIndex = (currentAttackIndex + 1) % currentSwordCount;

        } while (currentAttackIndex != startIndex); // 轉了一圈都沒找到準備好的劍，就不攻擊

        // 如果找不到任何一把準備好的劍，就代表玩家需要等待填充，完美呈現「輪流攻擊」限制。
    }

    // ✨ 修改後的協程：等待飛劍複本死亡，才算回鞘
    IEnumerator LaunchAndReloadRoutine(int index)
    {
        if (player == null || activeSwords[index] == null) yield break;

        GameObject handle = activeSwords[index];

        if (handle.transform.childCount > 0)
        {
            Transform blade = handle.transform.GetChild(0);

            // 1. 讓原本的劍隱形 (出鞘)
            blade.gameObject.SetActive(false);

            // 2. 生成飛劍複本
            GameObject flyingVisual = Instantiate(blade.gameObject, handle.transform.position, handle.transform.rotation);
            flyingVisual.SetActive(true);
            flyingVisual.transform.localScale = blade.localScale;

            // 裝上新的迴力鏢 AI
            flyingVisual.AddComponent<FlyingSword>();

            if (AudioManager.instance != null) AudioManager.instance.PlayHitSound();

            // ✨ 3. 核心修改：不再死等 3 秒，而是每一幀檢查那把飛劍複本還活著嗎？
            // 只要飛劍還在場上 (還沒飛回主角身邊銷毀)，這個協程就會一直卡在這裡等
            while (flyingVisual != null)
            {
                yield return null; // 等待下一幀再檢查
            }

            // ✨ 4. 當 while 迴圈結束，代表飛劍已經飛回主角身邊並銷毀了！
            // 執行回鞘動作
            isSwordReady[index] = true;
            if (blade != null)
            {
                blade.gameObject.SetActive(true); // 原本的劍亮起來
            }
        }
    }

    void InitializeAmmoPool()
    {
        currentSwordCount = activeSwords.Count;
        isSwordReady = new bool[currentSwordCount];
        for (int i = 0; i < currentSwordCount; i++) isSwordReady[i] = true;

        // 重置攻擊索引
        currentAttackIndex = 0;
    }

    void UpdateAllSwordsSize()
    {
        foreach (GameObject swordHandle in activeSwords)
        {
            if (swordHandle != null && swordHandle.transform.childCount > 0)
            {
                Transform blade = swordHandle.transform.GetChild(0);
                blade.localScale = new Vector3(1f, currentSwordScale, 1f);
            }
        }
    }

    private void SpawnSwords(int amountToIncrease)
    {
        // 終極大招解鎖後，不需要重生成物理劍陣，保留彈匣 Reservoir 即可
        if (isUltimateUnlocked) return;

        if (player == null || swordHandlePrefab == null) return;
        currentSwordCount += amountToIncrease;

        foreach (GameObject sword in activeSwords)
        {
            if (sword != null) Destroy(sword);
        }
        activeSwords.Clear();

        for (int i = 0; i < currentSwordCount; i++)
        {
            float angle = i * (360f / currentSwordCount);
            Quaternion initialRotation = Quaternion.Euler(0, 0, angle);
            GameObject newSword = Instantiate(swordHandlePrefab, player.position, initialRotation, player);
            newSword.transform.localPosition = Vector3.zero;

            if (newSword.transform.childCount > 0)
            {
                newSword.transform.GetChild(0).localScale = new Vector3(1f, currentSwordScale, 1f);
            }

            activeSwords.Add(newSword);
        }
    }

    void ResumeGame()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}