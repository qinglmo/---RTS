using UnityEngine;

public class MouseHoverControl : MonoBehaviour
{
    private static MouseHoverControl instance;
    public static MouseHoverControl Instance => instance;

    private ISnapToGrid snapToGrid;

    [Tooltip("悬停触发所需秒数")]
    [SerializeField] private float hoverTime = 0.3f;

    private float timer = 0f;
    private Vector2Int lastGrid;               // 上一次有效的格子坐标
    private bool hasTriggered = false;          // 当前格子是否已触发过悬停事件

    // 定义一个无效格子值（当SnapToGrid返回此值时视为鼠标不在任何格子上）
    private readonly Vector2Int invalidGrid = new Vector2Int(int.MinValue, int.MinValue);

    // 缓存主摄像机
    private Camera mainCamera;

    // 用于射线检测的平面（假设格子平面为 z=0，可根据实际修改）
    private Plane gridPlane;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        snapToGrid = GridManager.Instance;   // 假设 GridManager 实现了 ISnapToGrid
        mainCamera = Camera.main;

        lastGrid = invalidGrid;
    }

    private void Update()
    {
        float distance = 10f; // 可根据实际场景调整
        Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // 3. 获取当前鼠标所在格子
        Vector2Int currentGrid = snapToGrid.WorldToCell(worldPos);

        if (currentGrid != lastGrid)
        {
            // 切换到另一个格子
            lastGrid = currentGrid;
            timer = 0f;
            hasTriggered = false;
            OnHoverCancel();
        }
        else
        {
            // 同一个格子持续悬停
            if (!hasTriggered)
            {
                timer += Time.deltaTime;
                if (timer >= hoverTime)
                {
                    OnHoverTrigger(currentGrid);
                    hasTriggered = true;
                }
            }
        }
    }
    /// <summary>
    /// 悬停满 hoverTime 秒后触发的事件
    /// </summary>
    private void OnHoverTrigger(Vector2Int grid)
    {
        if(!GridManager.Instance.BoundsChecking(grid))
            return;
        FloatingPanelController.Instance.Show();
        FloatingPanel.Instance.UpdateInfo(grid,GridManager.Instance.GetFactionMember(grid));

    }
    private void OnHoverCancel()
    {
        FloatingPanelController.Instance.Hide();
    }
}