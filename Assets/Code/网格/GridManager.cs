using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

// ======================= 接口定义 =======================

public interface IOccupyGridManager
{
    public bool TryOccupy(Vector2Int gridPos, IOccupyEnity unit);
    public void Release(Vector2Int gridPos, IOccupyEnity unit);
    /// <summary>
    /// 判断单位是否处于占用状态，非占用状态的单位需要尽早离开格子
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool IsOccupiedForUnit(BaseUnit unit);
    public void ClearOccupyGrid(BaseUnit unit);
}

public interface ISnapToGrid
{
    public Vector2Int WorldToCell(Vector2 rawWorldPos);
    public Vector2 CellToWorld(Vector2Int gridPos);
}

public interface IGridPassableCheck
{
    public bool IsOccupied(Vector2Int gridPos);
    public bool BoundsChecking(Vector2Int gridPos);

    public bool IsOccupied_Static(Vector2Int gridPos);
}




// ======================= GridManager 实现 =======================

public class GridManager : MonoBehaviour, IOccupyGridManager, ISnapToGrid, IGridPassableCheck
{
    // ----------------------- 单例 -----------------------
    private static GridManager instance;
    public static GridManager Instance
    {
        get { return instance; }
        private set { }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [Header("Tilemap 图层")]
    [SerializeField] public Tilemap terrainTilemap;      // 地形（用于计算边界）
    public Tilemap resourceTilemap;//地表
    public Grid grid;

    // ----------------------- 网格配置 -----------------------
    [Header("网格设置")]
    public Vector2 gridOrigin = Vector2.zero;   // 网格原点，比如 (0,0)
    public float gridSpacing = 1f;              // 网格间距，比如 1 个单位

    // ----------------------- 数据容器 -----------------------
    private Dictionary<Vector2Int, IOccupyEnity> occupiedEnityInCells = new Dictionary<Vector2Int, IOccupyEnity>();
    private HashSet<BaseUnit> unitOccupied = new HashSet<BaseUnit>(); // 表示单位是否处于占用状态，非占用状态必须处于移动状态。

    // ----------------------- IGridEntityProvider 实现 -----------------------
    public IOccupyEnity GetOccupiedEnity(Vector2Int pos)
    {
        if (!BoundsChecking(pos))
            return null;
        if (occupiedEnityInCells.ContainsKey(pos))
        {
            return occupiedEnityInCells[pos];
        }
        return null;
    }

    // ----------------------- IOccupyGridManager 实现 -----------------------
    public void ClearOccupyGrid(BaseUnit unit)
    {
        if (unitOccupied.Contains(unit))
            unitOccupied.Remove(unit);
    }

    public bool IsOccupiedForUnit(BaseUnit unit)
    {
        if (unitOccupied.Contains(unit))
            return true;
        return false;
    }

    /// <summary>
    /// 尝试占用某个格子
    /// </summary>
    /// <param name="gridPos">网格坐标（已对齐的整数坐标）</param>
    /// <param name="unit">申请占用的单位</param>
    /// <returns>是否成功占用</returns>
    public bool TryOccupy(Vector2Int gridPos, IOccupyEnity occupy)
    {
        // 边界检查：坐标必须在 [0, GridWidth-1] 和 [0, GridHeight-1] 范围内
        if (!BoundsChecking(gridPos))
        {
            return false;   // 超出边界，无法占用
        }
        if (occupiedEnityInCells.ContainsKey(gridPos))
        {
            // 格子已被占用
            return false;
        }
        occupiedEnityInCells[gridPos] = occupy;
        return true;
    }

    /// <summary>
    /// 释放某个格子（当单位离开或销毁时调用）
    /// </summary>
    public void Release(Vector2Int gridPos, IOccupyEnity unit)
    {
        if (occupiedEnityInCells.ContainsKey(gridPos))
        {
            //唉，还真是这个检查的问题，这也太阴了，浪费我那么多时间。忘记了友军穿过可能释放了其他单位的占用，下次就算想到了也要加注释，不然再下次可能忘记就删了。
            if(occupiedEnityInCells[gridPos] ==unit )//需要严格限制，否则容易出bug
            {
                occupiedEnityInCells.Remove(gridPos);
            }
        }
    }

    // ----------------------- IGridPassableCheck 实现 -----------------------
    /// <summary>
    /// 检查格子是否被占用（可选）被占用返回true
    /// </summary>
    public bool IsOccupied(Vector2Int gridPos)
    {
        if (!BoundsChecking(gridPos))
            return true;
        if (occupiedEnityInCells.ContainsKey(gridPos))
        {
            // 格子已被占用
            return true;
        }
        return false;
    }
    public bool IsOccupied_Static(Vector2Int gridPos)//提供静态地图占用判断,忽略动态单位
    {
        if(!BoundsChecking(gridPos))
            return true;
        if(occupiedEnityInCells.ContainsKey(gridPos))
        {
            if(occupiedEnityInCells[gridPos] is BaseBuilding)
                return true;
            if(occupiedEnityInCells[gridPos] is SurfaceResources)
                return true;
        }
        return false;
    }

    /// <summary>
    ///  可用返回true，不可以返回false
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public bool BoundsChecking(Vector2Int gridPos)
    {
        if(terrainTilemap.HasTile((Vector3Int)gridPos))
            return true;
        return false;
    }

    // ----------------------- ISnapToGrid 实现 -----------------------
    // 将不规则坐标转换为规则的网格坐标。
    public Vector2Int WorldToCell(Vector2 rawWorldPos)
    {
        rawWorldPos=rawWorldPos+Vector2.one/2;//TileMap，这逻辑是真绕。逻辑位置和瓦片中心有偏移，世界位置和逻辑位置也有固定的偏移，这意味着调整了逻辑位置和瓦片中心的偏移，那世界位置相对瓦片中心的偏移也会改变。
        return (Vector2Int)grid.WorldToCell(rawWorldPos);
    }
    public Vector2 CellToWorld(Vector2Int gridPos)
    {
        return grid.CellToWorld((Vector3Int)gridPos) ;
    }
}