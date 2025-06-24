using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("屬性")]
    public int maxHP = 50;
    public int currentHP;
    public int energy = 5;
    public int block = 0;

    [Header("卡牌管理")]
    public List<CardBase> deck = new List<CardBase>();
    public List<CardBase> hand = new List<CardBase>();
    public List<CardBase> discardPile = new List<CardBase>();
    public List<CardBase> relics = new List<CardBase>();  // 收集到的遺物

    [Header("回合內紀錄")]
    public bool hasDiscardedThisTurn = false;  // 是否棄過牌
    public int discardCountThisTurn = 0;       // 棄牌數
    public int attackUsedThisTurn = 0;         // 本回合使用的攻擊牌數

    // 簡易紀錄玩家位置(若做2D網格地圖)
    public Vector2Int position = new Vector2Int(0, 0);

    // Buff結構
    public PlayerBuffs buffs = new PlayerBuffs();

    private void Awake()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// 回合開始時調用
    /// </summary>
    public void StartTurn()
    {
        block = 0;  // 視遊戲需求，若是SLAY THE SPIRE風格，回合結束會清除block
        energy = 5; // 重置能量(也可視遊戲規則)
        hasDiscardedThisTurn = false;
        discardCountThisTurn = 0;
        attackUsedThisTurn = 0;
        DrawCards(5); // 例如抽5張
        BattleManager bm = FindObjectOfType<BattleManager>();
        bm.RefreshHandUI();

        // 回合開始buff處理 (如 movementCostModify歸零, damageTakenRatio重置等)
        buffs.OnTurnStartReset();
        // 若有遺物 "枯木書籤" => OnTurnStart
        foreach (CardBase r in relics)
        {
            if (r is Relic_KuMuShuQian kk)
            {
                kk.OnTurnStart(this);
            }
        }
    }

    /// <summary>
    /// 回合結束時（在 BattleManager 裡呼叫）
    /// </summary>
    public void EndTurn()
    {
        // 若使用破魔簫 => OnEndTurn
        foreach (CardBase r in relics)
        {
            if (r is Relic_PoMoXiao pmx)
            {
                pmx.OnEndTurn(this, attackUsedThisTurn);
            }
        }

        // 假設「翻箱倒櫃」需要在回合結束隨機棄2 => 
        if (buffs.needRandomDiscardAtEnd > 0)
        {
            int n = buffs.needRandomDiscardAtEnd;
            buffs.needRandomDiscardAtEnd = 0;
            // 隨機丟 n 張
            for (int i = 0; i < n; i++)
            {
                if (hand.Count > 0)
                {
                    int idx = Random.Range(0, hand.Count);
                    CardBase c = hand[idx];
                    hand.RemoveAt(idx);
                    discardPile.Add(c);
                    hasDiscardedThisTurn = true;
                    discardCountThisTurn++;
                }
            }
        }

        // 回合結束，將 damageTakenRatio 恢復1.0f 或看你設計
        // buff等做相應處理
        buffs.OnTurnEndReset();
    }

    /// <summary>
    /// 扣能量
    /// </summary>
    public void UseEnergy(int cost)
    {
        Debug.Log($"UseEnergy: deducting {cost} energy. Energy before={energy}");
        // 若buff.nextAttackCostModify或buff.movementCostModify有影響，應在 PlayCard 時處理
        energy -= cost;
        if (energy < 0) energy = 0;
    }

    /// <summary>
    /// 依照buff計算最終攻擊傷害，並消耗一次性加成
    /// </summary>
    public int CalculateAttackDamage(int baseDamage)
    {
        int dmg = baseDamage + buffs.nextAttackPlus + buffs.nextTurnAllAttackPlus;
        if (dmg < 0) dmg = 0;
        buffs.nextAttackPlus = 0;
        return dmg;
    }

    /// <summary>
    /// 增加格擋
    /// </summary>
    public void AddBlock(int amount)
    {
        block += amount;
        // 觸發某些遺物檢查(如紫電角)
        foreach (CardBase r in relics)
        {
            if (r is Relic_ZiDianJiao z)
            {
                z.OnAddBlock(this, amount);
            }
        }
    }

    /// <summary>
    /// 受傷(考慮buff: damageTakenRatio)
    /// </summary>
    public void TakeDamage(int dmg)
    {
        int reduced = dmg - buffs.meleeDamageReduce;
        if (reduced < 0) reduced = 0;
        float realDmgF = reduced * buffs.damageTakenRatio;
        int realDmg = Mathf.CeilToInt(realDmgF);

        int remain = realDmg - block;
        if (remain > 0)
        {
            block = 0;
            currentHP -= remain;
            if (currentHP <= 0)
            {
                currentHP = 0;
                // Player Die
            }
        }
        else
        {
            block -= realDmg;
        }
    }

    /// <summary>
    /// 直接扣血(無視block) - 給自殘或特殊設計
    /// </summary>
    public void TakeDamageDirect(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) currentHP = 0;
    }

    /// <summary>
    /// 抽 n 張牌
    /// </summary>
    public void DrawCards(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (deck.Count == 0)
            {
                // 牌庫空 => 嘗試洗牌
                ReshuffleDiscardIntoDeck();
                // 若還是沒卡 => break
                if (deck.Count == 0) break;
            }
            CardBase top = deck[0];
            deck.RemoveAt(0);
            hand.Add(top);
        }
        FindObjectOfType<BattleManager>()?.RefreshHandUI();
    }

    public void DrawNewHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                if (discardPile.Count > 0)
                {
                    deck.AddRange(discardPile);
                    discardPile.Clear();
                    ShuffleDeck();
                }
                else
                {
                    break;
                }
            }

            if (deck.Count > 0)
            {
                CardBase drawn = deck[0];
                deck.RemoveAt(0);
                hand.Add(drawn);
            }
        }
    }


    public void ReshuffleDiscardIntoDeck()
    {
        // 合併
        deck.AddRange(discardPile);
        discardPile.Clear();
        // 洗牌
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        System.Random rnd = new System.Random();
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int r = rnd.Next(0, i + 1);
            CardBase temp = deck[i];
            deck[i] = deck[r];
            deck[r] = temp;
        }
    }

    /// <summary>
    /// 棄 n 張牌 (簡化處理: 從手牌最後幾張丟棄)
    /// </summary>
    public void DiscardCards(int n)
    {
        if (hand.Count < n) n = hand.Count;
        for (int i = 0; i < n; i++)
        {
            CardBase c = hand[hand.Count - 1];
            hand.RemoveAt(hand.Count - 1);
            discardPile.Add(c);
        }
        hasDiscardedThisTurn = true;
        discardCountThisTurn += n;

        // 輪迴竹簡觸發
        foreach (CardBase r in relics)
        {
            if (r is Relic_LunHuiZhuJian zhujian)
            {
                zhujian.OnPlayerDiscard(this, n);
            }
        }
    }

    /// <summary>
    /// 棄1張牌 (給燃盡斬等使用)
    /// </summary>
    public bool DiscardOneCard()
    {
        if (hand.Count == 0) return false;
        CardBase c = hand[hand.Count - 1];
        hand.RemoveAt(hand.Count - 1);
        discardPile.Add(c);

        hasDiscardedThisTurn = true;
        discardCountThisTurn++;

        // 觸發輪迴竹簡
        foreach (CardBase r in relics)
        {
            if (r is Relic_LunHuiZhuJian zhujian)
            {
                zhujian.OnPlayerDiscard(this, 1);
            }
        }

        return true;
    }

    /// <summary>
    /// 檢查是否計畫棄牌(給強化護盾等用)
    /// 此示範直接用 hasDiscardedThisTurn 當條件
    /// </summary>
    public bool CheckDiscardPlan()
    {
        return hasDiscardedThisTurn;
    }

    /// <summary>
    /// 移動至指定格子(若做2D網格)
    /// </summary>
    public void MoveToPosition(Vector2Int targetPos)
    {
        // 簡易示範
        position = targetPos;
        // 同時移動 Transform?
        transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
    }

    /// <summary>
    /// 瞬移 (縮地成寸可用)
    /// </summary>
    public void TeleportToPosition(Vector2Int targetPos)
    {
        position = targetPos;
        transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
    }
}

/// <summary>
/// 玩家身上的Buff與回合狀態
/// </summary>
[System.Serializable]
public class PlayerBuffs
{
    public float damageTakenRatio = 1.0f;   // 受傷倍率(神降可 =0.5)
    public int nextAttackPlus = 0;         // 下次攻擊額外傷害
    public int nextDamageTakenUp = 0;      // 敵人下次受傷+X (也可放在 Enemy 端)
    public int nextAttackCostModify = 0;   // 下次攻擊卡費用增減
    public int movementCostModify = 0;     // 本回合所有移動牌費用增減
    public int nextTurnDrawChange = 0;     // 下回合抽牌增減
    public int needRandomDiscardAtEnd = 0; // 回合結束隨機棄牌
    public int meleeDamageReduce = 0;      // 近戰傷害固定減少
    public int weak = 0;                   // 虛弱回合數
    public int stun = 0;                   // 暈眩回合(無法行動)
    public int nextTurnAllAttackPlus = 0;  // 下回合所有攻擊+X

    /// <summary>
    /// 回合開始重置或計算
    /// </summary>
    public void OnTurnStartReset()
    {
        // 例如上一回合給的 nextTurnAllAttackPlus 可在這回合生效
        // damageTakenRatio回歸1.0f? 視情況
        // 這裡僅示範
        if (stun > 0) stun--;

        // 虛弱也遞減
        if (weak > 0) weak--;

        // nextAttackPlus 只針對"下一次"攻擊, 用後可歸0
        // 如果你要回合開始就歸0, 也可

        // movementCostModify 可歸0
        movementCostModify = 0;
        // nextAttackCostModify 歸0
        nextAttackCostModify = 0;
    }

    /// <summary>
    /// 回合結束處理
    /// </summary>
    public void OnTurnEndReset()
    {
        // damageTakenRatio 若只在本回合生效，回合結束要重置
        damageTakenRatio = 1.0f;
        // nextAttackPlus 也可清零 (若只生效一次)
        // needRandomDiscardAtEnd 在外部處理後歸0
    }
}

