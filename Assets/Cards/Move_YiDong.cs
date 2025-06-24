using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移動 (0費, 移動1格, 升級後可獲得少量護盾)
/// </summary>
[CreateAssetMenu(fileName = "Move_YiDong", menuName = "Cards/Movement/移動")]
public class Move_YiDong : MovementCardBase
{
    public int blockIfUpgraded = 1; // 假設升級後給1護盾

    private void OnEnable()
    {
        cardType = CardType.Movement;
    }

    public override void ExecuteEffect(Player player, Enemy enemy)
    {

    }

    public override void ExecuteOnPosition(Player player, Vector2Int targetGridPos)
    {
        // 實際移動玩家到 targetGridPos(只差1格)
        // 這裡只做示範:
        player.MoveToPosition(targetGridPos);

        // 如果此卡已升級, 給點護盾 => 省略狀態判斷, 你可在Card資料加個bool isUpgraded
        // if(isUpgraded) player.AddBlock(blockIfUpgraded);
    }
}