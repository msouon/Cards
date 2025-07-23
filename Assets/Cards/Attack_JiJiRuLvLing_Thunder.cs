using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack_JiJiRuLvLing_Thunder", menuName = "Cards/Attack/急急如律令(雷)")]
public class Attack_JiJiRuLvLing_Thunder : AttackCardBase
{
    public int baseDamage = 6;

    private void OnEnable() { cardType = CardType.Attack; }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = enemy.ApplyElementalAttack(ElementType.Thunder, baseDamage, player);
        enemy.TakeDamage(dmg);
        
    }
}
