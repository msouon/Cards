using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有卡牌的抽象基底類別，繼承 ScriptableObject 便於在 Unity 中建檔
/// </summary>
public abstract class CardBase : ScriptableObject
{
    [Header("卡牌基本屬性")]
    public string cardName;         // 卡牌名稱
    public int cost;                // 能量消耗
    [TextArea] public string description;   // 敘述文字
    public Sprite cardImage;        // 卡面圖示 (可選)


    [Header("卡牌類型")]
    public CardType cardType;

    /// <summary>
    /// 執行卡牌效果 (由子類別實作)
    /// </summary>
    /// <param name="player">玩家</param>
    /// <param name="enemy">目標敵人(單體)</param>
    public abstract void ExecuteEffect(Player player, Enemy enemy);

    /// <summary>
    /// (可選) 給移動卡或範圍攻擊卡使用的擴充
    /// 例如有的卡需要指定格子或範圍
    /// </summary>
    /// 

    public virtual void ExecuteOnPosition(Player player, Vector2Int targetGridPos)
    {
        // 預設不做事，移動卡可以覆寫
    }
}
