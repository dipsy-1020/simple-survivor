using UnityEngine;

// 讓你可以從 Unity 右鍵選單直接建立這個資料檔
[CreateAssetMenu(fileName = "NewMonsterData", menuName = "簡單的倖存者/怪物食譜")]
public class MonsterData : ScriptableObject
{
    [Header("外觀與體型")]
    public string monsterName = "未命名怪物";
    public Color monsterColor = Color.white; // 預設白色
    public float scaleMultiplier = 1f;       // 預設大小 1 倍

    [Header("基礎數值")]
    public int maxHealth = 50;
    public float moveSpeed = 2f;

    [Header("行為模塊切換")]
    public bool useMeleeAttack = true; // 是否掛載近戰衝撞模組
    public bool useRangeAttack = false;// 是否掛載遠程射擊模組

    // 定義怪物的大腦類型
    public enum AIType { StraightChaser, SpiderCharge, BatKite }
    public AIType brainType = AIType.StraightChaser;
}