using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static IntentionDecision;


public static  class BackgroundScan
{
    private static IGridEntityProvider entityProvider;
    private static IGridEntityProvider EntityProvider
    {
        get
        {
            if (entityProvider == null)
                entityProvider = GridEntityProvider.Instance;
            return entityProvider;
        }
    }
    /// <summary>
    /// 寻找范围内的建筑和单位目标,目前只接收圆形范围
    /// </summary>
    /// <param name="range"></param>
    /// <param name="attackRangeCheckMode"></param>
    /// <param name="enemyFaction"></param>
    /// <returns></returns>
    public static List<IHasPosition> GetAllTargetsInRange( Faction enemyFaction,List<Vector2Int> cells)
    {
       var targets = new List<IHasPosition>();
        foreach (var cell in cells)
        {
            IFactionMember target = EntityProvider.GetFactionMember(cell);

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
    public static List<IHasPosition> GetUnitTargetsInRange( Faction Faciton, List<Vector2Int> cells)
    {
        var targets = new List<IHasPosition>();
        foreach (var cell in cells)
        {
            BaseUnit unit = EntityProvider.GetUnit(cell);
            
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

    public static List<SurfaceResources> GetSurfaceResourcesInRange(List<Vector2Int> cells)
    {
        var resources = new List<SurfaceResources>();
        foreach (var cell in cells)
        {
            SurfaceResources resource = EntityProvider.GetResource(cell);
            if (resource != null)
            {
                resources.Add(resource);
            }
        }
        return resources;
    }
    public static List<BaseBuilding> GetBaseBuildings(List<Vector2Int> cells)
    {
        var buildings = new List<BaseBuilding>();
        foreach (var cell in cells)
        {
            BaseBuilding building = EntityProvider.GetBuilding(cell);
            if (building != null)
            {
                buildings.Add(building);
            }
        }
        return buildings;
    }
}
