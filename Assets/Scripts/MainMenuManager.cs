using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("面板綁定")]
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject levelSelectPanel;

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    // ========================
    // 核心邏輯：傳遞「關卡編號」
    // ========================
    public void LoadLevel(int levelIndex)
    {
        // 將玩家選擇的關卡編號 (0代表Easy, 1代表Normal) 存起來
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        PlayerPrefs.Save();

        // 永遠載入遊戲場景 (Build Settings 裡的 Index 1)
        SceneManager.LoadScene(1);
    }

    public void OpenCredits() { if (creditsPanel != null) creditsPanel.SetActive(true); }
    public void CloseCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }
    public void QuitGame() { Application.Quit(); }
}