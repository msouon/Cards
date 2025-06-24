using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack_JiJiRuLvLing_Fire", menuName = "Cards/Attack/«æ«æ¦p«ß¥O(¤õ)")]
public class Attack_JiJiRuLvLing_Fire : AttackCardBase
{
    public int baseDamage = 6;

    private void OnEnable() { cardType = CardType.Attack; }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = enemy.ApplyElementalAttack(ElementType.Fire, baseDamage, player);
        enemy.TakeDamage(dmg);
        player.UseEnergy(cost);
    }
}

[CreateAssetMenu(fileName = "Attack_JiJiRuLvLing_Thunder", menuName = "Cards/Attack/«æ«æ¦p«ß¥O(¹p)")]
public class Attack_JiJiRuLvLing_Thunder : AttackCardBase
{
    public int baseDamage = 6;

    private void OnEnable() { cardType = CardType.Attack; }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = enemy.ApplyElementalAttack(ElementType.Thunder, baseDamage, player);
        enemy.TakeDamage(dmg);
        player.UseEnergy(cost);
    }
}

[CreateAssetMenu(fileName = "Attack_JiJiRuLvLing_Ice", menuName = "Cards/Attack/«æ«æ¦p«ß¥O(¦B)")]
public class Attack_JiJiRuLvLing_Ice : AttackCardBase
{
    public int baseDamage = 6;

    private void OnEnable() { cardType = CardType.Attack; }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = enemy.ApplyElementalAttack(ElementType.Ice, baseDamage, player);
        enemy.TakeDamage(dmg);
        Board board = GameObject.FindObjectOfType<Board>();
        if (board)
        {
            foreach (var adjE in GameObject.FindObjectsOfType<Enemy>())
            {
                if (adjE == enemy) continue;
                if (Vector2Int.Distance(adjE.gridPosition, enemy.gridPosition) <= 1.1f)
                {
                    adjE.AddElementTag(ElementType.Ice);
                }
            }
        }
        player.UseEnergy(cost);
    }
}

