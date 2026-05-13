using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class FriendlySupportEvaluation
{
    public static IHasPosition Evaluate(IReadOnlyList<BaseUnit> units, IHasPosition currentUnit)
    {
        // 筛选出正在攻击敌人的单位
        var attackingUnits = units.Where(u =>u.detector.CurrentTarget != null)
                                  .ToList();

        if (attackingUnits.Count == 0)
            return null;

        // 假设 IHasPosition 有 GridPos，或使用 GridManager 转换
        Vector2Int currentGrid = currentUnit.GridPos; // 若 IHasPosition 无 GridPos，可改为 GridManager.Instance.WorldToCell(currentUnit.Position)

        // 按距离升序排序，返回最近的
        return attackingUnits.OrderBy(u => DistanceCalculate.Heuristic(u.GridPos, currentGrid))
                             .FirstOrDefault();
    }
}
