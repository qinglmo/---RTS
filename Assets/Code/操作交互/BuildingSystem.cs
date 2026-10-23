using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance;          // 方便外部调用

    [Header("References")]
    [SerializeField] private GridManager grid;         // 你的网格类组件
    private Camera mainCamera;
    private GameObject imageObj;               // 当前跟随鼠标的建筑图片
    private SpriteRenderer imageRenderer;
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
        if (imageObj == null)
        {
            imageObj = new GameObject("BuildingImage");
        }
        imageRenderer=imageObj.AddComponent<SpriteRenderer>();
        imageObj.SetActive(false);
    }
    private void Update()
    {
        if (!isBuilding) return;

        // 1. 更新建筑图片位置（对齐网格）
        UpdateImagePosition();

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

        imageObj.SetActive(true);
        imageRenderer.sprite=prefab.icon;
        selectedPrefab = prefab;
        isBuilding = true;
    }


    /// <summary>
    /// 更新建筑图片的位置，使其对齐网格
    /// </summary>
    private void UpdateImagePosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPositionOnPlane();
        // 转换为网格坐标并获取对齐后的世界坐标
        Vector2Int cell = grid.WorldToCell(mouseWorldPos);
        Vector3 snappedPos = grid.CellToWorld(cell);
        imageObj.transform.position = snappedPos;
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
        var pos = grid.WorldToCell(imageObj.transform.position);
        if (!CanPlace(pos))
        {
            Debug.Log("此处无法建造！");
            return;
        }

        if (!ResourceSystem.TryBuild(selectedPrefab.foodCost, selectedPrefab.woodCost,selectedPrefab.stoneCost))
        {
            Debug.Log("资源不足！");
            return;
        }
        //设置建筑Tile
        grid.SetSurfaceTile(pos, selectedPrefab.buildingTile);
        //获取Tile实体
        var Building = grid.GetInstantiatedObject(pos);
        if(Building!=null)
        {
            // 目前没有初始化需求
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

        // 默认允许（需根据实际情况修改）
        return true;
    }

    /// <summary>
    /// 取消建造
    /// </summary>
    private void CancelBuilding()
    {
        if (imageObj != null)
        {
            imageObj.SetActive(false);
        }
        isBuilding = false;
        selectedPrefab = null;
    }
}
