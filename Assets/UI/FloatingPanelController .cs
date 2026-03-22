using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 浮动面板控制器：按下指定键显示/隐藏面板，并自动将面板放置在鼠标位置附近，
/// 同时进行边界检测，确保面板不超出屏幕。
/// </summary>
public class FloatingPanelController : MonoBehaviour
{
    public static FloatingPanelController Instance { get; private set; }
    [Header("设置")]
    public KeyCode toggleKey = KeyCode.Space;   // 呼出/隐藏的按键
    public Vector2 offset = new Vector2(20, -20); // 鼠标偏移量（右，下）

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 初始隐藏
        Hide();
    }



    public void Show()
    {
        isVisible = true;
        UpdatePosition();           // 显示时更新位置到鼠标处
        canvasGroup.alpha = 1;       // 显示
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        isVisible = false;
        canvasGroup.alpha = 0;       // 隐藏
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void UpdatePosition()
    {
        // 获取鼠标的屏幕坐标
        Vector3 mousePos = Input.mousePosition;

        // 加上偏移量
        Vector3 newPos = mousePos + new Vector3(offset.x, offset.y, 0);

        // 获取面板的宽高
        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;

        // 屏幕边界限制（防止面板跑出屏幕）
        // 左边不能小于 0
        if (newPos.x < 0) newPos.x = 0;
        // 右边不能大于 Screen.width
        if (newPos.x + panelWidth > Screen.width) newPos.x = Screen.width - panelWidth;
        // 下边不能小于 0（屏幕坐标系 y=0 是底部）
        if (newPos.y < 0) newPos.y = 0;
        // 上边不能大于 Screen.height
        if (newPos.y + panelHeight > Screen.height) newPos.y = Screen.height - panelHeight;

        // 应用到面板位置
        rectTransform.position = newPos;
    }
}