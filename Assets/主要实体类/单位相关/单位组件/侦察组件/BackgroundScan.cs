using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TargetDetector;

// 定义在外部，例如在某个公共命名空间
public readonly struct EntityCollection
{
    public IReadOnlyList<BaseBuilding> Buildings { get; }
    public IReadOnlyList<BaseUnit> Units { get; }

    public EntityCollection(IReadOnlyList<BaseBuilding> buildings, IReadOnlyList<BaseUnit> units)
    {
        Buildings = buildings;
        Units = units;
    }
}
public interface ITargetProvider
{
    IEnumerable<IHasPosition> GetUnitTargetsInRange(Vector2Int center, float range, Faction enemyFaction);
    EntityCollection GetAllTargetsInRange(Vector2Int center, float range, Faction enemyFaction);
    
}
//拆分接口细粒度，取决于使用者的使用程度，如果使用者只需要其中一个方法，那么应该拆分。后期查看代码时可以清晰理解使用者的意图。
public interface IResourceProvider
{
    IEnumerable<IResource> GetAllResourceInCircle(Vector2Int center, float range);
}
public class BackgroundScan : MonoBehaviour, ITargetProvider,IResourceProvider
{
    private static GridManager gridManager;
    private static GridManager GridManager
    {
        get
        {
            if (gridManager == null)
                gridManager = GridManager.Instance;
            return gridManager;
        }
    }
    /// <summary>
    /// 寻找范围内的单位目标
    /// </summary>
    /// <param name="range"></param>
    /// <param name="attackRangeCheckMode"></param>
    /// <param name="enemyFaction"></param>
    /// <returns></returns>
    public IEnumerable<IHasPosition> GetUnitTargetsInRange(Vector2Int center, float range, Faction enemyFaction)
    {

        return FindUnitInCircle(center,range, enemyFaction);
    }
    /// <summary>
    /// 寻找范围内的建筑和单位目标
    /// </summary>
    /// <param name="range"></param>
    /// <param name="attackRangeCheckMode"></param>
    /// <param name="enemyFaction"></param>
    /// <returns></returns>
    public EntityCollection GetAllTargetsInRange(Vector2Int center,float range, Faction enemyFaction)
    {
       return FindAllEnemyInCircle(center, range, enemyFaction);
    }
    /// <summary>
    /// 在指定范围内寻找最近的、符合标签的敌人
    /// </summary>
    private IEnumerable<IHasPosition> FindUnitInCircle(Vector2Int center,float range, Faction Faciton)
    {
        var cells = GetCellsInCircle(center, range);
        foreach (var cell in cells)
        {
            BaseUnit unit = GridManager.GetUnit(cell);
            
            if (unit != null)
            {
                if (unit.Faction == Faciton)
                {
                    yield return unit;
                }
            }
        }
    }
    /// <summary>
    /// 在指定范围内寻找最近的、符合标签的敌人。同时收集建筑
    /// </summary>
    private EntityCollection FindAllEnemyInCircle(Vector2Int center,float range,Faction enemyFaciton)
    {
        var cells = GetCellsInCircle(center, range);
        var units = new List<BaseUnit>();
        var buildings= new List<BaseBuilding>();
        foreach (var cell in cells)
        {
            if (!GridManager.BoundsChecking(cell))
                continue;
            IFactionMember unit = GridManager.GetFactionMember(cell);

            if (unit!=null)
            {
                if (unit.Faction == enemyFaciton)
                {
                    if(unit as BaseUnit)
                        units.Add(unit as BaseUnit);
                    if(unit as BaseBuilding)
                        buildings.Add(unit as BaseBuilding);
                }
            }
        }
        return new EntityCollection(buildings, units);
    }
    /// <summary>
    /// 获取与指定圆形区域相交的所有格子坐标（基于格子中心到圆心的距离 ≤ 半径）
    /// </summary>
    private IEnumerable<Vector2Int> GetCellsInCircle(Vector2Int center, float radius)
    {
        // 计算需要检查的格子范围（基于半径 / 格子大小，向上取整）
        int cellRadius = (int)radius;

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {

                Vector2Int cell = new Vector2Int(center.x + x, center.y + y);
                if (cell == center)
                {
                    continue;//排除自身
                }
                // 获取格子中心的世界坐标
                Vector2 cellCenter = GridManager.CellToWorld(cell);
                // 如果格子中心在圆内，则返回该格子
                if (Vector2.Distance(cellCenter, center) <= radius)
                {
                    yield return cell;

                }
            }
        }
    }
    /// <summary>十字形范围内查找所有敌人（以自身为中心向上下左右延伸）</summary>
    private IEnumerable<IHasPosition> FindAllEnemyInCross(Vector2Int centerCell, float range, Faction enemyFaction)
    {
        int steps = Mathf.CeilToInt(range / GridManager.gridSpacing); // 单方向最大格子数

        // 水平方向（左右）
        for (int i = -steps; i <= steps; i++)
        {
            Vector2Int cell = new Vector2Int(centerCell.x + i, centerCell.y);
            var unit = GridManager.GetUnit(cell);
            if (unit != null && unit.Faction == enemyFaction)
                yield return unit;
        }

        // 垂直方向（上下），跳过中心格子避免重复
        for (int i = -steps; i <= steps; i++)
        {
            if (i == 0) continue;
            Vector2Int cell = new Vector2Int(centerCell.x, centerCell.y + i);
            var unit = GridManager.GetUnit(cell);
            if (unit != null && unit.Faction == enemyFaction)
                yield return unit;
        }
    }
    public IEnumerable<IResource> GetAllResourceInCircle(Vector2Int center, float range)
    {
        var cells = GetCellsInCircle(center, range);
        var resources= new List<IResource>();
        foreach (var cell in cells)
        {
            if (!GridManager.BoundsChecking(cell))
                continue;
            IResource resource = GridManager.GetResource(cell);
            if (resource != null)
                resources.Add(resource);
            
        }
        return resources;
    }


}
