using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 簡單 OnMouseDown；被點擊後轉給 BattleManager
/// </summary>
public class BoardTileSelectable : MonoBehaviour
{
    private void OnMouseDown()
    {
        BattleManager bm = FindObjectOfType<BattleManager>();
        if (bm) bm.OnTileClicked(GetComponent<BoardTile>());
    }
}
