using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#region 4 張移動牌

/// <summary>
/// 翻滾 (1費, 移動1格, 並獲得閃避狀態 - 減少部分近戰傷害)
/// </summary>
[CreateAssetMenu(fileName = "Move_FanGun", menuName = "Cards/Movement/翻滾")]
public class Move_FanGun : MovementCardBase
{
    public int dodgeValue = 3; // 近戰減傷

    public override void ExecuteEffect(Player player, Enemy enemy)
{
    // 留空或寫你想做的事
}
    private void OnEnable()
    {
        cardType = CardType.Movement;
    }

    public override void ExecuteOnPosition(Player player, Vector2Int targetGridPos)
    {
        // 移動1格
        player.MoveToPosition(targetGridPos);

        // 獲得閃避buff
        player.buffs.meleeDamageReduce += dodgeValue;
    }
}

/// <summary>
/// 衝刺 (1費, 直線突進2格, 撞到敵人造成傷害並使其後退)
/// </summary>
[CreateAssetMenu(fileName = "Move_ChongCi", menuName = "Cards/Movement/衝刺")]
public class Move_ChongCi : MovementCardBase
{
    public int dashDistance = 2;
    public int collisionDamage = 5;

    public override void ExecuteEffect(Player player, Enemy enemy)
    {
        // 留空或寫你想做的事
    }
    private void OnEnable()
    {
        cardType = CardType.Movement;
    }

    public override void ExecuteOnPosition(Player player, Vector2Int targetGridPos)
    {
        // 直線衝刺
        // 1) 檢查路徑是否有敵人
        Enemy e = FindEnemyOnTheWay(player.position, targetGridPos);
        if (e != null)
        {
            // 對敵人造成傷害
            e.TakeDamage(collisionDamage);
            // 使其後退1格 => e.PushBack(1);
        }

        // 然後移動玩家
        player.MoveToPosition(targetGridPos);
    }

    private Enemy FindEnemyOnTheWay(Vector2Int startPos, Vector2Int endPos)
    {
        // 你可在此做「格子檢查」邏輯
        // 這裡示範直接回傳場上唯一 enemy
        return GameObject.FindObjectOfType<Enemy>();
    }
}

#endregion

