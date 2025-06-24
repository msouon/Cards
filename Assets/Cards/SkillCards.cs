using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 技能牌1-5：早期設計

/// <summary>
/// 護我真身 (防禦+下次攻擊傷害+2)
/// </summary>
[CreateAssetMenu(fileName = "Skill_HuWoZhenShen", menuName = "Cards/Skill/護我真身")]
public class Skill_HuWoZhenShen : CardBase
{
    public int blockValue = 8;
    public int nextAttackBoost = 2;

    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // +防禦
        player.AddBlock(blockValue);
        // 下張攻擊傷害+2 => 可以在 Player 中設個 buffs.nextAttackPlus = nextAttackBoost;
        player.buffs.nextAttackPlus = nextAttackBoost; // 需你在 Player 內部定義 buffs
    }
}

/// <summary>
/// 神威 (給自己格擋 + 敵人下次受到傷害 +2)
/// </summary>
[CreateAssetMenu(fileName = "Skill_ShenWei", menuName = "Cards/Skill/神威")]
public class Skill_ShenWei : CardBase
{
    public int blockValue = 5;
    public int enemyNextDamageUp = 2;

    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 增加自身格擋
        player.AddBlock(blockValue);

        // 敵人下次受的傷害+2 => 在 Enemy 裡可做 e.buffs.nextDamageTakenUp = 2;
        //enemy.buffs.nextDamageTakenUp = enemyNextDamageUp;

    }
}

/// <summary>
/// 靈魂震盪 (獲得1能量, 但陷入虛弱1回合 + 施加雷電效果)
/// </summary>
[CreateAssetMenu(fileName = "Skill_LingHunZhenDang", menuName = "Cards/Skill/靈魂震盪")]
public class Skill_LingHunZhenDang : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 獲得1能量 (能量上限需自行管理)
        player.energy += 1;
        // 令玩家虛弱 1 回合 => player.buffs.weak = 1;
        player.buffs.weak += 1;

        // 對場上施加雷電效果 => 可能由 BattleManager 或 GameManager 控制
        // GameManager.instance.isThunderPresent = true; (依你設計)

    }
}

/// <summary>
/// 神臨 (使單體敵人無法行動1回合, 下回合抽牌-1)
/// </summary>
[CreateAssetMenu(fileName = "Skill_ShenLin", menuName = "Cards/Skill/神臨")]
public class Skill_ShenLin : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 使敵人無法行動
        enemy.buffs.stun = 1; // 1回合

        // 下回合抽牌 -1
        player.buffs.nextTurnDrawChange -= 1;

    }
}

/// <summary>
/// 神降 (本回合我方受傷減半, 若多人模式可作用全隊)
/// </summary>
[CreateAssetMenu(fileName = "Skill_ShenJiang", menuName = "Cards/Skill/神降")]
public class Skill_ShenJiang : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 本回合受傷減半
        player.buffs.damageTakenRatio = 0.5f; // 讓玩家本回合只受50%傷害
        // 下回合可重置
    }
}

#endregion

#region 技能牌6-10：新增設計

/// <summary>
/// 不滅意志 (抽2張, 棄2張)
/// </summary>
[CreateAssetMenu(fileName = "Skill_BuMieYiZhi", menuName = "Cards/Skill/不滅意志")]
public class Skill_BuMieYiZhi : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 抽2
        player.DrawCards(2);
        // 棄2
        player.DiscardCards(2);
    }
}

/// <summary>
/// 強化護盾 (基礎防禦, 若手牌中計畫棄牌/或已棄牌, 則額外防禦)
/// </summary>
[CreateAssetMenu(fileName = "Skill_QiangHuaHuDun", menuName = "Cards/Skill/強化護盾")]
public class Skill_QiangHuaHuDun : CardBase
{
    public int baseBlock = 7;
    public int bonusBlock = 3;

    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        bool isPlanToDiscard = player.CheckDiscardPlan();
        int totalBlock = baseBlock;
        if (isPlanToDiscard)
        {
            totalBlock += bonusBlock;
        }
        player.AddBlock(totalBlock);
    }
}

/// <summary>
/// 誘敵之策 (0費：棄1張牌, 下1張攻擊卡費用-1)
/// </summary>
[CreateAssetMenu(fileName = "Skill_YouDiZhiCe", menuName = "Cards/Skill/誘敵之策")]
public class Skill_YouDiZhiCe : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 棄1張牌
        bool success = player.DiscardOneCard();
        if (success)
        {
            // 使下1張攻擊卡 cost-1 => 需Player有個 nextAttackCostModify = -1
            player.buffs.nextAttackCostModify -= 1;
        }
    }
}

/// <summary>
/// 翻箱倒櫃 (抽3, 回合結束隨機棄2)
/// </summary>
[CreateAssetMenu(fileName = "Skill_FanXiangDaoGui", menuName = "Cards/Skill/翻箱倒櫃")]
public class Skill_FanXiangDaoGui : CardBase
{
    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 抽3
        player.DrawCards(3);

        // 在 BattleManager 或 Player 端的 "EndTurn" 時檢查:
        // if(本回合用過翻箱倒櫃) => 隨機棄2
        // 這裡僅記錄一個flag
        player.buffs.needRandomDiscardAtEnd = 2; // 回合結束隨機棄2
    }
}

/// <summary>
/// 徹底防禦 (高額防禦, 但下回合抽牌-1)
/// </summary>
[CreateAssetMenu(fileName = "Skill_CheDiFangYu", menuName = "Cards/Skill/徹底防禦")]
public class Skill_CheDiFangYu : CardBase
{
    public int blockValue = 12;

    private void OnEnable()
    {
        cardType = CardType.Skill;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        player.AddBlock(blockValue);
        // 下回合抽牌 -1
        player.buffs.nextTurnDrawChange -= 1;
    }
}

#endregion

