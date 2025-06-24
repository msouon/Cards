using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // 如需TextMeshPro，若不用則改回UnityEngine.UI.Text

public class CardUI: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI 參考")]
    public Text cardNameText;
    public Text costText;
    public Text descriptionText;
    public Image cardImage;
    public Image cardBackground;

    [Header("內部參數")]
    public CardBase cardData; // 這張卡所對應的 ScriptableObject
    public Transform originalParent;
    private Canvas canvas;     // 為了正確拖曳(協助計算鼠標位置)
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindObjectOfType<Canvas>();
    }

    /// <summary>
    /// 設定本卡牌的顯示
    /// </summary>
    public void SetupCard(CardBase data)
    {
        cardData = data;
        if (cardNameText) cardNameText.text = data.cardName;
        if (costText) costText.text = data.cost.ToString();
        if (descriptionText) descriptionText.text = data.description;
        if (cardImage && data.cardImage) cardImage.sprite = data.cardImage;

        // 也可根據 cardType 來切換背景顏色
        switch (data.cardType)
        {
            case CardType.Attack:
                if (cardBackground) cardBackground.color = Color.red;
                break;
            case CardType.Skill:
                if (cardBackground) cardBackground.color = Color.blue;
                break;
            case CardType.Movement:
                if (cardBackground) cardBackground.color = Color.green;
                break;
            case CardType.Relic:
                if (cardBackground) cardBackground.color = Color.yellow;
                break;
        }
    }

    #region 拖曳事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(FindObjectOfType<Canvas>().transform);
        // 讓拖曳時不會被父物件的Layout影響
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 讓卡牌跟著鼠標移動
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        // 加一條 Debug.Log，印出是否成功打到
        Debug.Log($"[OnEndDrag] Raycast from {worldPos}, hit = {hit.collider?.name}");

        // 2. 根據卡牌類型做不同邏輯
        switch (cardData.cardType)
        {
            case CardType.Attack:
                
                    // 先取得 BattleManager
                    BattleManager bm = FindObjectOfType<BattleManager>();

                    // 判斷是否為「急急如律令」
                    if (cardData.cardName == "急急如律令")
                    {
                        // 只要不是 HandPanel 就觸發九宮格選取
                        bm.StartAttackSelect(cardData);
                        Destroy(gameObject);          // 移除卡片 UI
                    }
                    if (cardData.cardName == "火")
                    {
                        // 只要不是 HandPanel 就觸發九宮格選取
                        bm.StartAttackSelect(cardData);
                        Destroy(gameObject);          // 移除卡片 UI
                    }
                    if (cardData.cardName == "木")
                    {
                        // 只要不是 HandPanel 就觸發九宮格選取
                        bm.StartAttackSelect(cardData);
                        Destroy(gameObject);          // 移除卡片 UI
                    }
                    if (cardData.cardName == "水")
                    {
                        // 只要不是 HandPanel 就觸發九宮格選取
                        bm.StartAttackSelect(cardData);
                        Destroy(gameObject);          // 移除卡片 UI
                     }
                     break;
                

            case CardType.Movement:
                HandleMovementCard(eventData);
                break;

            // 其他類型(e.g. Skill, Relic)也可依需求延伸
            default:
                // 其他類型暫時不做，回手牌
                ReturnToHand();
                break;
        }
    }

    // 攻擊牌：只有撞到 Enemy 時才觸發攻擊並消失，否則退回手牌

    #endregion

    // 移動牌：只要沒放回手牌，就觸發移動效果並消失
    // 若判定撞到HandPanel, 就回手牌
    private void HandleMovementCard(PointerEventData eventData)
    {
        RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
        // 檢查是否擊中 HandPanel（可用 Tag / 名稱 / 結構來識別）
        if (hit2D.collider != null)
        {
            HandPanelMarker handPanel = hit2D.collider.GetComponent<HandPanelMarker>();

            if (handPanel != null)
            {
                // 表示丟到 HandPanel => 回手牌
                ReturnToHand();
                return;
            }
        }

        // 如果不是 HandPanel => 觸發移動牌
        BattleManager bm = FindObjectOfType<BattleManager>();
        bm.UseMovementCard(cardData); // 進入後續的選擇Tile流程

        // 銷毀UI物件(避免卡留在手牌)
        Destroy(gameObject);

    }


    private void ReturnToHand()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 檢查卡牌拖曳結束時的落點
    /// </summary>
    private void CheckDropTarget(PointerEventData eventData)
    {
        // 對UI物件做GraphicRaycast or Physics Raycast (2D)
        // 這裡簡化: 
        //   - 若玩家拖到 Enemy UI 上 => 攻擊
        //   - 若拖到 Board Tile => 移動
        //   - 否則回到手牌
        RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
        if (hit2D.collider != null)
        {
            // 檢查是否是 Enemy
            Enemy e = hit2D.collider.GetComponent<Enemy>();
            if (e != null)
            {
                // 攻擊 or 技能對敵人
                UseCardOnEnemy(e);
                return;
            }
        }

        // 若上面都沒匹配 => 回手牌
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 對敵人使用此卡
    /// </summary>
    private void UseCardOnEnemy(Enemy enemyTarget)
    {
        // 向 BattleManager 發送 "PlayCard" 
        BattleManager bm = FindObjectOfType<BattleManager>();
        if (bm != null)
        {
            bm.PlayCard(cardData);
        }
        // 卡可能會被移到棄牌堆, UI做相應更新
    }
}
