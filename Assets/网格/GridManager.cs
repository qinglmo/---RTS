using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}

public interface IGridEntityProvider
{
    public IFactionMember GetFactionMember(Vector2Int pos);
    public BaseUnit GetUnit(Vector2Int pos);
    public BaseBuilding GetBuilding(Vector2Int pos);
    public IResource GetResource(Vector2Int pos);
}

public interface IRegisterEntity //注册资源，地形
{
    public bool TryRegisterResource(Vector2Int grid, IResource resource);
}

// ======================= GridManager 实现 =======================

public class GridManager : MonoBehaviour, IOccupyGridManager, ISnapToGrid, IGridPassableCheck, IGridEntityProvider, IRegisterEntity
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------- 网格配置 -----------------------
    [Header("网格设置")]
    public Vector2 gridOrigin = Vector2.zero;   // 网格原点，比如 (0,0)
    public float gridSpacing = 1f;              // 网格间距，比如 1 个单位
    // 网格边界（只读，可根据需要调整）
    public static readonly int GridWidth = 30;          // 水平方向格子数
    public static readonly int GridHeight = 30;         // 垂直方向格子数

    // ----------------------- 数据容器 -----------------------
    public static Dictionary<Vector2Int, BaseTerrain> CellsOfTerrain = new Dictionary<Vector2Int, BaseTerrain>();
    private Dictionary<Vector2Int, IResource> resources = new Dictionary<Vector2Int, IResource>();
    private IOccupyEnity[,] occupiedEnityInCells = new IOccupyEnity[GridWidth, GridHeight];
    private HashSet<BaseUnit> unitOccupied = new HashSet<BaseUnit>(); // 表示单位是否处于占用状态，非占用状态必须处于移动状态。

    // ----------------------- IGridEntityProvider 实现 -----------------------
    public IFactionMember GetFactionMember(Vector2Int pos)
    {
        IFactionMember enity = occupiedEnityInCells[pos.x, pos.y] as IFactionMember;
        if (enity as MonoBehaviour == null)
            return null;
        return enity;
    }

    public BaseUnit GetUnit(Vector2Int pos)
    {
        IOccupyEnity enity = occupiedEnityInCells[pos.x, pos.y];
        return enity as BaseUnit;
    }

    public BaseBuilding GetBuilding(Vector2Int pos)
    {
        IOccupyEnity enity = occupiedEnityInCells[pos.x, pos.y];
        return enity as BaseBuilding;
    }

    public IResource GetResource(Vector2Int pos)
    {
        if (resources.ContainsKey(pos))
        {
            return resources[pos];
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
        if (occupiedEnityInCells[gridPos.x, gridPos.y] != null)
        {
            // 格子已被占用
            return false;
        }
        occupiedEnityInCells[gridPos.x, gridPos.y] = occupy;
        return true;
    }

    /// <summary>
    /// 释放某个格子（当单位离开或销毁时调用）
    /// </summary>
    public void Release(Vector2Int gridPos, IOccupyEnity unit)
    {
        if (unit == occupiedEnityInCells[gridPos.x, gridPos.y])
        {
            occupiedEnityInCells[gridPos.x, gridPos.y] = null;
        }
    }

    // ----------------------- IRegisterEntity 实现 -----------------------
    public bool TryRegisterResource(Vector2Int grid, IResource resource)
    {
        if (resources.ContainsKey(grid))
        {
            return false;
        }
        resources[grid] = resource;
        return true;
    }

    // ----------------------- IGridPassableCheck 实现 -----------------------
    /// <summary>
    /// 检查格子是否被占用（可选）被占用返回true
    /// </summary>
    public bool IsOccupied(Vector2Int gridPos)
    {
        if (occupiedEnityInCells[gridPos.x, gridPos.y] != null)
        {
            // 格子已被占用
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
        // 边界检查：坐标必须在 [0, GridWidth-1] 和 [0, GridHeight-1] 范围内
        if (gridPos.x < 0 || gridPos.x >= GridWidth || gridPos.y < 0 || gridPos.y >= GridHeight)
        {
            return false;   // 超出边界，无法占用
        }
        return true;
    }

    // ----------------------- ISnapToGrid 实现 -----------------------
    // 将不规则坐标转换为规则的网格坐标。
    public Vector2Int WorldToCell(Vector2 rawWorldPos)
    {
        float gridX = Mathf.Round((rawWorldPos.x - gridOrigin.x) / gridSpacing) * gridSpacing + gridOrigin.x;
        float gridY = Mathf.Round((rawWorldPos.y - gridOrigin.y) / gridSpacing) * gridSpacing + gridOrigin.y;
        return new Vector2Int((int)gridX, (int)gridY);
    }

    public Vector2 CellToWorld(Vector2Int gridPos)
    {
        return gridPos;
    }
}