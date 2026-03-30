using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ✨ 單例模式 (Singleton)：讓其他腳本不用 Find 就能直接呼叫它
    public static AudioManager instance;

    [Header("喇叭 (Audio Source)")]
    public AudioSource sfxSource;

    [Header("音效檔案 (Audio Clips)")]
    public AudioClip gemSound;      // 吃寶石的叮噹聲
    public AudioClip levelUpSound;  // 升級的音效
    public AudioClip hitSound;      // 怪物受傷的音效 (選用)

    void Awake()
    {
        // 確保整個場景只有一個 AudioManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 播放吃寶石音效的方法
    public void PlayGem()
    {
        // PlayOneShot 的好處是：連續吃 10 顆寶石，它會疊加播放 10 次，不會互相切斷！
        if (gemSound != null && sfxSource != null)
        {
            // 因為寶石數量太多，可以稍微調低一點音量 (例如 0.5f)，避免玩家耳朵炸裂
            sfxSource.PlayOneShot(gemSound, 0.5f);
        }
    }

    // 播放升級音效的方法
    public void PlayLevelUp()
    {
        if (levelUpSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(levelUpSound, 1.0f);
        }
    }

    // 播放怪物受擊音效的方法
    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            // 因為怪海數量很多，一秒鐘可能會砍中十幾次
            // 音量一定要調小一點（例如 0.3f），才不會變成噪音干擾
            sfxSource.PlayOneShot(hitSound, 0.3f);
        }
    }
}