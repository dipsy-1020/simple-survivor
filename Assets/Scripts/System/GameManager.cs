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

        // 如果有開始畫面，請根據你的設計決定要不要先暫停。這裡預設遊戲直接開始。
        Time.timeScale = 1f;
        UpdateTimerUI();
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

                // 2. ✨ 核心機制：呼叫 UpgradeManager 彈出「渡劫抉擇面板」！
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