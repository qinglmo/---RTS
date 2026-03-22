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
        List<Vector2Int> paths = Pathfinding.FindPathToAdjacentAStar(baseUnit.movement.stepMover.CurrentPos,
            target, baseUnit, MovementRules.IsWalkable,MovementRules.IsPreOccupyable);
        if (paths != null && paths.Count > 1)
        {
            paths.RemoveAt(0);          // 移除起点
        }
        if (paths != null && paths.Any())
            baseUnit.movement.MoveToGridWithPaths(paths.Last(), paths);
    }
}

