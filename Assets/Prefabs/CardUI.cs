using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 參考")]
    public Image cardImage;

    [Header("資料參考")]
    public CardBase cardData; // 對應卡片資料的 ScriptableObject
    public Transform originalParent;
    private Canvas canvas;     // 用於計算拖曳位移（避免受 Canvas 縮放影響）
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform canvasRoot;
    private BattleManager battleManager;
    private Camera mainCamera;

    [Header("拖曳外觀")]
    [SerializeField, Range(0f, 1f)]
    private float draggingAlpha = 0.5f;

    private float originalAlpha = 1f;

    [Header("滑鼠懸停效果")]
    [SerializeField, Tooltip("滑鼠懸停時卡片向上移動的距離（UI 座標單位）")]
    private float hoverMoveDistance = 20f;

    [SerializeField, Tooltip("滑鼠懸停時用於顯示的發光圖層（可為額外的 Image 或特效物件）")]
    private Image hoverGlowImage;

    [SerializeField, Tooltip("滑鼠懸停時的發光顏色")]
    private Color hoverGlowColor = Color.green;


    private Vector2 originalAnchoredPosition;
    private bool isDragging;
    private bool isHovering;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        canvasRoot = canvas != null ? canvas.transform : null;
        canvasGroup = GetComponent<CanvasGroup>();
        battleManager = FindObjectOfType<BattleManager>();
        mainCamera = Camera.main;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
        {
            originalAlpha = canvasGroup.alpha;
        }

        if (hoverGlowImage != null)
        {
            hoverGlowImage.gameObject.SetActive(false);
            hoverGlowImage.color = hoverGlowColor;
        }

    }

    /// <summary>
    /// 設定卡片顯示內容
    /// </summary>
    public void SetupCard(CardBase data)
    {
        cardData = data;
        if (cardImage != null && data != null && data.cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }
    }

    #region 拖曳事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        ResetHoverPosition();

        originalParent = transform.parent;
        if (canvasRoot != null)
        {
            transform.SetParent(canvasRoot);
        }


        SetCardAlpha(draggingAlpha);


        if (battleManager != null && cardData != null)
        {
            if (cardData.cardType == CardType.Attack)
            {
                battleManager.StartAttackSelect(cardData);
            }
            else if (cardData.cardType == CardType.Movement)
            {
                battleManager.UseMovementCard(cardData);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 以 Canvas 的 scaleFactor 校正拖曳位移，避免縮放造成位移過大/過小
        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        SetCardAlpha(originalAlpha);

        Camera targetCamera = mainCamera != null ? mainCamera : Camera.main;
        Vector2 worldPos = targetCamera != null
            ? targetCamera.ScreenToWorldPoint(eventData.position)
            : eventData.position;
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        bool used = false;

        if (battleManager != null && cardData != null)
        {
            if (cardData.cardType == CardType.Attack)
            {
                used = HandleAttackDrop(hit);
            }
            else if (cardData.cardType == CardType.Movement)
            {
                used = HandleMovementDrop(hit);
            }
        }

        // 使用成功就把這張卡片的 UI 移除；未使用則回到手牌
        if (used)
        {
            Destroy(gameObject);
        }
        else
        {
            ReturnToHand();
        }
    }
    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging)
        {
            return;
        }

        originalAnchoredPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = originalAnchoredPosition + Vector2.up * hoverMoveDistance;
        isHovering = true;

        SetHoverGlowVisible(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging)
        {
            return;
        }

        ResetHoverPosition();
    }

    // 攻擊牌：若拖到 Enemy 上會觸發使用；否則還原到手牌
    // （此備註對應上方 OnEndDrag 的行為說明）

    // 移動牌：不需要刻意拖回手牌，會直接進入可選移動格流程
    // 若偵測到是 HandPanel，則直接復位到手牌
    private void ReturnToHand()
    {
        SetCardAlpha(originalAlpha);
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }
        rectTransform.anchoredPosition = Vector2.zero;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        isDragging = false;
        isHovering = false;
        SetHoverGlowVisible(false);
    }

    private void SetCardAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    private void ResetHoverPosition()
    {
        if (!isHovering)
        {
            return;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
        isHovering = false;

        SetHoverGlowVisible(false);
    }

    private void SetHoverGlowVisible(bool visible)
    {
        if (hoverGlowImage != null)
        {
            hoverGlowImage.gameObject.SetActive(visible);
        }
    }

    public void SetHoverGlowColor(Color color)
    {
        hoverGlowColor = color;

        if (hoverGlowImage != null)
        {
            hoverGlowImage.color = color;
        }
    }
    
    private bool HandleAttackDrop(Collider2D hit)
    {
        if (hit != null)
        {
            Enemy enemy;
            if (hit.TryGetComponent(out enemy))
            {
                if (battleManager.OnEnemyClicked(enemy))
                {
                    return true;
                }
            }
        }

        battleManager.EndAttackSelect();
        return false;
    }

    private bool HandleMovementDrop(Collider2D hit)
    {
        if (hit != null)
        {
            BoardTile tile;
            if (hit.TryGetComponent(out tile))
            {
                return battleManager.OnTileClicked(tile);
            }
        }

        battleManager.CancelMovementSelection();
        return false;
    }
}
