using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

// ✨ 包含所有流派與彈射的升級選項
public enum UpgradeType
{
    UnlockRotatingSword,
    UnlockFlyingSword,

    DamageUp,
    MaxHealthUp,

    RotatingSwordCount,
    RotatingSwordSpeed,
    RotatingSwordSize,

    FlyingSwordFireRate,
    FlyingSwordCount,
    FlyingSwordPierce,
}

[System.Serializable]
public class UpgradeOption
{
    public string upgradeName;
    public Sprite icon;
    public UpgradeType type;

    [Header("解鎖前置條件 (沒解鎖不會出現)")]
    public bool requiresRotatingSword;
    public bool requiresFlyingSword;

    public int maxLevel = 0;
    [HideInInspector] public int currentLevel = 0;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    [Header("UI 介面")]
    public GameObject upgradePanel;
    public Button[] optionButtons;
    public TextMeshProUGUI[] titleTexts;
    public TextMeshProUGUI[] descTexts;
    public Image[] iconImages;
    public Image[] baneIconImages;

    [Header("✨ 局內強化圖示清單 (Loadout UI)")]
    public GameObject iconPrefab;
    public Transform iconContainer;
    private Dictionary<UpgradeType, Image> activeIconUI = new Dictionary<UpgradeType, Image>();

    private List<MonsterData> pastBosses = new List<MonsterData>();

    [HideInInspector] public bool hasRotatingSword = false;
    [HideInInspector] public bool hasFlyingSword = false;
    [HideInInspector] public int extraSwordDamage = 0;

    [Header("武器傷害統一設定")]
    public int baseSwordDamage = 15; // ✨ 新增這行，以後兩把武器的基礎傷害都在這裡調！

    [Header("開局固定流派設定 (二選一)")]
    public UpgradeOption initialBuffA;
    public MonsterData initialBaneA;
    [Space(10)]
    public UpgradeOption initialBuffB;
    public MonsterData initialBaneB;

    [Header("升級庫設定 (玩家 Buff)")]
    public List<UpgradeOption> upgradePool;

    // ==========================================
    // ✨ 核心修改 1：將大雜燴改為三個明確分類的怪物庫
    // ==========================================
    [Header("業障庫設定 (分類怪物庫)")]
    public List<MonsterData> normalMonsters; // 放普通小怪
    public List<MonsterData> eliteMonsters;  // 放菁英怪
    public List<MonsterData> bossMonsters;   // 放 Boss

    [Header("環繞武器設定")]
    public GameObject swordHandlePrefab;
    public int currentSwordCount = 0;
    public float rotationSpeed = 180f;
    public float currentSwordScale = 1.5f;

    [Header("✨ 狂熱狀態管理")]
    private bool isFeverActive = false; // 只用一個布林值當作鎖
    private float normalRotationSpeed;
    private float normalFireRate;

    private Transform player;
    private List<GameObject> activeSwords = new List<GameObject>();

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        foreach (GameObject sword in activeSwords)
        {
            if (sword != null) sword.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    // ==========================================
    // 開局流派選擇 (超級防呆版)
    // ==========================================
    public void ShowInitialMenu()
    {
        upgradePanel.SetActive(true);

        // --- 選項 A ---
        if (optionButtons.Length > 0 && optionButtons[0] != null)
        {
            optionButtons[0].gameObject.SetActive(true);
            if (titleTexts.Length > 0 && titleTexts[0] != null) titleTexts[0].text = initialBuffA.upgradeName;

            if (iconImages.Length > 0 && iconImages[0] != null)
            {
                if (initialBuffA.icon != null) { iconImages[0].sprite = initialBuffA.icon; iconImages[0].gameObject.SetActive(true); }
                else { iconImages[0].gameObject.SetActive(false); }
            }

            if (baneIconImages != null && baneIconImages.Length > 0 && baneIconImages[0] != null)
            {
                if (initialBaneA != null && initialBaneA.monsterSprite != null) { baneIconImages[0].sprite = initialBaneA.monsterSprite; baneIconImages[0].gameObject.SetActive(true); }
                else { baneIconImages[0].gameObject.SetActive(false); }
            }

            string warningA = $"\n<color=red>【初始劫數】加入：{(initialBaneA != null ? initialBaneA.monsterName : "無")}</color>";
            if (descTexts != null && descTexts.Length > 0 && descTexts[0] != null) { descTexts[0].text = warningA; descTexts[0].gameObject.SetActive(true); }
            else if (titleTexts.Length > 0 && titleTexts[0] != null) { titleTexts[0].text += warningA; }

            optionButtons[0].onClick.RemoveAllListeners();
            optionButtons[0].onClick.AddListener(() => ApplyInitialChoice(initialBuffA, initialBaneA));
        }

        // --- 選項 B ---
        if (optionButtons.Length > 1 && optionButtons[1] != null)
        {
            optionButtons[1].gameObject.SetActive(true);
            if (titleTexts.Length > 1 && titleTexts[1] != null) titleTexts[1].text = initialBuffB.upgradeName;

            if (iconImages.Length > 1 && iconImages[1] != null)
            {
                if (initialBuffB.icon != null) { iconImages[1].sprite = initialBuffB.icon; iconImages[1].gameObject.SetActive(true); }
                else { iconImages[1].gameObject.SetActive(false); }
            }

            if (baneIconImages != null && baneIconImages.Length > 1 && baneIconImages[1] != null)
            {
                if (initialBaneB != null && initialBaneB.monsterSprite != null) { baneIconImages[1].sprite = initialBaneB.monsterSprite; baneIconImages[1].gameObject.SetActive(true); }
                else { baneIconImages[1].gameObject.SetActive(false); }
            }

            string warningB = $"\n<color=red>【初始劫數】加入：{(initialBaneB != null ? initialBaneB.monsterName : "無")}</color>";
            if (descTexts != null && descTexts.Length > 1 && descTexts[1] != null) { descTexts[1].text = warningB; descTexts[1].gameObject.SetActive(true); }
            else if (titleTexts.Length > 1 && titleTexts[1] != null) { titleTexts[1].text += warningB; }

            optionButtons[1].onClick.RemoveAllListeners();
            optionButtons[1].onClick.AddListener(() => ApplyInitialChoice(initialBuffB, initialBaneB));
        }

        // --- 隱藏其餘按鈕 ---
        for (int i = 2; i < optionButtons.Length; i++) { if (optionButtons[i] != null) optionButtons[i].gameObject.SetActive(false); }
    }

    private void ApplyInitialChoice(UpgradeOption buff, MonsterData bane)
    {
        if (EnemySpawner.instance != null && bane != null)
        {
            EnemySpawner.instance.activeMonsterRoster.Clear();
            EnemySpawner.instance.activeMonsterRoster.Add(bane);
        }
        ApplyUpgrade(buff, 1);
    }

    // ==========================================
    // 波次抽卡與倍率系統 (包含最後一波鎖定 Boss 邏輯)
    // ==========================================
    public void ShowBaneMenu()
    {
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
        RollBaneChoices();
    }

    private void RollBaneChoices()
    {
        List<MonsterData> availableBanes = new List<MonsterData>();

        // 獲取即將到來的波次
        int nextWave = 1;
        if (GameManager.instance != null) nextWave = GameManager.instance.currentWave;

        // ==========================================
        // ✨ 核心修改 2：根據波次，直接從分類庫中把怪物倒進抽卡池！
        // ==========================================
        if (nextWave == 5 || nextWave == 10)
        {
            // 讀取 Boss 庫
            if (bossMonsters != null) availableBanes.AddRange(bossMonsters);
        }
        else if (nextWave == 3 || nextWave == 7 || nextWave == 8)
        {
            // 讀取菁英怪庫
            if (eliteMonsters != null) availableBanes.AddRange(eliteMonsters);
        }
        else
        {
            // 其他波次讀取普通怪庫
            if (normalMonsters != null) availableBanes.AddRange(normalMonsters);
        }

        int optionsToShow = Mathf.Min(3, optionButtons.Length);

        for (int i = 0; i < optionsToShow; i++)
        {
            optionButtons[i].gameObject.SetActive(true);

            if (availableBanes.Count == 0) break;

            int randomBaneIndex = Random.Range(0, availableBanes.Count);
            MonsterData selectedBane = availableBanes[randomBaneIndex];

            // 避免三個選項都抽到同一隻怪物
            availableBanes.RemoveAt(randomBaneIndex);

            // 根據怪物階級給予倍率 (Normal = 1x, Elite = 2x, Boss = 3x)
            int multiplier = (int)selectedBane.tier + 1;

            // 過濾玩家的升級卡池
            List<UpgradeOption> validPool = new List<UpgradeOption>();
            foreach (UpgradeOption option in upgradePool)
            {
                if (option.maxLevel > 0 && option.currentLevel >= option.maxLevel) continue;
                if (option.requiresRotatingSword && !hasRotatingSword) continue;
                if (option.requiresFlyingSword && !hasFlyingSword) continue;

                // 已經解鎖過的流派不再重複出現解鎖選項
                if (option.type == UpgradeType.UnlockRotatingSword && hasRotatingSword) continue;
                if (option.type == UpgradeType.UnlockFlyingSword && hasFlyingSword) continue;

                validPool.Add(option);
            }

            UpgradeOption selectedBuff = null;
            if (validPool.Count > 0) { selectedBuff = validPool[Random.Range(0, validPool.Count)]; }
            else { selectedBuff = new UpgradeOption { upgradeName = "血中送炭 (生命回復)", type = UpgradeType.MaxHealthUp }; }

            // ==========================================
            // ✨ 核心修復：解鎖類技能不吃倍率防呆鎖
            // ==========================================
            if (selectedBuff.type == UpgradeType.UnlockRotatingSword ||
                selectedBuff.type == UpgradeType.UnlockFlyingSword)
            {
                multiplier = 1; // 強制將倍率壓回 1，避免顯示 x2 或 x3
            }

            // UI 文字與倍率提示
            string multiText = multiplier > 1 ? $" <color=yellow>(效果 x{multiplier})</color>" : "";
            if (titleTexts.Length > i && titleTexts[i] != null)
            {
                titleTexts[i].text = selectedBuff.maxLevel > 0
                    ? $"{selectedBuff.upgradeName} (Lv.{selectedBuff.currentLevel + 1}){multiText}"
                    : $"{selectedBuff.upgradeName}{multiText}";
            }

            if (iconImages.Length > i && iconImages[i] != null)
            {
                if (selectedBuff.icon != null)
                {
                    iconImages[i].sprite = selectedBuff.icon;
                    iconImages[i].gameObject.SetActive(true);
                }
                else { iconImages[i].gameObject.SetActive(false); }
            }

            if (baneIconImages.Length > i && baneIconImages[i] != null)
            {
                if (selectedBane.monsterSprite != null)
                {
                    baneIconImages[i].sprite = selectedBane.monsterSprite;
                    baneIconImages[i].gameObject.SetActive(true);
                }
                else { baneIconImages[i].gameObject.SetActive(false); }
            }

            string tierName = selectedBane.tier == MonsterData.MonsterTier.Boss ? "【頭目降臨】" :
                              selectedBane.tier == MonsterData.MonsterTier.Elite ? "【菁英入侵】" : "【普通威脅】";

            if (descTexts != null && descTexts.Length > i && descTexts[i] != null)
            {
                descTexts[i].text = $"\n<color=red>{tierName}加入：{selectedBane.monsterName}</color>";
                descTexts[i].gameObject.SetActive(true);
            }
            else if (titleTexts.Length > i && titleTexts[i] != null)
            {
                titleTexts[i].text += $"\n<color=red>{tierName}加入：{selectedBane.monsterName}</color>";
            }

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => ApplyBaneUpgrade(selectedBuff, selectedBane, multiplier));
        }
    }

    private void ApplyBaneUpgrade(UpgradeOption buff, MonsterData bane, int multiplier)
    {
        ApplyUpgrade(buff, multiplier);

        if (EnemySpawner.instance != null)
        {
            if (bane.tier == MonsterData.MonsterTier.Boss)
            {
                EnemySpawner.instance.RegisterBoss(bane); // 叫出這波選的王

                // ✨ 如果是最後一波，把之前的王全部叫出來大亂鬥！
                if (GameManager.instance != null && GameManager.instance.currentWave >= 10)
                {
                    foreach (var pastBoss in pastBosses)
                    {
                        EnemySpawner.instance.RegisterBoss(pastBoss);
                    }
                }
                pastBosses.Add(bane); // 記錄選過的王
            }
            else
            {
                EnemySpawner.instance.activeMonsterRoster.Add(bane);
            }
        }
    }

    // ==========================================
    // 升級數值實作與動態圖示
    // ==========================================
    void ApplyUpgrade(UpgradeOption option, int multiplier)
    {
        option.currentLevel += multiplier;

        // 更新左上角的被動技能圖示清單
        UpdateIconDisplay(option);

        switch (option.type)
        {
            case UpgradeType.UnlockRotatingSword:
                hasRotatingSword = true;
                if (currentSwordCount == 0) SpawnSwords(1);
                break;

            case UpgradeType.UnlockFlyingSword:
                hasFlyingSword = true;
                PlayerAutoShoot autoShoot = player.GetComponent<PlayerAutoShoot>();
                if (autoShoot != null) autoShoot.enabled = true;
                break;

            case UpgradeType.DamageUp:
                extraSwordDamage += 5 * multiplier;
                UpdateAllSwordsDamage();
                break;

            case UpgradeType.MaxHealthUp:
                PlayerHealth ph = FindObjectOfType<PlayerHealth>();
                if (ph != null) { ph.maxHealth += 20 * multiplier; ph.Heal(20 * multiplier); }
                break;

            case UpgradeType.RotatingSwordCount:
                if (hasRotatingSword) SpawnSwords(1 * multiplier);
                break;

            case UpgradeType.RotatingSwordSpeed:
                rotationSpeed += 40f * multiplier;
                break;

            case UpgradeType.RotatingSwordSize:
                currentSwordScale += 0.2f * multiplier;
                UpdateAllSwordsSize();
                break;

            case UpgradeType.FlyingSwordFireRate:
                PlayerAutoShoot pasRate = player.GetComponent<PlayerAutoShoot>();
                if (pasRate != null)
                {
                    pasRate.fireRate -= 0.15f * multiplier;
                    if (pasRate.fireRate < 0.1f) pasRate.fireRate = 0.1f;
                }
                break;

            case UpgradeType.FlyingSwordCount:
                PlayerAutoShoot pasCount = player.GetComponent<PlayerAutoShoot>();
                if (pasCount != null) pasCount.projectileCount += 1 * multiplier;
                break;

            case UpgradeType.FlyingSwordPierce:
                PlayerAutoShoot pasPierce = player.GetComponent<PlayerAutoShoot>();
                if (pasPierce != null) pasPierce.pierceCount += 1 * multiplier;
                break;
        }

        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();
        ResumeGame();
    }

    void UpdateIconDisplay(UpgradeOption option)
    {
        if (iconContainer == null || iconPrefab == null || option.icon == null) return;

        if (!activeIconUI.ContainsKey(option.type))
        {
            GameObject newIcon = Instantiate(iconPrefab, iconContainer);
            Image img = newIcon.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = option.icon;
                activeIconUI.Add(option.type, img);
            }
        }
    }

    void UpdateAllSwordsSize()
    {
        foreach (GameObject swordHandle in activeSwords)
        {
            if (swordHandle != null && swordHandle.transform.childCount > 0)
            {
                // 抓到我們做好的第二層 ScalePivot
                Transform blade = swordHandle.transform.GetChild(0);

                // ==========================================
                // ✨ 終極修正：橫向圖片要縮放 X 軸！
                // ==========================================
                // 把原本的 new Vector3(1f, currentSwordScale, 1f);
                // 改成 new Vector3(currentSwordScale, 1f, 1f);
                blade.localScale = new Vector3(currentSwordScale, 1f, 1f);
            }
        }
    }

    void UpdateAllSwordsDamage()
    {
        UniversalDamageHitbox[] hitboxes = FindObjectsOfType<UniversalDamageHitbox>();
        foreach (UniversalDamageHitbox h in hitboxes)
        {
            if (h.targetTag == "Enemy")
            {
                // ✨ 核心修正：統一使用「基礎傷害 + 額外傷害」
                h.damage = baseSwordDamage + extraSwordDamage;
            }
        }
    }

    private void SpawnSwords(int amountToIncrease)
    {
        if (player == null || swordHandlePrefab == null) return;
        currentSwordCount += amountToIncrease;

        foreach (GameObject sword in activeSwords) { if (sword != null) Destroy(sword); }
        activeSwords.Clear();

        for (int i = 0; i < currentSwordCount; i++)
        {
            float angle = i * (360f / currentSwordCount);
            Quaternion initialRotation = Quaternion.Euler(0, 0, angle);
            GameObject newSword = Instantiate(swordHandlePrefab, player.position, initialRotation, player);
            newSword.transform.localPosition = Vector3.zero;

            if (newSword.transform.childCount > 0)
            {
                // ==========================================
                // ✨ 這裡也要改成 X 軸放大！
                // ==========================================
                // 把原本的 new Vector3(1f, currentSwordScale, 1f);
                // 改成下面這樣：
                newSword.transform.GetChild(0).localScale = new Vector3(currentSwordScale, 1f, 1f);
            }

            activeSwords.Add(newSword);
        }
        UpdateAllSwordsDamage();
    }

    void ResumeGame()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ==========================================
    // 局內全自動微升級 (不排隊、不連續觸發)
    // ==========================================
    public void ApplyMicroUpgrade()
    {
        // 1. 永久屬性正常疊加 (一口氣升 3 級就疊 3 次)
        extraSwordDamage += 2;
        UpdateAllSwordsDamage();

        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) { ph.maxHealth += 5; ph.Heal(30); }

        if (AudioManager.instance != null) AudioManager.instance.PlayLevelUp();

        // 2. 處理轉速的永久提升
        if (isFeverActive)
        {
            // 如果剛好在狂熱中吃寶石升級，把永久增加的轉速存進「正常值」
            normalRotationSpeed += 5f;
        }
        else
        {
            // 否則正常加上去
            rotationSpeed += 5f;
        }

        // 3. ✨ 核心修正：如果沒有在狂熱狀態，才觸發 3 秒狂熱！(拒絕排隊)
        if (!isFeverActive)
        {
            StartCoroutine(FeverRoutine());
        }
    }

    private IEnumerator FeverRoutine()
    {
        isFeverActive = true; // 上鎖，避免連續觸發

        // 1. 紀錄發動前的正常數值
        normalRotationSpeed = rotationSpeed;
        PlayerAutoShoot autoShoot = player.GetComponent<PlayerAutoShoot>();
        if (autoShoot != null)
        {
            normalFireRate = autoShoot.fireRate;
            // ✨ 已經刪除：紀錄子彈數量的邏輯
        }

        // 3. 🔥 切換為狂熱數值 (只保留極致轉速與極致射速！)
        rotationSpeed = 1000f;
        if (autoShoot != null)
        {
            autoShoot.fireRate = 0.1f;
            // ✨ 已經刪除：增加子彈數量的邏輯
        }

        // 4. ⏳ 維持時間 (依照你原本的設定是 2 秒)
        yield return new WaitForSeconds(2f);

        // 5. 🛑 狂熱結束，數值回歸正常
        rotationSpeed = normalRotationSpeed;
        if (autoShoot != null)
        {
            autoShoot.fireRate = normalFireRate;
            // ✨ 已經刪除：恢復子彈數量的邏輯
        }

        isFeverActive = false; // 解鎖
    }
}