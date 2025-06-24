using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    public GameObject cardPrefab;    // 從Inspector拖入

    private enum TurnState { PlayerTurn, EnemyTurn, Victory, Defeat }
    private TurnState currentState;
    public Transform handPanel;      // Inspector裡指定 HandPanel
    public Transform deckPile;       // 顯示抽牌堆
    public Transform discardPile;    // 顯示棄牌堆
    public Board board; // 在Inspector 指向你的Board物件
    // 1) 紀錄是否正在選擇移動Tile
    private bool isSelectingMovementTile = false;
    // 2) 暫存玩家正在使用的移動卡
    private CardBase currentMovementCard = null;
    private bool isSelectingAttackTarget = false;
    private CardBase currentAttackCard = null;
    private List<Enemy> highlightedEnemies = new List<Enemy>();
    
    void Start()
    {
        StartPlayerTurn(); // 確保抽牌邏輯集中管理
        // 假設場上一開始就有 player, enemy
        currentState = TurnState.PlayerTurn;
        if (enemy != null) enemy.ProcessTurnStart();
        
    }

    void Update()
    {
        if (currentState == TurnState.PlayerTurn)
        {
            // 若玩家按下結束回合 => EndPlayerTurn();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
            }
        }

        // 勝敗判斷
        if (enemy != null && enemy.currentHP <= 0 && currentState != TurnState.Victory)
        {
            currentState = TurnState.Victory;
            Debug.Log("勝利！");
        }
        if (player.currentHP <= 0 && currentState != TurnState.Defeat)
        {
            currentState = TurnState.Defeat;
            Debug.Log("失敗...");
        }
    }

    public void EndPlayerTurn()
    {
        // 1) 丟棄所有手牌
        DiscardAllHand();

        if (currentState == TurnState.PlayerTurn)
        {
            // 結束玩家回合
            player.EndTurn();
            StartCoroutine(EnemyTurn());
        }

    }

    public void StartPlayerTurn()
    {
        if (enemy != null) enemy.ProcessTurnStart();
        int drawCount = 5 + player.buffs.nextTurnDrawChange;
        if (drawCount < 0) drawCount = 0;
        player.buffs.nextTurnDrawChange = 0;
        player.DrawNewHand(drawCount);
        // 更新UI
        RefreshHandUI();
    }


    private void DiscardAllHand()
    {
        // 把全部手牌移到棄牌堆
        player.discardPile.AddRange(player.hand);
        player.hand.Clear();
        // UI一併更新(手牌是空的)
        RefreshHandUI();
    }

    IEnumerator EnemyTurn()
    {
        currentState = TurnState.EnemyTurn;
        if (enemy != null) enemy.ProcessTurnStart();
        // 模擬敵人思考1秒
        yield return new WaitForSeconds(1f);

        if (enemy != null) enemy.EnemyAction(player);

        yield return new WaitForSeconds(1f);
        // 回合結束, reset block or do nothing (Slay the Spire -> block歸0)
        // 這裡示範:
        player.block = 0;
        if (enemy != null) enemy.block = 0;

        // 切回玩家回合
        currentState = TurnState.PlayerTurn;
        StartPlayerTurn();
        Debug.Log("玩家回合開始");
    }

    /// <summary>
    /// UI/按鈕點擊: 打出卡牌
    /// </summary>
    public void PlayCard(CardBase cardData)
    {
        if (currentState != TurnState.PlayerTurn) return;

        // 檢查能量 & 類型
        int finalCost = cardData.cost;

        // 若是攻擊卡, 檢查 player.buffs.nextAttackCostModify
        if (cardData.cardType == CardType.Attack && player.buffs.nextAttackCostModify != 0)
        {
            finalCost += player.buffs.nextAttackCostModify;
            if (finalCost < 0) finalCost = 0;
        }

        // 若是移動牌, 檢查 player.buffs.movementCostModify
        if (cardData.cardType == CardType.Movement && player.buffs.movementCostModify != 0)
        {
            finalCost += player.buffs.movementCostModify;
            if (finalCost < 0) finalCost = 0;
        }

        if (player.energy < finalCost)
        {
            Debug.Log("能量不足");
            return;
        }

        // 執行效果
        cardData.ExecuteEffect(player, enemy);

        // 若是攻擊卡 => 累計本回合攻擊次數
        if (cardData.cardType == CardType.Attack)
        {
            player.attackUsedThisTurn++;
            // 下次攻擊+X => 用一次後清0
            if (player.buffs.nextAttackPlus > 0)
            {
                // 追加傷害 => 需手動再呼叫?? 
                // 或事先在 ExecuteEffect 時加
                // 這裡示範把lastDamage+ nextAttackPlus再對enemy 
                // 會較複雜, 可依實際需求
                player.buffs.nextAttackPlus = 0;
            }
        }

        // 卡進棄牌堆
        if (player.hand.Contains(cardData))
        {
            player.hand.Remove(cardData);
            player.discardPile.Add(cardData);
        }

        // 4) 扣能量 / 更新UI
        player.UseEnergy(finalCost);
        RefreshHandUI();
    }

    public void UseMovementCard(CardBase movementCard)
    {
        // 1) 檢查能量
        if (player.energy < movementCard.cost)
        {
            Debug.Log("能量不足不能移動");
            return;
        }
        if (isSelectingMovementTile)
        {
            Debug.Log("已在選擇移動Tile狀態, 請先完成");
            return;
        }

        isSelectingMovementTile = true;
        currentMovementCard = movementCard;

        MovementCardBase mCard = movementCard as MovementCardBase;
        List<Vector2Int> offs = null;
        if (mCard != null && mCard.rangeOffsets != null && mCard.rangeOffsets.Count > 0)
        {
            offs = mCard.rangeOffsets;
        }
        else
        {
            offs = new List<Vector2Int>
            {
                new Vector2Int(0,1), new Vector2Int(0,-1),
                new Vector2Int(-1,0), new Vector2Int(1,0)
            };
        }

        HighlightTilesWithOffsets(player.position, offs);
    }

    // 假設player.position是( x , y ), 我們想標記四周( x±1 , y ), ( x , y±1 )
    private void HighlightTilesWithOffsets(Vector2Int centerPos, List<Vector2Int> offsets)
    {
        foreach (var off in offsets)
        {
            Vector2Int tilePos = centerPos + off;
            BoardTile tile = board.GetTileAt(tilePos);
            if (tile != null)
            {
                tile.SetSelectable(true);
            }
        }
    }

    public void ResetAllTilesSelectable()
    {
        board.ResetAllTilesSelectable();
    }

    public void OnTileClicked(BoardTile tile)
    {
        // 如果目前不是在選移動tile狀態 => 忽略
        if (!isSelectingMovementTile) return;

        // 1) 由移動牌決定行動
        currentMovementCard.ExecuteOnPosition(player, tile.gridPosition);

        // 扣能量
        int finalCost = currentMovementCard.cost + player.buffs.movementCostModify;
        if (finalCost < 0) finalCost = 0;
        player.UseEnergy(finalCost);

        // 3) 從手牌移除 currentMovementCard => 丟到棄牌
        if (player.hand.Contains(currentMovementCard))
        {
            player.hand.Remove(currentMovementCard);
            player.discardPile.Add(currentMovementCard);
        }

        // 4) 重置狀態
        isSelectingMovementTile = false;
        currentMovementCard = null;

        // 5) 關閉所有可選Tile
        board.ResetAllTilesSelectable();

        // 6) 刷新手牌UI
        RefreshHandUI();
    }


    // 當玩家抽牌、打牌、回合開始等狀態改變後，可呼叫此方法更新手牌UI
    public void RefreshHandUI()
    {
        if (deckPile)
        {
            Text t = deckPile.GetComponentInChildren<Text>();
            if (t) t.text = "牌庫: " + player.deck.Count;
        }
        if (discardPile)
        {
            Text t2 = discardPile.GetComponentInChildren<Text>();
            if (t2) t2.text = "棄牌: " + player.discardPile.Count;
        }
        // 1. 先清除現有的子物件(手牌UI)
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. 重新生成
        foreach (var cardData in player.hand)
        {
            GameObject cardObj = Instantiate(cardPrefab, handPanel);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetupCard(cardData);
        }
    }

    public void StartAttackSelect(CardBase attackCard)
    {
        int finalCost = attackCard.cost + player.buffs.nextAttackCostModify;
        if (finalCost < 0) finalCost = 0;
        if (player.energy < finalCost) { Debug.Log("能量不足"); return; }

        // 設狀態
        isSelectingAttackTarget = true;
        currentAttackCard = attackCard;

        // 高亮範圍內的敵人
        AttackCardBase aCard = attackCard as AttackCardBase;
        List<Vector2Int> offs = null;
        if (aCard != null && aCard.rangeOffsets != null && aCard.rangeOffsets.Count > 0)
        {
            offs = aCard.rangeOffsets;
        }
        else
        {
            offs = new List<Vector2Int>
            {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1),
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
        }

        HighlightEnemiesWithOffsets(player.position, offs);
    }

    // ====== 點擊偵測：由 EnemyClickable.cs 或 OnMouseDown 觸發 ======
    public void OnEnemyClicked(Enemy e)
    {
        if (!isSelectingAttackTarget) return;
        if (!highlightedEnemies.Contains(e)) return;    // 只允許高亮範圍內的敵人

        // 執行攻擊
        currentAttackCard.ExecuteEffect(player, e);

        // 扣能量、丟牌
        player.hand.Remove(currentAttackCard);
        player.discardPile.Add(currentAttackCard);
        int finalCost = currentAttackCard.cost + player.buffs.nextAttackCostModify;
        if (finalCost < 0) finalCost = 0;
        player.UseEnergy(finalCost);

        // 收尾
        EndAttackSelect();
        RefreshHandUI();
    }

    // ====== 取消 / 結束選取 ======
    public void EndAttackSelect()
    {
        isSelectingAttackTarget = false;
        currentAttackCard = null;
        foreach (var en in highlightedEnemies) en.SetHighlight(false);
        highlightedEnemies.Clear();
    }

    // ====== 高亮搜尋 ======
    private void HighlightEnemiesWithOffsets(Vector2Int center, List<Vector2Int> offsets)
    {
        highlightedEnemies.Clear(); // 清除已高亮的敵人

        Enemy[] all = FindObjectsOfType<Enemy>(); // 找出所有敵人

        foreach (var off in offsets) // 對每個偏移進行檢查
        {
            Vector2Int targetPos = center + off; // 計算目標格子座標

            foreach (var e in all) // 檢查所有敵人
            {
                if (e.gridPosition == targetPos) // 如果敵人的位置符合目標格
                {
                    if (!highlightedEnemies.Contains(e)) // 如果還沒高亮
                    {
                        e.SetHighlight(true); // 設定為高亮狀態
                        highlightedEnemies.Add(e); // 加入高亮清單
                    }
                }
            }
        }
    }

    // 你可以在 Player.DrawCards / DiscardCards 後, or StartTurn() 後, 
    // 以及 PlayCard(...) 後, 都呼叫 RefreshHandUI() 更新畫面。
}
