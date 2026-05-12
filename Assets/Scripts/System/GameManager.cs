using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ✨ 單例模式，方便其他腳本 (如玩家血量) 呼叫 GameOver
    public static GameManager instance;

    [Header("UI 介面")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI timerText;

    [Header("波次設定 (渡劫系統)")]
    public int currentWave = 1;
    public int maxWaves = 10;          // 總共 10 波
    public float waveDuration = 60f;   // 每波 60 秒
    private float waveTimer;

    private bool isGameOver = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 遊戲開始時，初始化第一波的時間
        waveTimer = waveDuration;

        // ✨ 修改這裡：遊戲開局直接凍結時間，並呼叫 UpgradeManager 彈出二選一
        Time.timeScale = 0f;
        UpdateTimerUI();

        UpgradeManager um = FindObjectOfType<UpgradeManager>();
        if (um != null)
        {
            um.ShowInitialMenu(); // 呼叫我們即將寫好的開局專用選單
        }
    }

    void Update()
    {
        // 如果遊戲結束或已經暫停（例如正在選升級），就不繼續倒數
        if (isGameOver || Time.timeScale <= 0f) return;

        // 波次倒數計時
        waveTimer -= Time.deltaTime;
        UpdateTimerUI();

        // 當這波時間結束時！
        if (waveTimer <= 0f)
        {
            if (currentWave < maxWaves)
            {
                // 1. 進入下一波
                currentWave++;
                waveTimer = waveDuration;

                // 2. ✨ 核心機制：全圖清場與吸取寶石！
                ClearBoardAndSuckGems();

                // 3. 呼叫 UpgradeManager 彈出「渡劫抉擇面板」！
                UpgradeManager um = FindObjectOfType<UpgradeManager>();
                if (um != null)
                {
                    um.ShowBaneMenu();
                }
                else
                {
                    Debug.LogWarning("場上找不到 UpgradeManager，無法跳出渡劫面板！");
                }
            }
            else
            {
                // 如果已經是最後一波（第 10 波）且倒數完畢，代表玩家存活下來了！
                Victory();
            }
        }
    }

    // 更新畫面上方的 UI 文字
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 將剩餘秒數無條件進位，顯示得比較好看
            int secondsLeft = Mathf.CeilToInt(waveTimer);

            // 顯示格式例如：「第 1 / 10 波 | 剩餘：45 秒」
            timerText.text = $"第 {currentWave} / {maxWaves} 波 | 剩餘：{secondsLeft} 秒";
        }
    }

    // ✨ 創造波次感的核心：波次結算清場
    private void ClearBoardAndSuckGems()
    {
        // 1. 瞬間抹殺場上所有怪物 (直接 Destroy，不呼叫 Die 以免噴一堆特效跟寶石)
        EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }

        // 1.5. 順便把場上還在飛的敵方子彈也清掉，保證絕對安全
        EnemyProjectile[] allBullets = FindObjectsOfType<EnemyProjectile>();
        foreach (EnemyProjectile bullet in allBullets)
        {
            Destroy(bullet.gameObject);
        }

        // 2. 全圖寶石大磁鐵！把地上的寶石全部吸給主角
        Gem[] allGems = FindObjectsOfType<Gem>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            foreach (Gem gem in allGems)
            {
                // 呼叫寶石的 StartFlying 方法，朝主角飛去
                gem.StartFlying(playerObj.transform);
            }
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // 給 UI 按鈕綁定的重新開始方法
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 給 UI 按鈕綁定的返回主選單方法 (假設你的主選單場景叫 "MainMenu")
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}