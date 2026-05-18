using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "簡單的倖存者/怪物食譜")]
public class MonsterData : ScriptableObject
{
    // ✨ 新增階級定義
    public enum MonsterTier { Normal, Elite, Boss }

    [Header("外觀與體型")]
    public string monsterName = "未命名怪物";
    public Sprite monsterSprite;
    public Color monsterColor = Color.white;
    public float scaleMultiplier = 1f;

    // ✨ 讓你在 Unity 面板可以設定這隻怪是普通、菁英還是頭目
    [Header("✨ 怪物強度階級")]
    public MonsterTier tier = MonsterTier.Normal;

    [Header("基礎數值")]
    public int maxHealth = 50;
    public float moveSpeed = 2f;

    [Header("行為模塊切換")]
    public bool useMeleeAttack = true;
    public bool useRangeAttack = false;
    public GameObject projectilePrefab;

    public enum AIType { StraightChaser, SpiderCharge, BatKite }
    public AIType brainType = AIType.StraightChaser;

    // ==========================================
    // ✨ 新增：各 AI 專屬的詳細技能參數設定區
    // ==========================================
    [Header("【衝刺怪 (Spider)】專屬設定")]
    public float chargeDistance = 6f;  // 索敵衝刺距離
    public float chargePrepTime = 0.5f; // 衝刺前搖警告時間
    public float chargeCooldown = 3f;  // 衝刺冷卻時間

    [Header("【遠程怪 (Bat)】專屬設定")]
    public float stoppingDistance = 5f; // 停下來射擊的安全距離
    public float fireRate = 2f;         // 攻擊頻率(秒)
    public int rangedAttackDamage = 10; // 子彈傷害

    [Header("【頭目 (Boss)】專屬設定")]
    public float bossAttackInterval = 4f; // Boss 放招間隔
    public int bossBulletDamage = 20;     // Boss 彈幕傷害
}