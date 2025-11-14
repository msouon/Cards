using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 招魂（消耗：本場戰鬥不再抽到；讓當前手牌的費用 -1）
/// </summary>
[CreateAssetMenu(fileName = "Skill_ZhaoHun", menuName = "Cards/Skill/招魂")]
public class Skill_ZhaoHun : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
        exhaustOnUse = true;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        if (player == null)
        {
            return;
        }

        List<CardBase> currentHand = player.Hand;
        if (currentHand == null)
        {
            return;
        }

        foreach (CardBase card in currentHand)
        {
            if (card == null || card == this)
            {
                continue;
            }

            player.AddCardCostModifier(card, -1);
        }
    }
}