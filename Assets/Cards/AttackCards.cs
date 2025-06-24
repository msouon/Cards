using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 攻擊牌1-4：早期設計

/// <summary>
/// 急急如律令
/// </summary>
[CreateAssetMenu(fileName = "Attack_JiJiRuLvLing", menuName = "Cards/Attack/急急如律令")]
public class Attack_JiJiRuLvLing : AttackCardBase
{
    [Header("基礎傷害")]
    public int baseDamage = 6;
    [Header("對爆走目標的額外真實傷害")]
    public int extraTrueDamage = 3;
    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        Debug.Log($"ExecuteEffect called with cost={cost}");
        // 1. 檢查敵人是否處於爆走狀態(此示範直接用 enemy.hasBerserk 之類)
        //   (視情況自己在 Enemy.cs 加bool hasBerserk)
        int totalDamage = baseDamage;

        if (enemy.hasBerserk)
        {
            // 額外真實傷害
            // 真實傷害可考慮直接扣 enemy.currentHP，或繞過block
            enemy.TakeTrueDamage(extraTrueDamage);
        }

        // 2. 普通傷害 (套用玩家攻擊加成)
        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);

    }
}

/// <summary>
/// 天罰 (AOE 攻擊, 若有陰氣或雷電等元素場景可加成)
/// </summary>
[CreateAssetMenu(fileName = "Attack_TianFa", menuName = "Cards/Attack/天罰")]
public class Attack_TianFa : CardBase
{
    public int aoeDamage = 5;       // 對全體敵人基礎傷害
    public int elementBonus = 2;    // 若場上有陰氣/雷電時的加成

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 本示範只有單一 Enemy，但若有多敵人可改成:
        // foreach(Enemy e in battleManager.enemyList) e.TakeDamage(aoeDamage)
        int totalDamage = aoeDamage;

        // 假設我們在 GameManager 或 BattleManager 有 isYinQiPresent / isThunderPresent 之類的bool
        bool isYinQiOrThunder = false; // 你可改成實際檢查
        if (isYinQiOrThunder)
        {
            totalDamage += elementBonus;
        }

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// 驅邪 (低費破增益)
/// </summary>
[CreateAssetMenu(fileName = "Attack_QuXie", menuName = "Cards/Attack/驅邪")]
public class Attack_QuXie : CardBase
{
    public int damage = 4;
    public int dispelCount = 1; // 可驅散敵人增益層數

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg);
        // 依需求，移除敵人的增益buff
        enemy.DispelBuff(dispelCount);
    }
}

/// <summary>
/// 真訓 (若本回合使用過防禦牌, 則+額外傷害)
/// </summary>
[CreateAssetMenu(fileName = "Attack_ZhenXun", menuName = "Cards/Attack/真訓")]
public class Attack_ZhenXun : CardBase
{
    public int baseDamage = 10;
    public int bonusDamage = 4;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 假設 Player 有個 bool usedDefenseThisTurn 做紀錄
        // 或 check block>0 也可以
        bool usedDefense = (player.block > 0); // 簡化用: 若有block, 表示用過防禦
        int totalDamage = baseDamage;
        if (usedDefense)
        {
            totalDamage += bonusDamage;
        }

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

#endregion

#region 攻擊牌5-10：新增設計

/// <summary>
/// 靈巧穿刺
/// </summary>
[CreateAssetMenu(fileName = "Attack_LingQiaoChuanCi", menuName = "Cards/Attack/靈巧穿刺")]
public class Attack_LingQiaoChuanCi : CardBase
{
    public int baseDamage = 6;
    public int bonusDamageIfDiscard = 3;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 檢查本回合是否棄牌
        bool hasDiscarded = player.hasDiscardedThisTurn;
        int totalDamage = baseDamage;
        if (hasDiscarded)
        {
            totalDamage += bonusDamageIfDiscard;
        }
        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// 燃盡斬 (可棄1張牌再打一次)
/// </summary>
[CreateAssetMenu(fileName = "Attack_RanJinZhan", menuName = "Cards/Attack/燃盡斬")]
public class Attack_RanJinZhan : CardBase
{
    public int damage = 5;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int dmg1 = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg1);

        // 棄 1 張牌 -> 若成功，再次攻擊
        bool hasDiscard = player.DiscardOneCard();
        if (hasDiscard)
        {
            int dmg2 = player.CalculateAttackDamage(damage);
            enemy.TakeDamage(dmg2);
        }
    }
}

/// <summary>
/// 碎甲衝擊 (破除敵方部分防禦)
/// </summary>
[CreateAssetMenu(fileName = "Attack_SuiJiaChongJi", menuName = "Cards/Attack/碎甲衝擊")]
public class Attack_SuiJiaChongJi : CardBase
{
    public int damage = 8;
    public int reduceBlock = 5;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 先破除敵方 block
        enemy.ReduceBlock(reduceBlock);
        // 再造成傷害
        int dmg = player.CalculateAttackDamage(damage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// 盾擊 (若本回合格擋 >= N , 額外傷害)
/// </summary>
[CreateAssetMenu(fileName = "Attack_DunJi", menuName = "Cards/Attack/盾擊")]
public class Attack_DunJi : CardBase
{
    public int baseDamage = 4;
    public int blockThreshold = 6;
    public int bonusDamage = 4;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        int totalDamage = baseDamage;
        if (player.block >= blockThreshold)
        {
            totalDamage += bonusDamage;
        }
        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// 亂流手裡劍 (棄牌次數越多, 傷害越高)
/// </summary>
[CreateAssetMenu(fileName = "Attack_LuanLiuShuriken", menuName = "Cards/Attack/亂流手裡劍")]
public class Attack_LuanLiuShuriken : CardBase
{
    public int baseDamagePerDiscard = 2;
    public int baseDamageIfNoDiscard = 2; // 若無棄牌紀錄, 給個保底傷害

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 若本回合棄牌過幾張? 此示範只記錄 hasDiscardedThisTurn (bool)
        // 若想更細, 可以在 Player 裡紀錄 discardCountThisTurn (int)
        bool hasDiscarded = player.hasDiscardedThisTurn;
        int discardCount = player.discardCountThisTurn; // 假設你在 Player 中做了此變數
        if (discardCount < 0) discardCount = 0;

        int totalDamage = 0;
        if (!hasDiscarded)
        {
            // 無棄牌情況
            totalDamage = baseDamageIfNoDiscard;
        }
        else
        {
            // 有棄牌, 依棄牌數計算
            totalDamage = discardCount * baseDamagePerDiscard;
            if (totalDamage <= 0) totalDamage = baseDamageIfNoDiscard;
        }

        int dmg = player.CalculateAttackDamage(totalDamage);
        enemy.TakeDamage(dmg);
    }
}

/// <summary>
/// 騙術突襲 (抽1棄1, 若棄掉的是技能牌則追加傷害)
/// </summary>
[CreateAssetMenu(fileName = "Attack_PianShuTuXi", menuName = "Cards/Attack/騙術突襲")]
public class Attack_PianShuTuXi : CardBase
{
    public int baseDamage = 9;
    public int bonusDamage = 3;

    private void OnEnable()
    {
        cardType = CardType.Attack;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 造成基礎傷害
        int dmg = player.CalculateAttackDamage(baseDamage);
        enemy.TakeDamage(dmg);

        // 抽1張
        player.DrawCards(1);
        // 棄1張(此處簡化, 直接棄最後一張)
        CardBase lastCard = null;
        if (player.hand.Count > 0)
        {
            lastCard = player.hand[player.hand.Count - 1];
            player.hand.RemoveAt(player.hand.Count - 1);
            player.discardPile.Add(lastCard);
            player.hasDiscardedThisTurn = true;
            player.discardCountThisTurn++; // 需確保有此紀錄
        }

        // 若棄掉的牌是技能牌, 則對敵人再造成 bonusDamage
        if (lastCard != null && lastCard.cardType == CardType.Skill)
        {
            int bonus = player.CalculateAttackDamage(bonusDamage);
            enemy.TakeDamage(bonus);
        }

        player.UseEnergy(cost);
    }
}

#endregion

