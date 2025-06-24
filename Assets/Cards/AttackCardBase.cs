using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻擊卡基底，加入可自訂範圍設定
/// </summary>
public abstract class AttackCardBase : CardBase
{
    [Header("攻擊範圍偏移表")]
    public List<Vector2Int> rangeOffsets = new List<Vector2Int>();
}
