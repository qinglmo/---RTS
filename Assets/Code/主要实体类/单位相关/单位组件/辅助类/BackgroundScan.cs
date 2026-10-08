using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static IntentionDecision;

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
    List<IHasPosition> GetUnitTargetsInRange( Faction Faction,List<Vector2Int> cells);
    List<IHasPosition> GetAllTargetsInRange( Faction enemyFaction,List<Vector2Int> cells);
    
}
//拆分接口细粒度，取决于使用者的使用程度，如果使用者只需要其中一个方法，那么应该拆分。后期查看代码时可以清晰理解使用者的意图。
public interface IResourceProvider
{
    IEnumerable<IResource> GetAllResourceInCircle(List<Vector2Int> cells);
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
    /// 寻找范围内的建筑和单位目标,目前只接收圆形范围
    /// </summary>
    /// <param name="range"></param>
    /// <param name="attackRangeCheckMode"></param>
    /// <param name="enemyFaction"></param>
    /// <returns></returns>
    public List<IHasPosition> GetAllTargetsInRange( Faction enemyFaction,List<Vector2Int> cells)
    {
       var targets = new List<IHasPosition>();
        foreach (var cell in cells)
        {
            if (!GridManager.BoundsChecking(cell))
                continue;
            IFactionMember target = GridManager.GetFactionMember(cell);

            if (target!=null)
            {
                if (target.Faction == enemyFaction)
                {
                    targets.Add(target);
                }
            }
        }
        return targets;
    }
    /// <summary>
    /// 在指定范围内寻找符合标签的敌人
    /// </summary>
    public List<IHasPosition> GetUnitTargetsInRange( Faction Faciton, List<Vector2Int> cells)
    {
        var targets = new List<IHasPosition>();
        foreach (var cell in cells)
        {
            if (!GridManager.BoundsChecking(cell))
                continue;
            BaseUnit unit = GridManager.GetUnit(cell);
            
            if (unit != null)
            {
                if (unit.Faction == Faciton)
                {
                    targets.Add(unit);
                }
            }
        }
        return targets;
    }

    public IEnumerable<IResource> GetAllResourceInCircle(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            if (!GridManager.BoundsChecking(cell))
                continue;
            IResource resource = GridManager.GetResource(cell);
            if (resource != null)
            {
                yield return resource;
            }
        }
    }



}
