using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//暂时这样写，未来看情况扩展
public static class RouteDecision
{
    /// <summary>
    /// 向底层发布一个移动到目标的命令，如果目标被占用，会尝试选择其他位置
    /// 用于向敌人移动，向一个有占领的地点移动。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="final"></param>
    /// <param name="baseUnit"></param>
    public static void UniversalRouteDecision(BaseUnit baseUnit, Vector2Int final)
    {
        Vector2Int? alternativeGoal=null;
        if (MovementRules.IsOccupyable(final,baseUnit))
        {
            alternativeGoal=final;
        }
        else 
            alternativeGoal = PointDecision.FindAlternativeOccupyCell2(final, baseUnit); // 寻找当前位置周围的可占用格子
        if (alternativeGoal.HasValue)
        {
            List<Vector2Int> paths = Pathfinding.FindPathAStar(baseUnit.movement.stepMover.CurrentPos, alternativeGoal.Value, baseUnit, MovementRules.IsWalkable);
            if (paths != null && paths.Count > 1)
            {
                paths.RemoveAt(0);          // 移除起点
            }
            if (paths != null && paths.Any()) 
                baseUnit.movement.MoveToGridWithPaths(alternativeGoal.Value, paths);
        }
    }
    /// <summary>
    /// 对于敌对建筑或者单位，可以直接接近对方。
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <param name="target"></param>
    public static void TargetApproaching(BaseUnit baseUnit, Vector2Int target)
    {
        List<Vector2Int> paths = Pathfinding.FindPathToAdjacentAStar(
            target, baseUnit, MovementRules.IsWalkable,MovementRules.IsPreOccupyable);
        if (paths != null && paths.Count > 1)
        {
            paths.RemoveAt(0);          // 移除起点
        }
        if (paths != null && paths.Any())
            baseUnit.movement.MoveToGridWithPaths(paths.Last(), paths);
    }
    /// <summary>
    /// 更聪明的寻路，不会原地发呆，会尽可能接近
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <param name="target"></param>
    public static void TargetApproaching2(BaseUnit baseUnit, Vector2Int target)
    {
        List<Vector2Int> paths = Pathfinding.FindPathToAdjacentAStar2(
            target, baseUnit, MovementRules.IsWalkable, MovementRules.IsPreOccupyable);
        if (paths != null && paths.Count > 1)
        {
            paths.RemoveAt(0);          // 移除起点
        }
        if (paths != null && paths.Any())
            baseUnit.movement.MoveToGridWithPaths(paths.Last(), paths);
    }
    /// <summary>
    /// 更聪明的寻路，更优异的性能，更简洁的架构
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <param name="target"></param>
    public static void TargetApproaching_new(BaseUnit baseUnit, Vector2Int target)
    {
        var targetsPos=PointDecision.FindClosestValidRing(target, baseUnit, MovementRules.IsWalkable, MovementRules.IsPreOccupyable, 5);
        var paths = Pathfinding.FindPathMultiTarget(baseUnit.GridPos, targetsPos, target,baseUnit, MovementRules.IsWalkable);
        if (paths != null && paths.Count > 1)
        {
            paths.RemoveAt(0);          // 移除起点
        }
        if (paths != null && paths.Any())
            baseUnit.movement.MoveToGridWithPaths(paths.Last(), paths);
    }
    public static void MoveToOther(BaseUnit baseUnit)//让单位移动到其他位置
    {
        var otherPos=PointDecision.FindAlternativeOccupyCell2(baseUnit.GridPos, baseUnit);
        if (otherPos.HasValue)
        {
            TargetApproaching_new(baseUnit, otherPos.Value);
        }
    }
}

