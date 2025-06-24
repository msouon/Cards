using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理所有 BoardTile，並提供查詢功能
/// </summary>
public class Board : MonoBehaviour
{
    // 把所有 Tile 用 (x,y) 做索引
    private Dictionary<Vector2Int, BoardTile> tileDict = new Dictionary<Vector2Int, BoardTile>();

    private void Awake()
    {
        // 自動收集子物件中的 BoardTile
        BoardTile[] tiles = GetComponentsInChildren<BoardTile>();
        foreach (var t in tiles)
        {
            tileDict[t.gridPosition] = t;
        }
    }

    /// <summary>
    /// 依座標取回 BoardTile；若無回傳 null
    /// </summary>
    public BoardTile GetTileAt(Vector2Int pos)
    {
        tileDict.TryGetValue(pos, out BoardTile tile);
        return tile;
    }

    /// <summary>
    /// 把所有 Tile 的高亮與可點擊關閉
    /// </summary>
    public void ResetAllTilesSelectable()
    {
        foreach (var kv in tileDict)
        {
            kv.Value.SetSelectable(false);
        }
    }

    // 取得鄰近四格
    public List<BoardTile> GetAdjacentTiles(Vector2Int pos)
    {
        List<BoardTile> result = new List<BoardTile>();
        Vector2Int[] offs = { new Vector2Int(4, 0), new Vector2Int(-4, 0), new Vector2Int(-2, -4), new Vector2Int(2, -4), new Vector2Int(-2, 4), new Vector2Int(2, 4) };
        foreach (var o in offs)
        {
            BoardTile t = GetTileAt(pos + o);
            if (t != null) result.Add(t);
        }
        return result;
    }
}
