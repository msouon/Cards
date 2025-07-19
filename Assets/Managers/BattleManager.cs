using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    public GameObject cardPrefab;    // �qInspector��J

    private enum TurnState { PlayerTurn, EnemyTurn, Victory, Defeat }
    private TurnState currentState;
    public Transform handPanel;      // Inspector�̫��w HandPanel
    public Transform deckPile;       // ��ܩ�P��
    public Transform discardPile;    // ��ܱ�P��
    public Board board; // �bInspector ���V�A��Board����
    // 1) �����O�_���b��ܲ���Tile
    private bool isSelectingMovementTile = false;
    // 2) �Ȧs���a���b�ϥΪ����ʥd
    private CardBase currentMovementCard = null;
    private bool isSelectingAttackTarget = false;
    private CardBase currentAttackCard = null;
    private List<Enemy> highlightedEnemies = new List<Enemy>();
    
    void Start()
    {
        StartPlayerTurn(); // �T�O��P�޿趰���޲z
        // ���]���W�@�}�l�N�� player, enemy
        currentState = TurnState.PlayerTurn;
        if (enemy != null) enemy.ProcessTurnStart();
        
    }

    void Update()
    {
        if (currentState == TurnState.PlayerTurn)
        {
            // �Y���a���U�����^�X => EndPlayerTurn();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
            }
        }

        // �ӱѧP�_
        if (enemy != null && enemy.currentHP <= 0 && currentState != TurnState.Victory)
        {
            currentState = TurnState.Victory;
            Debug.Log("�ӧQ�I");
        }
        if (player.currentHP <= 0 && currentState != TurnState.Defeat)
        {
            currentState = TurnState.Defeat;
            Debug.Log("����...");
        }
    }

    public void EndPlayerTurn()
    {
        // 1) ���Ҧ���P
        DiscardAllHand();

        if (currentState == TurnState.PlayerTurn)
        {
            // �������a�^�X
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
        // ��sUI
        RefreshHandUI();
    }


    private void DiscardAllHand()
    {
        // �������P�����P��
        player.discardPile.AddRange(player.hand);
        player.hand.Clear();
        // UI�@�֧�s(��P�O�Ū�)
        RefreshHandUI();
    }

    IEnumerator EnemyTurn()
    {
        currentState = TurnState.EnemyTurn;
        if (enemy != null) enemy.ProcessTurnStart();
        // �����ĤH���1��
        yield return new WaitForSeconds(1f);

        if (enemy != null) enemy.EnemyAction(player);

        yield return new WaitForSeconds(1f);
        // �^�X����, reset block or do nothing (Slay the Spire -> block�k0)
        // �o�̥ܽd:
        player.block = 0;
        if (enemy != null) enemy.block = 0;

        // ���^���a�^�X
        currentState = TurnState.PlayerTurn;
        StartPlayerTurn();
        Debug.Log("���a�^�X�}�l");
    }

    /// <summary>
    /// UI/���s�I��: ���X�d�P
    /// </summary>
    public void PlayCard(CardBase cardData)
    {
        if (currentState != TurnState.PlayerTurn) return;

        // �ˬd��q & ����
        int finalCost = cardData.cost;

        // �Y�O�����d, �ˬd player.buffs.nextAttackCostModify
        if (cardData.cardType == CardType.Attack && player.buffs.nextAttackCostModify != 0)
        {
            finalCost += player.buffs.nextAttackCostModify;
            if (finalCost < 0) finalCost = 0;
        }

        // �Y�O���ʵP, �ˬd player.buffs.movementCostModify
        if (cardData.cardType == CardType.Movement && player.buffs.movementCostModify != 0)
        {
            finalCost += player.buffs.movementCostModify;
            if (finalCost < 0) finalCost = 0;
        }

        if (player.energy < finalCost)
        {
            Debug.Log("��q����");
            return;
        }

        // ����ĪG
        cardData.ExecuteEffect(player, enemy);

        // �Y�O�����d => �֭p���^�X��������
        if (cardData.cardType == CardType.Attack)
        {
            player.attackUsedThisTurn++;
            // �U������+X => �Τ@����M0
            if (player.buffs.nextAttackPlus > 0)
            {
                // �l�[�ˮ` => �ݤ�ʦA�I�s?? 
                // �Ψƥ��b ExecuteEffect �ɥ[
                // �o�̥ܽd��lastDamage+ nextAttackPlus�A��enemy 
                // �|������, �i�̹�ڻݨD
                player.buffs.nextAttackPlus = 0;
            }
        }

        // �d�i��P��
        if (player.hand.Contains(cardData))
        {
            player.hand.Remove(cardData);
            player.discardPile.Add(cardData);
        }

        // 4) ����q / ��sUI
        player.UseEnergy(finalCost);
        RefreshHandUI();
    }

    public void UseMovementCard(CardBase movementCard)
    {
        // 1) �ˬd��q
        if (player.energy < movementCard.cost)
        {
            Debug.Log("��q�������ಾ��");
            return;
        }
        if (isSelectingMovementTile)
        {
            Debug.Log("�w�b��ܲ���Tile���A, �Х�����");
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

    // ���]player.position�O( x , y ), �ڭ̷Q�аO�|�P( x��1 , y ), ( x , y��1 )
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
    public void CancelMovementSelection()
    {
        isSelectingMovementTile=false;
        currentMovementCard=null;
        board.ResetAllTilesSelectable();
    }

    public void OnTileClicked(BoardTile tile)
    {
        // �p�G�ثe���O�b�ﲾ��tile���A => ����
        if (!isSelectingMovementTile) return;

        // 1) �Ѳ��ʵP�M�w���
        currentMovementCard.ExecuteOnPosition(player, tile.gridPosition);

        // ����q
        int finalCost = currentMovementCard.cost + player.buffs.movementCostModify;
        if (finalCost < 0) finalCost = 0;
        player.UseEnergy(finalCost);

        // 3) �q��P���� currentMovementCard => ����P
        if (player.hand.Contains(currentMovementCard))
        {
            player.hand.Remove(currentMovementCard);
            player.discardPile.Add(currentMovementCard);
        }

        // 4) ���m���A
        isSelectingMovementTile = false;
        currentMovementCard = null;

        // 5) �����Ҧ��i��Tile
        board.ResetAllTilesSelectable();

        // 6) ��s��PUI
        RefreshHandUI();
    }


    // �����a��P�B���P�B�^�X�}�l�����A���ܫ�A�i�I�s����k��s��PUI
    public void RefreshHandUI()
    {
        if (deckPile)
        {
            Text t = deckPile.GetComponentInChildren<Text>();
            if (t) t.text = "牌庫區: " + player.deck.Count;
        }
        if (discardPile)
        {
            Text t2 = discardPile.GetComponentInChildren<Text>();
            if (t2) t2.text = "棄牌區: " + player.discardPile.Count;
        }
        // 1. ���M���{�����l����(��PUI)
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. ���s�ͦ�
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
        if (player.energy < finalCost) { Debug.Log("��q����"); return; }

        // �]���A
        isSelectingAttackTarget = true;
        currentAttackCard = attackCard;

        // ���G�d�򤺪��ĤH
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

    // ====== �I�������G�� EnemyClickable.cs �� OnMouseDown Ĳ�o ======
    public void OnEnemyClicked(Enemy e)
    {
        if (!isSelectingAttackTarget) return;
        if (!highlightedEnemies.Contains(e)) return;    // �u���\���G�d�򤺪��ĤH

        // �������
        currentAttackCard.ExecuteEffect(player, e);

        // ����q�B��P
        player.hand.Remove(currentAttackCard);
        player.discardPile.Add(currentAttackCard);
        int finalCost = currentAttackCard.cost + player.buffs.nextAttackCostModify;
        if (finalCost < 0) finalCost = 0;
        player.UseEnergy(finalCost);

        // ����
        EndAttackSelect();
        RefreshHandUI();
    }

    // ====== ���� / ������� ======
    public void EndAttackSelect()
    {
        isSelectingAttackTarget = false;
        currentAttackCard = null;
        foreach (var en in highlightedEnemies) en.SetHighlight(false);
        highlightedEnemies.Clear();
    }

    // ====== ���G�j�M ======
    private void HighlightEnemiesWithOffsets(Vector2Int center, List<Vector2Int> offsets)
    {
        highlightedEnemies.Clear(); // �M���w���G���ĤH

        Enemy[] all = FindObjectsOfType<Enemy>(); // ��X�Ҧ��ĤH

        foreach (var off in offsets) // ��C�Ӱ����i���ˬd
        {
            Vector2Int targetPos = center + off; // �p��ؼЮ�l�y��

            foreach (var e in all) // �ˬd�Ҧ��ĤH
            {
                if (e.gridPosition == targetPos) // �p�G�ĤH����m�ŦX�ؼЮ�
                {
                    if (!highlightedEnemies.Contains(e)) // �p�G�٨S���G
                    {
                        e.SetHighlight(true); // �]�w�����G���A
                        highlightedEnemies.Add(e); // �[�J���G�M��
                    }
                }
            }
        }
    }

    // �A�i�H�b Player.DrawCards / DiscardCards ��, or StartTurn() ��, 
    // �H�� PlayCard(...) ��, ���I�s RefreshHandUI() ��s�e���C
}
