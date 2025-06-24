using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementCardBase : CardBase
{
    // 若你希望MovementCard也可以放在場上執行位置行動，
    // 可 override ExecuteOnPosition

    [Header("移動範圍偏移表")]
    public List<Vector2Int> rangeOffsets = new List<Vector2Int>();
}
