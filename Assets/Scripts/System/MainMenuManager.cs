using UnityEngine;
using UnityEngine.SceneManagement; // ✨ 切換場景必備

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // ✨ 載入你的遊戲關卡 (請確保括號裡的名字跟你遊戲場景的檔名一模一樣)
        // 如果你之前把場景改名叫 MainLevel，這裡就要打 "MainLevel"
        SceneManager.LoadScene("MainLevel");
    }

    public void QuitGame()
    {
        // 離開遊戲 (在 Unity 編輯器裡按了沒反應，但打包成 EXE 之後就會關閉視窗)
        Debug.Log("離開遊戲！");
        Application.Quit();
    }
}