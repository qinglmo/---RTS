using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance;          // 方便外部调用

    [Header("References")]
    [SerializeField] private GridManager grid;         // 你的网格类组件
    [SerializeField] private float groundPlaneY = 0f; // 地面平面高度（假设Y轴向上）
    [SerializeField] private LayerMask groundLayer;    // 用于射线检测（可选）

    private Camera mainCamera;
    private GameObject ghostBuilding;               // 当前跟随鼠标的虚化建筑
    private BuildingData selectedPrefab;              // 当前选中的建筑预制体
    private bool isBuilding = false;                 // 是否处于建造模式

    private void Awake()
    {
        // 简单单例
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        
    }
    private void Start()
    {
        grid = GridManager.Instance;
        mainCamera=Camera.main;
    }
    private void Update()
    {
        if (!isBuilding) return;

        // 1. 更新虚化建筑位置（对齐网格）
        UpdateGhostPosition();

        // 2. 检测输入
        if (Input.GetMouseButtonDown(0)) // 左键放置
        {
            TryPlaceBuilding();
        }
        else if (Input.GetMouseButtonDown(1)) // 右键取消
        {
            CancelBuilding();
        }
    }

    /// <summary>
    /// 由UI按钮调用，开始建造模式
    /// </summary>
    /// <param name="prefab">要建造的建筑预制体</param>
    public void StartBuilding(BuildingData prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("建筑预制体为空！");
            return;
        }

        // 如果已经在建造，先取消当前建造
        if (isBuilding) CancelBuilding();

        selectedPrefab = prefab;

        // 实例化虚化建筑（半透明、无碰撞）
        ghostBuilding = Instantiate(selectedPrefab.prefab);
        var building = ghostBuilding.GetComponent<BaseBuilding>();
        if (building != null)
        {
            building.Initialize();//抢先初始化，避免虚化建筑占用世界
        }
        SetupGhost(ghostBuilding);

        isBuilding = true;
    }

    /// <summary>
    /// 将普通建筑设置为虚化状态
    /// </summary>
    private void SetupGhost(GameObject ghost)
    {
        // 设置所有材质半透明（alpha = 0.5）
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.materials;
            foreach (Material mat in mats)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = 0.5f;
                    mat.color = c;
                }
                // 可进一步设置渲染模式为透明，此处简化
            }
            renderer.materials = mats;
        }

        // 禁用所有碰撞器，避免干扰射线检测
        Collider[] colliders = ghost.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;

        // 可选：设置图层为Ignore Raycast，防止阻挡自身射线
        ghost.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    /// <summary>
    /// 更新虚化建筑的位置，使其对齐网格
    /// </summary>
    private void UpdateGhostPosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPositionOnPlane();
        // 转换为网格坐标并获取对齐后的世界坐标
        Vector2Int cell = grid.WorldToCell(mouseWorldPos);
        Vector3 snappedPos = grid.CellToWorld(cell);
        ghostBuilding.transform.position = snappedPos;
    }

    /// <summary>
    /// 通过射线获取鼠标在指定平面上的世界坐标
    /// </summary>
    private Vector3 GetMouseWorldPositionOnPlane()
    {
        float distance = 10f; // 可根据实际场景调整
        Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return worldPos;
    }

    /// <summary>
    /// 尝试放置建筑
    /// </summary>
    private void TryPlaceBuilding()
    {
        if (!CanPlace(grid.WorldToCell(ghostBuilding.transform.position)))
        {
            Debug.Log("此处无法建造！");
            return;
        }

        // 实例化真实建筑
        GameObject newBuilding = Instantiate(selectedPrefab.prefab, ghostBuilding.transform.position, ghostBuilding.transform.rotation);
        ResourceSystem.TryBuild(selectedPrefab.foodCost, selectedPrefab.woodCost,selectedPrefab.stoneCost);
        // 调用建筑的初始化方法（假设建筑上有 IBuilding 接口）
        IBuilding buildingComp = newBuilding.GetComponent<IBuilding>();
        if (buildingComp != null)
        {
            buildingComp.Initialize(ghostBuilding.transform.position);
        }

        // 建造成功，退出建造模式
        CancelBuilding();
    }

    /// <summary>
    /// 检查某位置是否允许放置建筑
    /// </summary>
    private bool CanPlace(Vector2Int position)
    {
        // 使用网格类的方法检查格子是否空闲（假设网格类有 IsCellFree）
        // 如果网格类没有提供，你可以自行维护一个占用列表
        if (grid != null)
        {
            return !grid.IsOccupied(position);
        }

        // 备选方案：使用Physics.CheckSphere检测碰撞（需要建筑有碰撞体）
        // float checkRadius = 0.1f;
        // return !Physics.CheckSphere(position, checkRadius, obstacleLayer);

        // 默认允许（需根据实际情况修改）
        return true;
    }

    /// <summary>
    /// 取消建造，清理虚化建筑
    /// </summary>
    private void CancelBuilding()
    {
        if (ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }
        isBuilding = false;
        selectedPrefab = null;
    }
}

/// <summary>
/// 可选接口，建筑实现此接口以接收初始化调用
/// </summary>
public interface IBuilding
{
    void Initialize(Vector3 position);
}