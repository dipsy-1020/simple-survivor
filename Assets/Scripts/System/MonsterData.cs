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
}