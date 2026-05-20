using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("音軌發射器")]
    public AudioSource sfxSource; // 記得在同一個物件上掛 Audio Source 組件並拖進來

    [Header("音效檔案")]
    public AudioClip hitClip;     // 打擊音效
    public AudioClip expClip;     // 吸經驗音效
    public AudioClip levelUpClip; // 升級音效

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 播放打擊
    public void PlayHit()
    {
        if (hitClip != null && sfxSource != null) sfxSource.PlayOneShot(hitClip);
    }

    // 播放吸經驗 (稍微調小聲一點 0.5f，不然滿地寶石一起吸會破音)
    public void PlayExp()
    {
        if (expClip != null && sfxSource != null) sfxSource.PlayOneShot(expClip, 0.5f);
    }

    // 播放升級
    public void PlayLevelUp()
    {
        if (levelUpClip != null && sfxSource != null) sfxSource.PlayOneShot(levelUpClip);
    }
}