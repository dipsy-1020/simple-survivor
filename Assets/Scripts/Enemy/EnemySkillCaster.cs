using UnityEngine;

public class EnemySkillCaster : MonoBehaviour
{
    private DashSkill dashSkill;
    private Transform player;

    void Start()
    {
        dashSkill = GetComponent<DashSkill>(); // 把身上的技能包抓過來
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // 只要玩家還活著，就瘋狂嘗試對玩家按下「衝撞技能」！
        // (不用擔心會連衝，因為 DashSkill 裡面有 Cooldown 冷卻保護機制)
        if (player != null && dashSkill != null)
        {
            dashSkill.TryDash(player.position);
        }
    }
}