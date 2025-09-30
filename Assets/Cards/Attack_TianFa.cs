using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天罰（AOE 可擴充）
/// </summary>
[CreateAssetMenu(fileName = "Attack_TianFa", menuName = "Cards/Attack/�ѻ@")]
public class Attack_TianFa : CardBase
{
    public int aoeDamage = 5;    

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int totalDamage = aoeDamage;

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}