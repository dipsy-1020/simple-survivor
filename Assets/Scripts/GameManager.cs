using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("血量 UI 綁定")]
    public Image hpImage;
    public TextMeshProUGUI hpText;

    [Header("經驗值 UI 綁定")]
    public Image expImage;
    public TextMeshProUGUI levelText;

    [Header("計時器 UI 綁定")]
    public TextMeshProUGUI timerText;    // 拖入你的 TimerText 物件
    private float gameTimer = 0f;

    [Header("結束面板頁面")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        // 只有在遊戲正常跑、沒暫停的時候才跑計時器
        if (Time.timeScale > 0f)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // 更新時間文字
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            // 格式化成兩位數的 分:秒 (例如 01:23)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // --- UI 更新方法 ---
    public void UpdateHPUI(int currentHP, int maxHP)
    {
        if (hpImage != null) hpImage.fillAmount = (float)currentHP / maxHP;
        if (hpText != null) hpText.text = $"{currentHP} / {maxHP}";
    }

    public void UpdateExpUI(int currentExp, int maxExp, int currentLevel)
    {
        if (expImage != null) expImage.fillAmount = (float)currentExp / maxExp;
        if (levelText != null) levelText.text = $"LV. {currentLevel}";
    }

    // --- 結束狀態 ---
    public void ShowVictory()
    {
        Time.timeScale = 0f;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            GoToMainMenu();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}