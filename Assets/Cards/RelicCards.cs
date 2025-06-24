using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 5 種遺物

/// <summary>
/// 輪迴竹簡 - 棄牌後獲得格擋, 一次棄2張以上時再抽1
/// </summary>
[CreateAssetMenu(fileName = "Relic_LunHuiZhuJian", menuName = "Cards/Relic/輪迴竹簡")]
public class Relic_LunHuiZhuJian : CardBase
{
    public int blockPerDiscard = 1;
    public int extraDraw = 1;

    private void OnEnable()
    {
        cardType = CardType.Relic;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 不會主動被打出
    }

    public void OnPlayerDiscard(Player player, int discardCount)
    {
        int totalBlock = discardCount * blockPerDiscard;
        player.AddBlock(totalBlock);
        if (discardCount >= 2)
        {
            player.DrawCards(extraDraw);
        }
    }
}

/// <summary>
/// 百寶囊 - 戰鬥開始抽1, 每回合若打出至少2張攻擊再用1技能時抽1
/// </summary>
[CreateAssetMenu(fileName = "Relic_BaiBaoNang", menuName = "Cards/Relic/百寶囊")]
public class Relic_BaiBaoNang : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Relic;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
    }

    public void OnBattleStart(Player player)
    {
        player.DrawCards(1);
    }

    /// <summary>
    /// 可在BattleManager中檢查：若本回合已打出2張攻擊，再打出技能則抽1
    /// </summary>
    public void OnPlayerUseSkillAfterTwoAttacks(Player player)
    {
        player.DrawCards(1);
    }
}

/// <summary>
/// 紫電角 - 每次獲得>=6格擋就對下次攻擊+2傷害(若格擋>=12再+2=+4)
/// </summary>
[CreateAssetMenu(fileName = "Relic_ZiDianJiao", menuName = "Cards/Relic/紫電角")]
public class Relic_ZiDianJiao : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Relic;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
    }

    public void OnAddBlock(Player player, int blockAdded)
    {
        if (blockAdded >= 6 && blockAdded < 12)
        {
            player.buffs.nextAttackPlus += 2;
        }
        else if (blockAdded >= 12)
        {
            player.buffs.nextAttackPlus += 4;
        }
    }
}

/// <summary>
/// 枯木書籤 - 回合開始可選擇棄1張牌, 若棄則本回合移動牌費用-1
/// </summary>
[CreateAssetMenu(fileName = "Relic_KuMuShuQian", menuName = "Cards/Relic/枯木書籤")]
public class Relic_KuMuShuQian : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Relic;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
    }

    public void OnTurnStart(Player player)
    {
        // 由UI問玩家 "要不要丟1張牌"?
        // 這裡簡化
        bool wantDiscard = true;
        if (wantDiscard && player.hand.Count > 0)
        {
            player.DiscardOneCard();
            // 移動卡費用-1
            player.buffs.movementCostModify -= 1;
        }
    }
}

/// <summary>
/// 破魔簫 - 每回合若使用>=3張攻擊牌, 回合結束時抽1棄1, 若棄掉的是攻擊牌則下回合攻擊+1(累積)
/// </summary>
[CreateAssetMenu(fileName = "Relic_PoMoXiao", menuName = "Cards/Relic/破魔簫")]
public class Relic_PoMoXiao : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Relic;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
    }

    public void OnEndTurn(Player player, int attackCardUsedThisTurn)
    {
        if (attackCardUsedThisTurn >= 3)
        {
            // 抽1
            player.DrawCards(1);
            // 棄1 (簡化為棄最後一張)
            CardBase last = null;
            if (player.hand.Count > 0)
            {
                last = player.hand[player.hand.Count - 1];
                player.hand.RemoveAt(player.hand.Count - 1);
                player.discardPile.Add(last);
                player.hasDiscardedThisTurn = true;
                player.discardCountThisTurn++;
            }
            // 若棄掉的是攻擊牌 => 下回合攻擊+1
            if (last != null && last.cardType == CardType.Attack)
            {
                player.buffs.nextTurnAllAttackPlus += 1;
            }
        }
    }
}

#endregion
