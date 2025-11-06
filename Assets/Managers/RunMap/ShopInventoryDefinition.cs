using System.Collections.Generic;                      // 要用 List<>
using UnityEngine;                                     // Unity 命名空間

// 建立商店庫存用的 ScriptableObject，可以在 Unity 建資產
[CreateAssetMenu(menuName = "Run/Shop Inventory", fileName = "ShopInventory")]
public class ShopInventoryDefinition : ScriptableObject   // 這個資產描述一間商店可以賣什麼
{
    // 可以被買的卡片清單
    [SerializeField] private List<CardBase> purchasableCards = new List<CardBase>();
    // 可以被買的遺物清單（這裡也用 CardBase 存，之後可換成真正的 Relic 類型）
    [SerializeField] private List<CardBase> purchasableRelics = new List<CardBase>();
    // 玩家要移除卡片時需要花的錢
    [SerializeField] private int cardRemovalCost = 75;

    // 對外的唯讀屬性：商店有哪些卡可以買
    public IReadOnlyList<CardBase> PurchasableCards => purchasableCards;
    // 對外的唯讀屬性：商店有哪些「遺物」可以買
    public IReadOnlyList<CardBase> PurchasableRelics => purchasableRelics;
    // 對外的費用，保證至少是 0
    public int CardRemovalCost => Mathf.Max(0, cardRemovalCost);
}
