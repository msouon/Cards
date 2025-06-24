using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 縮地成寸 (1費, 越過障礙/敵人移動2格, 但付出小量自傷)
/// </summary>
[CreateAssetMenu(fileName = "Move_SuoDiChengCun", menuName = "Cards/Movement/縮地成寸")]
public class Move_SuoDiChengCun : MovementCardBase
{
    public int moveDistance = 2;
    public int selfDamage = 2;

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
        // 移動2格, 不管中間有無障礙 (需你在 MapSystem 中實作)
        player.TeleportToPosition(targetGridPos);
        // 自損
        player.TakeDamageDirect(selfDamage); // 直接扣血, 不計block
    }

}
