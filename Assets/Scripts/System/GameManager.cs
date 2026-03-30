using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // ✨ 處理 TextMeshPro 必備！

public class GameManager : MonoBehaviour
{
    [Header("UI 介面")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel; // ✨ 新增：勝利畫面
    public TextMeshProUGUI timerText; // ✨ 新增：畫面上方的計時器文字

    [Header("遊戲時間設定")]
    public float gameTime = 0f; // 遊戲已經進行的總秒數
    public float winTime = 300f; // 獲勝目標時間 (預設 300 秒 = 5 分鐘)
    private bool isGameOver = false; // 防止重複觸發結算

    void Update()
    {
        // 如果遊戲還沒結束，時間就繼續走
        if (!isGameOver)
        {
            gameTime += Time.deltaTime;
            UpdateTimerUI();

            // 檢查是否達到獲勝目標時間
            if (gameTime >= winTime)
            {
                ShowVictory();
            }
        }
    }

    // ✨ 將單調的秒數轉換為 00:00 的漂亮格式
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 算出分鐘數與秒數
            int minutes = Mathf.FloorToInt(gameTime / 60F);
            int seconds = Mathf.FloorToInt(gameTime - minutes * 60);

            // string.Format 會把數字填進 {0} 跟 {1} 裡，:00 代表必定顯示兩位數 (如 05:09)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ✨ 新增：觸發勝利的邏輯
    public void ShowVictory()
    {
        if (isGameOver) return;
        isGameOver = true;

        victoryPanel.SetActive(true);
        Time.timeScale = 0f; // 凍結時間

        // 把畫面上所有的敵人都清掉 (選做，讓勝利畫面更乾淨)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}