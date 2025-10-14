using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager 完整版
/// - SettingsButton 打開 Settings Panel
/// - 點 Settings Panel 背景 關閉 Settings Panel
/// - GearButton 打開 Music Panel
/// - Rule Panel 支援翻頁，並可以選擇是否讓 uiGroup 上移（若你需要可啟用）
/// - Deck / Discard 為獨立頁面
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels (拖入對應的 GameObject)")]
    [SerializeField] private GameObject rulePanel;       // 規則面板（節點）
    [SerializeField] private GameObject settingsPanel;   // 設定面板（節點）
    [SerializeField] private GameObject musicPanel;      // 音樂面板（節點）
    [SerializeField] private GameObject deckPanel;       // 牌庫面板（節點）
    [SerializeField] private GameObject discardPanel;    // 棄牌面板（節點）

    [Header("Buttons (可選，若在 Inspector 指定，程式會自動綁定事件)")]
    [SerializeField] private Button settingsButton;      // 開 Settings 的按鈕（你說的 Settings Button）
    [SerializeField] private Button gearButton;          // 齒輪按鈕（你說的 GearButton）
    [SerializeField] private Button settingsPanelBackgroundButton; // 若 Settings Panel 背景為 Button，可直接指定

    [Header("Rule Page (Image + Sprites)")]
    [SerializeField] private Image ruleImage;            // Rule 面板上顯示圖片的 Image
    [SerializeField] private Sprite[] rulePages;         // 規則圖片陣列
    [SerializeField] private Button ruleNextButton;      // 下一頁
    [SerializeField] private Button rulePrevButton;      // 上一頁
    [SerializeField] private Button ruleCloseButton;     // Rule 關閉按鈕

    [Header("Optional: UIGroup (若需要 rule 開啟時上移)")]
    [SerializeField] private RectTransform uiGroup;      // 要上移的 UI group（如果不需要可留空）
    [SerializeField] private float moveUpDistance = 150f;
    [SerializeField] private float moveSpeed = 6f;

    // internal states
    private int currentRulePage = 0;
    private bool settingsOpen = false;

    // uiGroup positions
    private Vector2 uiOriginalPos = Vector2.zero;
    private Vector2 uiTargetPos = Vector2.zero;

    // ================================
    // 初始化
    // ================================
    private void Awake()
    {
        // 如果你有指定 uiGroup，抓原始位置
        if (uiGroup != null)
        {
            uiOriginalPos = uiGroup.anchoredPosition;
            uiTargetPos = uiOriginalPos;
        }

        // 先全部關閉（安全）
        if (rulePanel) rulePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (musicPanel) musicPanel.SetActive(false);
        if (deckPanel) deckPanel.SetActive(false);
        if (discardPanel) discardPanel.SetActive(false);

        // 若 ruleImage 有 sprite 陣列，預設顯示第 0 張
        if (ruleImage != null && rulePages != null && rulePages.Length > 0)
        {
            currentRulePage = 0;
            ruleImage.sprite = rulePages[currentRulePage];
        }

        // 自動綁定按鈕事件（方便 Inspector 直接指定按鈕）
        WireUpButtons();
    }

    private void Update()
    {
        // 平滑移動 uiGroup（只有在 uiGroup 指定時）
        if (uiGroup != null)
        {
            uiGroup.anchoredPosition = Vector2.Lerp(uiGroup.anchoredPosition, uiTargetPos, Time.deltaTime * moveSpeed);
        }
    }

    // ================================
    // 按鈕自動綁定（如果你在 Inspector 指定了 Button）
    // ================================
    private void WireUpButtons()
    {
        // Settings Button: 開啟 Settings Panel
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        // Gear Button: 開啟 Music Panel
        if (gearButton != null)
        {
            gearButton.onClick.RemoveAllListeners();
            gearButton.onClick.AddListener(OnGearButtonClicked);
        }

        // 如果你在 Inspector 有把 Settings Panel 背景的 Button 指定進來，也綁上關閉
        if (settingsPanelBackgroundButton != null)
        {
            settingsPanelBackgroundButton.onClick.RemoveAllListeners();
            settingsPanelBackgroundButton.onClick.AddListener(OnSettingsPanelBackgroundClicked);
        }

        // Rule 翻頁按鈕
        if (ruleNextButton != null)
        {
            ruleNextButton.onClick.RemoveAllListeners();
            ruleNextButton.onClick.AddListener(NextRulePage);
        }

        if (rulePrevButton != null)
        {
            rulePrevButton.onClick.RemoveAllListeners();
            rulePrevButton.onClick.AddListener(PrevRulePage);
        }

        if (ruleCloseButton != null)
        {
            ruleCloseButton.onClick.RemoveAllListeners();
            ruleCloseButton.onClick.AddListener(CloseRulePanel);
        }
    }

    // ================================
    // Settings Button / Settings Panel 行為
    // ================================
    /// <summary>
    /// Inspector 或按鈕應該綁到這個：Settings Button (打開 Settings Panel)
    /// </summary>
    public void OnSettingsButtonClicked()
    {
        // open settings panel (always opens settings; gear is separate)
        if (settingsPanel)
            settingsPanel.SetActive(true);

        settingsOpen = true;

        // 當開啟 Settings 時，建議關閉會衝突的 panels（依 UX 規則）
        if (rulePanel) rulePanel.SetActive(false);
        if (deckPanel) deckPanel.SetActive(false);
        if (discardPanel) discardPanel.SetActive(false);
        if (musicPanel) musicPanel.SetActive(false);

        // 如果你想 Settings 開啟不影響 uiGroup，就不調整 uiTargetPos
        // 若要 rule 才上移，這裡不改 uiTargetPos
    }

    /// <summary>
    /// 若你把 Settings Panel 背景設為 Button 並於 Inspector 拖入 settingsPanelBackgroundButton，
    /// 點擊背景會呼叫此方法以關閉 Settings Panel。
    /// </summary>
    public void OnSettingsPanelBackgroundClicked()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        settingsOpen = false;
    }

    // ================================
    // Gear Button 行為
    // ================================
    /// <summary>
    /// Gear 按鈕（獨立） → 打開 Music Panel
    /// Inspector 或按鈕應綁到這個方法
    /// </summary>
    public void OnGearButtonClicked()
    {
        // 當 gear 被按下時，直接打開音樂面板（你的需求是 Gear 單獨開 Music）
        if (musicPanel) musicPanel.SetActive(true);

        // 同時關閉 Settings（若想保留 Settings 可拿掉）
        //if (settingsPanel) settingsPanel.SetActive(false);
        //settingsOpen = false;

        // 關閉其他會互衝的 panel（選擇性）
        if (rulePanel) rulePanel.SetActive(false);
        if (deckPanel) deckPanel.SetActive(false);
        if (discardPanel) discardPanel.SetActive(false);
    }

    // ================================
    // Music Panel
    // ================================
    public void CloseMusicPanel()
    {
        if (musicPanel) musicPanel.SetActive(false);
    }

    // ================================
    // Rule Panel 與 UIGroup 行為（若你需要 rule 才上移 uiGroup）
    // ================================
    public void OpenRulePanel()
    {
        if (rulePanel) rulePanel.SetActive(true);

        // 當 rule 開啟時，如果你需要移動 uiGroup，則設定目標位置
        if (uiGroup != null)
            uiTargetPos = uiOriginalPos + new Vector2(0f, moveUpDistance);
    }

    public void CloseRulePanel()
    {
        if (rulePanel) rulePanel.SetActive(false);

        if (uiGroup != null)
            uiTargetPos = uiOriginalPos;
    }

    public void NextRulePage()
    {
        if (rulePages == null || rulePages.Length == 0) return;
        currentRulePage = (currentRulePage + 1) % rulePages.Length;
        if (ruleImage) ruleImage.sprite = rulePages[currentRulePage];
    }

    public void PrevRulePage()
    {
        if (rulePages == null || rulePages.Length == 0) return;
        currentRulePage = (currentRulePage - 1 + rulePages.Length) % rulePages.Length;
        if (ruleImage) ruleImage.sprite = rulePages[currentRulePage];
    }

    // ================================
    // Deck / Discard Panels
    // ================================
    public void OpenDeckPanel()
    {
        if (deckPanel) deckPanel.SetActive(true);

        // 建議關閉其他 panel，避免誤觸
        if (rulePanel) rulePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (musicPanel) musicPanel.SetActive(false);
        settingsOpen = false;
    }

    public void CloseDeckPanel()
    {
        if (deckPanel) deckPanel.SetActive(false);
    }

    public void OpenDiscardPanel()
    {
        if (discardPanel) discardPanel.SetActive(true);

        if (rulePanel) rulePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (musicPanel) musicPanel.SetActive(false);
        settingsOpen = false;
    }

    public void CloseDiscardPanel()
    {
        if (discardPanel) discardPanel.SetActive(false);
    }

    // ================================
    // 幫助方法：全部關閉（測試用）
    // ================================
    public void CloseAllPanels()
    {
        if (rulePanel) rulePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (musicPanel) musicPanel.SetActive(false);
        if (deckPanel) deckPanel.SetActive(false);
        if (discardPanel) discardPanel.SetActive(false);
        settingsOpen = false;

        if (uiGroup != null)
            uiTargetPos = uiOriginalPos;
    }
}
