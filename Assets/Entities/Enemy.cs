using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName = "Slime";
    public int maxHP = 30;
    public int currentHP;
    public int block = 0;

    // 可能的狀態
    public bool hasBerserk = false; // 例如"爆走"檢查

    // buff 結構(若需要)
    public EnemyBuffs buffs = new EnemyBuffs();
    public Vector2Int gridPosition;

    public bool isBoss = false; // boss 判定

    // 元素標籤
    private HashSet<ElementType> elementTags = new HashSet<ElementType>();

    // 狀態效果
    public int burningTurns = 0;
    public int frozenTurns = 0;
    public bool thunderstrike = false;
    public bool superconduct = false;
    private void Awake()
    {
        currentHP = maxHP;
    }

    [SerializeField] private GameObject highlightFx; // 拖一個外框

    public void SetHighlight(bool on)
    {
        if (highlightFx) highlightFx.SetActive(on);
    }

    private void OnMouseDown()
    {
        BattleManager bm = FindObjectOfType<BattleManager>();
        bm.OnEnemyClicked(this);
    }


    public void TakeDamage(int dmg)
    {
        int remain = dmg - block;
        if (remain > 0)
        {
            block = 0;
            currentHP -= remain;
            if (currentHP <= 0)
            {
                currentHP = 0;
                Die();
            }
        }
        else
        {
            block -= dmg;
        }
    }

    /// <summary>
    /// 真實傷害(無視block)
    /// </summary>
    public void TakeTrueDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void AddBlock(int amount)
    {
        block += amount;
    }

    public void ReduceBlock(int amount)
    {
        block -= amount;
        if (block < 0) block = 0;
    }

    public void DispelBuff(int count)
    {
        // 視實際設計，你可移除某些Buff
        // 這裡僅示範清除count層
        buffs.ClearSomeBuff(count);
    }

    // ===== 元素標籤處理 =====
    public bool HasElement(ElementType e)
    {
        return elementTags.Contains(e);
    }

    public void AddElementTag(ElementType e)
    {
        elementTags.Add(e);
    }

    public void RemoveElementTag(ElementType e)
    {
        elementTags.Remove(e);
    }

    /// <summary>
    /// 處理帶有元素的攻擊，回傳實際傷害
    /// </summary>
    public int ApplyElementalAttack(ElementType e, int baseDamage, Player player)
    {
        int dmg = baseDamage;

        // 汽化
        if (e == ElementType.Fire && HasElement(ElementType.Water) ||
            e == ElementType.Water && HasElement(ElementType.Fire))
        {
            dmg = Mathf.CeilToInt(baseDamage * 1.5f);
            if (e == ElementType.Fire)
            {
                RemoveElementTag(ElementType.Water);
            }
            else
            {
                RemoveElementTag(ElementType.Fire);
            }
            elementTags.Add(e);
        }
        // 融化
        else if (e == ElementType.Fire && HasElement(ElementType.Ice) ||
                 e == ElementType.Ice && HasElement(ElementType.Fire))
        {
            dmg = Mathf.CeilToInt(baseDamage * 1.5f);
            if (e == ElementType.Fire)
            {
                RemoveElementTag(ElementType.Ice);
            }
            else
            {
                RemoveElementTag(ElementType.Fire);
            }
            elementTags.Add(e);
        }
        // 燃燒
        else if (e == ElementType.Fire && HasElement(ElementType.Wood) ||
                 e == ElementType.Wood && HasElement(ElementType.Fire))
        {
            burningTurns = 5;
            elementTags.Add(ElementType.Fire);
            elementTags.Add(ElementType.Wood);
        }
        // 凍結
        else if (e == ElementType.Ice && HasElement(ElementType.Water) ||
                 e == ElementType.Water && HasElement(ElementType.Ice))
        {
            bool freeze = true;
            if (isBoss)
            {
                if (Random.value < 0.5f) freeze = false;
            }
            if (freeze) frozenTurns = 1;
            RemoveElementTag(ElementType.Ice);
            RemoveElementTag(ElementType.Water);
        }
        // 超載
        else if (e == ElementType.Fire && HasElement(ElementType.Thunder) ||
                 e == ElementType.Thunder && HasElement(ElementType.Fire))
        {
            // 相鄰敵人受50%傷害並附帶最後元素
            ElementType keep = e;
            ElementType remove = (e == ElementType.Fire) ? ElementType.Thunder : ElementType.Fire;
            Board board = FindObjectOfType<Board>();
            if (board != null)
            {
                foreach (var en in FindObjectsOfType<Enemy>())
                {
                    if (en == this) continue;
                    if (Vector2Int.Distance(en.gridPosition, gridPosition) <= 1.1f)
                    {
                        en.TakeDamage(Mathf.CeilToInt(baseDamage * 0.5f));
                        en.AddElementTag(keep);
                    }
                }
            }
            RemoveElementTag(remove);
            elementTags.Add(keep);
        }
        // 導電
        else if (e == ElementType.Thunder && HasElement(ElementType.Water))
        {
            foreach (var en in FindObjectsOfType<Enemy>())
            {
                if (en == this) continue;
                bool adjacent = Vector2Int.Distance(en.gridPosition, gridPosition) <= 1.1f;
                bool valid = false;
                if (adjacent && en.HasElement(ElementType.Water)) valid = true;
                if (!valid)
                {
                    Board board = FindObjectOfType<Board>();
                    if (board != null)
                    {
                        BoardTile tile = board.GetTileAt(en.gridPosition);
                        if (tile != null && tile.HasElement(ElementType.Water)) valid = true;
                    }
                }
                if (valid)
                {
                    en.TakeDamage(baseDamage);
                }
            }
            elementTags.Add(e);
        }
        // 雷擊
        else if (e == ElementType.Thunder && HasElement(ElementType.Wood) ||
                 e == ElementType.Wood && HasElement(ElementType.Thunder))
        {
            thunderstrike = true;
            RemoveElementTag(ElementType.Wood);
            RemoveElementTag(ElementType.Thunder);
        }
        // 超導
        else if (e == ElementType.Thunder && HasElement(ElementType.Ice) ||
                 e == ElementType.Ice && HasElement(ElementType.Thunder))
        {
            superconduct = true;
            RemoveElementTag(ElementType.Thunder);
            RemoveElementTag(ElementType.Ice);
        }
        else
        {
            elementTags.Add(e);
        }

        // 觸發加成：雷擊
        if (thunderstrike)
        {
            dmg *= 2;
            thunderstrike = false;
        }

        // 超導
        if (superconduct && (e == ElementType.Thunder || e == ElementType.Ice))
        {
            dmg += 6;
            superconduct = false;
        }

        return dmg;
    }

    public void ProcessTurnStart()
    {
        if (burningTurns > 0)
        {
            TakeDamage(2);
            burningTurns--;
            if (burningTurns == 0)
            {
                RemoveElementTag(ElementType.Fire);
                RemoveElementTag(ElementType.Wood);
            }

            Board board = FindObjectOfType<Board>();
            if (board != null)
            {
                BoardTile tile = board.GetTileAt(gridPosition);
                if (tile != null)
                {
                    Debug.Log("Enemy is on tile with growth trap: " + tile.growthTrap);
                    tile.TriggerGrowthTrap(this);
                }
                else
                {
                    Debug.LogWarning("No tile found at enemy position: " + gridPosition);
                }
            }
            else
            {
                Debug.LogWarning("No Board found in scene.");
            }
        }
    }
    public void EnemyAction(Player player)
    {
        // 若凍結或暈眩無法行動
        if (frozenTurns > 0)
        {
            frozenTurns--;
            return;
        }
        // 簡易：每回合攻擊10點
        // 若 stun>0 則無法行動
        if (buffs.stun > 0)
        {
            buffs.stun--;
            return;
        }

        int atkValue = 10;
        // 若敵人有爆走, 可能 + 5
        if (hasBerserk)
        {
            atkValue += 5;
        }

        player.TakeDamage(atkValue);
    }

    void Die()
    {
        Debug.Log(enemyName + " died!");
        // 播死亡動畫、掉落等
        Destroy(gameObject);
    }
}

[System.Serializable]
public class EnemyBuffs
{
    public int stun = 0;

    public void ClearSomeBuff(int count)
    {
        // 自行定義：若敵人有 3 種Buff，各-1層?
        // 這裡僅示範 stun--
        if (stun > 0)
        {
            int remain = stun - count;
            if (remain < 0) remain = 0;
            stun = remain;
        }
    }
}


