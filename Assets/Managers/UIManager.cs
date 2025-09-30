using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject rulePanel;      // 規則面板
    [SerializeField] private GameObject settingsPanel;  // 設定面板

    [Header("UI Group (要往上推的部分)")]
    [SerializeField] private RectTransform uiGroup;     // 棄牌區 + 結束回合的容器

    [Header("Move Settings")]
    [SerializeField] private float moveUpDistance = 150f; // 往上推的距離
    [SerializeField] private float moveSpeed = 5f;        // 移動速度

    private Vector2 originalPos;   // UIGroup 原始位置
    private Vector2 targetPos;     // 目標位置
    private bool ruleOpen = false;
    private bool settingsOpen = false;

    private void Start()
    {
        if (uiGroup != null)
            originalPos = uiGroup.anchoredPosition;

        // 一開始先隱藏
        if (rulePanel != null) rulePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (uiGroup != null)
        {
            uiGroup.anchoredPosition = Vector2.Lerp(
                uiGroup.anchoredPosition,
                targetPos,
                Time.deltaTime * moveSpeed
            );
        }
    }

    /// <summary>
    /// 切換規則面板
    /// </summary>
    public void ToggleRulePanel()
    {
        ruleOpen = !ruleOpen;
        settingsOpen = false; // 確保同時間只有一個面板開
        rulePanel.SetActive(ruleOpen);
        settingsPanel.SetActive(false);

        UpdateUIGroupTarget();
    }

    /// <summary>
    /// 切換設定面板
    /// </summary>
    public void ToggleSettingsPanel()
    {
        settingsOpen = !settingsOpen;
        ruleOpen = false; // 確保同時間只有一個面板開
        settingsPanel.SetActive(settingsOpen);
        rulePanel.SetActive(false);

        UpdateUIGroupTarget();
    }

    /// <summary>
    /// 點擊 Panel 本身關閉
    /// </summary>
    public void ClosePanels()
    {
        ruleOpen = false;
        settingsOpen = false;
        rulePanel.SetActive(false);
        settingsPanel.SetActive(false);

        UpdateUIGroupTarget();
    }

    /// <summary>
    /// 更新 UIGroup 的目標位置
    /// </summary>
    private void UpdateUIGroupTarget()
    {
        if (ruleOpen)
            targetPos = originalPos + new Vector2(0, moveUpDistance);
        else
            targetPos = originalPos;
    }
}
