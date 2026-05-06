using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    public Sprite icon;
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
    public Image[] iconImages;

    [Header("升級庫設定")]
    public List<UpgradeOption> upgradePool;

    [Header("武器設定")]
    public GameObject swordHandlePrefab;
    public int currentSwordCount = 0;
    public float rotationSpeed = 180f;
    public float currentSwordScale = 1f;

    [Header("終極進化設定 (萬劍朝宗 - 智慧填充版)")]
    public bool isUltimateUnlocked = false;
    public float attackCooldown = 0.5f;
    public float reloadTime = 3f;
    public int ultimateUnlockLevel = 5;

    private float shootTimer = 0f;
    private int currentAttackIndex = 0;
    private bool[] isSwordReady;

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

        if (isUltimateUnlocked)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= attackCooldown && activeSwords.Count > 0)
            {
                TryLaunchNextSword();
                shootTimer = 0f;
            }
        }

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
            if (opt.type == UpgradeType.SwordCount && opt.currentLevel >= ultimateUnlockLevel)
            {
                canUnlockUltimate = true;
                break;
            }
        }

        foreach (UpgradeOption option in upgradePool)
        {
            // 防呆 1：滿等技能不出現
            if (option.maxLevel > 0 && option.currentLevel >= option.maxLevel) continue;
            // 防呆 2：滿血不出現補血
            if (option.type == UpgradeType.HealPlayer && playerHealth != null && playerHealth.currentHealth >= playerHealth.maxHealth) continue;
            // 防呆 3：條件未滿不出大招
            if (option.isUltimate && !canUnlockUltimate) continue;
            // ✨ 防呆 4 (修復 Bug)：如果大招已經解鎖過了，絕對不要再放進抽卡池！
            if (option.isUltimate && isUltimateUnlocked) continue;

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

                if (selectedOption.icon != null)
                {
                    iconImages[i].sprite = selectedOption.icon;
                    iconImages[i].gameObject.SetActive(true);
                }
                else
                {
                    iconImages[i].gameObject.SetActive(false);
                }

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
                // ✨ 已經替換為萬用傷害模組，並加上 Enemy 標籤防呆
                UniversalDamageHitbox[] hitboxes = FindObjectsOfType<UniversalDamageHitbox>();
                foreach (UniversalDamageHitbox h in hitboxes)
                {
                    if (h.targetTag == "Enemy") h.damage += 3;
                }
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
                isUltimateUnlocked = true;
                InitializeAmmoPool();
                break;
        }

        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();
        ResumeGame();
    }

    void TryLaunchNextSword()
    {
        if (isSwordReady == null || isSwordReady.Length == 0) return;

        int startIndex = currentAttackIndex;
        bool foundReadySword = false;

        do
        {
            if (isSwordReady[currentAttackIndex])
            {
                StartCoroutine(LaunchAndReloadRoutine(currentAttackIndex));
                isSwordReady[currentAttackIndex] = false;
                foundReadySword = true;
                currentAttackIndex = (currentAttackIndex + 1) % currentSwordCount;
                break;
            }
            currentAttackIndex = (currentAttackIndex + 1) % currentSwordCount;
        } while (currentAttackIndex != startIndex);
    }

    IEnumerator LaunchAndReloadRoutine(int index)
    {
        if (player == null || activeSwords[index] == null) yield break;

        GameObject handle = activeSwords[index];

        if (handle.transform.childCount > 0)
        {
            Transform blade = handle.transform.GetChild(0);
            blade.gameObject.SetActive(false);

            GameObject flyingVisual = Instantiate(blade.gameObject, handle.transform.position, handle.transform.rotation);
            flyingVisual.SetActive(true);
            flyingVisual.transform.localScale = blade.localScale;

            flyingVisual.AddComponent<FlyingSword>();

            if (AudioManager.instance != null) AudioManager.instance.PlayHitSound();

            while (flyingVisual != null)
            {
                yield return null;
            }

            isSwordReady[index] = true;
            if (blade != null)
            {
                blade.gameObject.SetActive(true);
            }
        }
    }

    void InitializeAmmoPool()
    {
        currentSwordCount = activeSwords.Count;
        isSwordReady = new bool[currentSwordCount];
        for (int i = 0; i < currentSwordCount; i++) isSwordReady[i] = true;
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

    // ==========================================
    // ✨ 動態波次系統：福禍相依抉擇 (玩家自己組合怪物)
    // ==========================================

    public void ChooseRedSpiderBane()
    {
        // 1. 給予玩家 Buff 
        SpawnSwords(1); // 舉例：多給一把劍當作獎勵

        // 2. 動態捏出一隻紅色突進蜘蛛的食譜
        MonsterData newMonster = ScriptableObject.CreateInstance<MonsterData>();
        newMonster.monsterName = "業障·血紅突進者";
        newMonster.monsterColor = Color.red;
        newMonster.scaleMultiplier = 1.5f;
        newMonster.maxHealth = 100;
        newMonster.moveSpeed = 3f;
        newMonster.useMeleeAttack = true;
        newMonster.brainType = MonsterData.AIType.SpiderCharge;

        // 3. 塞入 Spawner 的卡池中
        if (EnemySpawner.instance != null)
        {
            EnemySpawner.instance.activeMonsterRoster.Add(newMonster);
        }

        ResumeGame();
    }

    public void ChooseGreenGiantBane()
    {
        rotationSpeed += 30f; // 舉例：轉速變快當作獎勵

        MonsterData newMonster = ScriptableObject.CreateInstance<MonsterData>();
        newMonster.monsterName = "業障·綠色巨型怪";
        newMonster.monsterColor = Color.green;
        newMonster.scaleMultiplier = 2.5f;
        newMonster.maxHealth = 300;
        newMonster.moveSpeed = 1f;
        newMonster.useMeleeAttack = true;
        newMonster.brainType = MonsterData.AIType.StraightChaser;

        if (EnemySpawner.instance != null)
        {
            EnemySpawner.instance.activeMonsterRoster.Add(newMonster);
        }

        ResumeGame();
    }
    // ==========================================
    // ✨ 路線二：局內全自動微升級 (經驗條滿時呼叫)
    // ==========================================
    public void ApplyMicroUpgrade()
    {
        // 1. 三圍微幅提升 (你可以依據平衡性自己微調數字)

        // 【三圍 1：傷害微升】
        UniversalDamageHitbox[] hitboxes = FindObjectsOfType<UniversalDamageHitbox>();
        foreach (UniversalDamageHitbox h in hitboxes)
        {
            // 非常重要：確保只加成標籤為 "Enemy" 的觸發器 (也就是玩家的飛劍)
            // 不然你連怪物跟蝙蝠的攻擊力都會一起升級，玩家會哭出來 XD
            if (h.targetTag == "Enemy")
            {
                h.damage += 1;
            }
        }

        // 【三圍 2：轉速微升】
        rotationSpeed += 5f; // 每次升級加 5 轉速

        // 【三圍 3：血量微升與回復】
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null)
        {
            ph.maxHealth += 5;   // 上限加 5
            ph.Heal(5);          // 順便補 5 滴血，提高生存率
        }

        // 2. 播放升級音效 (保留原本的爽感)
        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();

        // 3. 可以在這裡加一個玩家頭上飄出 "Level Up!" 的小特效或文字 (選配)
        Debug.Log("自動微升級完成：傷害+1、轉速+5、血量+5！");
    }
    // ==========================================
    // ✨ 路線二：波次大抉擇 (三選一福禍相依)
    // ==========================================

    /// <summary>
    /// 隨機捏造一隻變異怪物的「食譜」
    /// </summary>
    private MonsterData GenerateRandomBane()
    {
        MonsterData newMonster = ScriptableObject.CreateInstance<MonsterData>();

        // 1. 隨機決定 AI 類型 (0 = 直線追擊, 1 = 蜘蛛突進)
        int aiRoll = Random.Range(0, 2);

        // 2. 隨機決定顏色與前綴詞
        string[] prefixes = { "血紅", "劇毒", "虛空", "狂暴", "鋼鐵" };
        Color[] colors = { Color.red, Color.green, new Color(0.5f, 0, 0.5f), new Color(1f, 0.5f, 0f), Color.gray };
        int colorRoll = Random.Range(0, prefixes.Length);

        // 3. 組合屬性
        newMonster.monsterColor = colors[colorRoll];
        newMonster.useMeleeAttack = true;

        if (aiRoll == 0)
        {
            // 巨型肉盾型
            newMonster.monsterName = $"業障·{prefixes[colorRoll]}巨獸";
            newMonster.scaleMultiplier = Random.Range(1.5f, 2.5f);
            newMonster.maxHealth = 150;
            newMonster.moveSpeed = Random.Range(1f, 1.8f); // 走得慢
            newMonster.brainType = MonsterData.AIType.StraightChaser;
        }
        else
        {
            // 高速刺客型
            newMonster.monsterName = $"業障·{prefixes[colorRoll]}突進者";
            newMonster.scaleMultiplier = Random.Range(0.8f, 1.2f); // 體型較小
            newMonster.maxHealth = 80;
            newMonster.moveSpeed = Random.Range(2.5f, 3.5f); // 跑得快
            newMonster.brainType = MonsterData.AIType.SpiderCharge;
        }

        return newMonster;
    }
    /// <summary>
    /// 呼叫這個方法來開啟「波次結束」的三選一面板
    /// </summary>
    public void ShowBaneMenu()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true); // 沿用你原本的面板！
        RollBaneChoices();
    }

    private void RollBaneChoices()
    {
        List<UpgradeOption> validPool = new List<UpgradeOption>();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        bool canUnlockUltimate = false;

        // 檢查大招條件
        foreach (var opt in upgradePool)
        {
            if (opt.type == UpgradeType.SwordCount && opt.currentLevel >= ultimateUnlockLevel)
            {
                canUnlockUltimate = true;
                break;
            }
        }

        // 過濾可用選項 (防呆邏輯)
        foreach (UpgradeOption option in upgradePool)
        {
            if (option.maxLevel > 0 && option.currentLevel >= option.maxLevel) continue;
            if (option.type == UpgradeType.HealPlayer && playerHealth != null && playerHealth.currentHealth >= playerHealth.maxHealth) continue;
            if (option.isUltimate && !canUnlockUltimate) continue;
            if (option.isUltimate && isUltimateUnlocked) continue;
            validPool.Add(option);
        }

        int optionsToShow = Mathf.Min(3, validPool.Count);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < optionsToShow)
            {
                optionButtons[i].gameObject.SetActive(true);

                // 隨機抽一個 Buff
                int randomIndex = Random.Range(0, validPool.Count);
                UpgradeOption selectedBuff = validPool[randomIndex];

                // ✨ 隨機生成一個 Bane (變異怪物)
                MonsterData generatedBane = GenerateRandomBane();

                // 設定標題與 Icon
                titleTexts[i].text = selectedBuff.maxLevel > 0 && !selectedBuff.isUltimate
                    ? $"{selectedBuff.upgradeName} (Lv.{selectedBuff.currentLevel + 1})"
                    : selectedBuff.upgradeName;

                if (selectedBuff.icon != null)
                {
                    iconImages[i].sprite = selectedBuff.icon;
                    iconImages[i].gameObject.SetActive(true);
                }
                else
                {
                    iconImages[i].gameObject.SetActive(false);
                }

                // ✨ 將 Buff 說明與 Bane 說明組合在一起顯示！
                descTexts[i].text = $"{selectedBuff.description}\n\n<color=red>【劫數代價】\n下一波加入：{generatedBane.monsterName}</color>";

                // 清除舊事件，綁定新的「同時給予 Buff 與 Bane」的事件
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => ApplyBaneUpgrade(selectedBuff, generatedBane));

                validPool.RemoveAt(randomIndex);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 玩家點擊選項後執行：給獎勵 -> 塞怪物 -> 關面板
    /// </summary>
    private void ApplyBaneUpgrade(UpgradeOption buff, MonsterData bane)
    {
        // 1. 給予原本的升級獎勵
        ApplyUpgrade(buff); // 這會呼叫你原本寫好的 switch 判斷並關閉面板

        // 2. 將隨機生成的怪物塞進卡池
        if (EnemySpawner.instance != null)
        {
            EnemySpawner.instance.activeMonsterRoster.Add(bane);
            Debug.Log($"福禍相依觸發！獲得 {buff.upgradeName}，卡池加入 {bane.monsterName}！");
        }
    }
}