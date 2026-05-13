using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITargetEvaluation
{
    IHasPosition Evaluate(EntityCollection targets, IHasPosition currentTarget, IFactionMember currentUnit);
}

public class BrainDecision : MonoBehaviour, ITargetEvaluation
{
    [SerializeField] private float minDistanceDifference = 0.8f; // 距离差异阈值（世界单位）

    /// <summary>
    /// 评估目标：优先选择最近且未被包围的单位；若无单位则考虑建筑。
    /// 当最近目标比当前目标近超过阈值时切换。
    /// </summary>
    public IHasPosition Evaluate(EntityCollection targets, IHasPosition currentTarget, IFactionMember currentUnit)
    {
        // 安全处理：若当前目标已被销毁，置空
        if (!IsAlive(currentTarget))
            currentTarget = null;

        // 若当前目标存活，检查是否被包围，若是则强制重新选择
        if (IsAlive(currentTarget) && IsTargetSurrounded(currentTarget.GridPos, currentUnit))
            currentTarget = null;

        IHasPosition bestTarget = null;
        if (targets.Units != null && targets.Units.Any())
        {
            // 获取最佳目标（优先单位
            var best = GetBestTarget(targets.Units, currentUnit);
            bestTarget = best as BaseUnit;
        }

        if (bestTarget == null &&targets.Buildings != null && targets.Buildings.Any())
        {
            var best = GetBestTarget(targets.Buildings,currentUnit);
            bestTarget = best as BaseBuilding;
        }
        
        // 没有有效目标，保持原样
        if (bestTarget == null)
            return currentTarget;

        // 没有当前目标，直接返回最佳目标
        if (currentTarget == null)
            return bestTarget;
        // 7. 类型优先级处理：如果最佳是单位且当前是建筑，直接切换
        bool bestIsUnit = bestTarget is BaseUnit;
        bool currentIsUnit = currentTarget is BaseUnit;

        if (bestIsUnit && !currentIsUnit) // 单位优先于建筑
            return bestTarget;

        // 8. 如果当前是单位而最佳是建筑，则不切换（保持单位）
        if (!bestIsUnit && currentIsUnit)
            return currentTarget;
        // 如果最佳目标就是当前目标，保持
        if (AreSameTarget(bestTarget, currentTarget))
            return currentTarget;

        // 计算距离差，决定是否切换
        float currentDist = Vector2.Distance(currentUnit.Position, currentTarget.Position);
        float bestDist = Vector2.Distance(currentUnit.Position, bestTarget.Position);

        if (currentDist - bestDist > minDistanceDifference)
            return bestTarget;
        else
            return currentTarget;
    }
    /// <summary>
    /// 获取最佳目标：先过滤出未被包围的目标，优先选择最近的单位，
    /// </summary>
    private IHasPosition GetBestTarget(IEnumerable<IHasPosition> targets, IFactionMember currentUnit)
    {
        // 过滤空引用和已销毁对象，并排除被包围的目标
        var validTargets = targets.Where(t => IsAlive(t) && !IsTargetSurrounded(t.GridPos, currentUnit)).ToList();

        if (!validTargets.Any())
            return null;


        return validTargets.OrderBy(u => Vector2.Distance(currentUnit.Position, u.Position)).First();
    }
    /// <summary>
    /// 判断目标是否被包围（近战单位无法找到攻击位置）
    /// 检查目标周围四个方向是否存在可占据的格子。
    /// </summary>
    private bool IsTargetSurrounded(Vector2Int targetGrid, IFactionMember currUnit)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = targetGrid + dir;
            if (MovementRules.IsPreOccupyable(neighbor, currUnit))
                return false; // 至少有一个可用攻击位
        }
        return true; // 所有方向都不可占据
    }
    // 辅助方法：安全判断接口对象是否存活（Unity对象未被销毁）
    private bool IsAlive(IHasPosition obj)
    {
        return (obj as MonoBehaviour) != null;
    }
    // 安全比较两个接口是否指向同一个存活对象
    private bool AreSameTarget(IHasPosition a, IHasPosition b)
    {
        if (!IsAlive(a) || !IsAlive(b)) return false;
        return a == b; // 此时两者都存活，可以用引用比较
    }
}