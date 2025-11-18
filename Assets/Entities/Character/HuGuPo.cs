using UnityEngine;

public class HuGuPo : Enemy
{
    [Header("Hu Gu Po Settings")]
    [SerializeField] private int bleedDuration = 2;
    [SerializeField] private int imprisonDuration = 2;
    [SerializeField] private int imprisonCooldownTurns = 3;
    [SerializeField] private int chargeCooldownTurns = 4;
    [SerializeField] private int chargeDamage = 15;

    private int imprisonCooldownCounter = 0;
    private int chargeCooldownCounter = 0;

    private bool isPreparingCharge = false;
    private Vector2Int? lockedChargePosition = null;
    private BoardTile lockedChargeTile = null;

    protected override void Awake()
    {
        enemyName = "虎姑婆";
        maxHP = 60;
        BaseAttackDamage = 10;
        base.Awake();
    }

    public override void EnemyAction(Player player)
    {
        if (HandleFrozenOrStunned())
        {
            return;
        }

        bool skipImprisonIncrement = false;
        bool skipChargeIncrement = false;

        if (ProcessChargeMovement(player))
        {
            skipChargeIncrement = true;
            IncrementCooldowns(skipImprisonIncrement, skipChargeIncrement);
            return;
        }

        if (TryStartCharge(player))
        {
            skipChargeIncrement = true;
            IncrementCooldowns(skipImprisonIncrement, skipChargeIncrement);
            return;
        }

        if (TryUseImprison(player))
        {
            skipImprisonIncrement = true;
            IncrementCooldowns(skipImprisonIncrement, skipChargeIncrement);
            return;
        }

        PerformAttackOrMoveWithBleed(player);
        IncrementCooldowns(skipImprisonIncrement, skipChargeIncrement);
    }

    private bool HandleFrozenOrStunned()
    {
        if (frozenTurns > 0)
        {
            frozenTurns--;
            IncrementCooldowns(false, false);
            return true;
        }

        if (buffs.stun > 0)
        {
            buffs.stun--;
            IncrementCooldowns(false, false);
            return true;
        }

        return false;
    }

    private void PerformAttackOrMoveWithBleed(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (IsPlayerInRange(player))
        {
            int damage = CalculateAttackDamage();
            if (damage > 0)
            {
                player.TakeDamage(damage);
                player.buffs.ApplyBleedFromEnemy(bleedDuration);
            }
            return;
        }

        MoveOneStepTowards(player);
    }

    private bool TryUseImprison(Player player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.buffs.imprison > 0)
        {
            return false;
        }

        if (imprisonCooldownCounter < imprisonCooldownTurns)
        {
            return false;
        }

        player.buffs.ApplyImprisonFromEnemy(imprisonDuration);
        imprisonCooldownCounter = 0;
        return true;
    }

    private bool TryStartCharge(Player player)
    {
        if (isPreparingCharge)
        {
            return false;
        }

        if (player == null || player.buffs.imprison <= 0)
        {
            return false;
        }

        if (chargeCooldownCounter < chargeCooldownTurns)
        {
            return false;
        }

        lockedChargePosition = player.position;
        Board board = FindObjectOfType<Board>();
        lockedChargeTile = board != null && lockedChargePosition.HasValue
            ? board.GetTileAt(lockedChargePosition.Value)
            : null;
        lockedChargeTile?.SetAttackHighlight(true);

        isPreparingCharge = true;
        chargeCooldownCounter = 0;
        return true;
    }

    private bool ProcessChargeMovement(Player player)
    {
        if (!isPreparingCharge)
        {
            return false;
        }

        Vector2Int targetPos = lockedChargePosition ?? gridPosition;
        Vector2Int startPos = gridPosition;
        Board board = FindObjectOfType<Board>();

        bool playerAtLockedSpot = player != null && player.position == targetPos;
        if (playerAtLockedSpot)
        {
            TryKnockbackPlayer(player, board, startPos, targetPos);
            player.TakeDamage(chargeDamage);
        }

        MoveTowardsLockedPosition(board, targetPos);

        isPreparingCharge = false;
        lockedChargePosition = null;
        if (lockedChargeTile != null)
        {
            lockedChargeTile.SetAttackHighlight(false);
            lockedChargeTile = null;
        }

        chargeCooldownCounter = 0;
        return true;
    }

    private void MoveTowardsLockedPosition(Board board, Vector2Int targetPos)
    {
        if (board == null)
        {
            return;
        }

        BoardTile tile = board.GetTileAt(targetPos);
        if (tile == null)
        {
            return;
        }

        bool occupiedByEnemy = board.IsTileOccupied(targetPos) && gridPosition != targetPos;
        if (occupiedByEnemy)
        {
            return;
        }

        gridPosition = targetPos;
        transform.position = tile.transform.position;
        UpdateSpriteSortingOrder();
    }

    private void TryKnockbackPlayer(Player player, Board board, Vector2Int startPos, Vector2Int targetPos)
    {
        if (player == null || board == null)
        {
            return;
        }

        Vector2Int direction = targetPos - startPos;
        if (direction == Vector2Int.zero)
        {
            return;
        }

        Vector2Int knockbackPos = targetPos + direction;
        BoardTile knockbackTile = board.GetTileAt(knockbackPos);
        if (knockbackTile == null)
        {
            return;
        }

        if (board.IsTileOccupied(knockbackPos))
        {
            return;
        }

        player.position = knockbackPos;
        player.transform.position = knockbackTile.transform.position;
        knockbackTile.HandlePlayerEntered(player);
    }

    private void IncrementCooldowns(bool skipImprison, bool skipCharge)
    {
        if (!skipImprison)
        {
            imprisonCooldownCounter = Mathf.Min(imprisonCooldownCounter + 1, imprisonCooldownTurns);
        }

        if (!skipCharge)
        {
            chargeCooldownCounter = Mathf.Min(chargeCooldownCounter + 1, chargeCooldownTurns);
        }
    }
}