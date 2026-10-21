using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITargetEvaluation
{
    public IHasPosition EvaluateUnitAndBuilding(List<IHasPosition> targets, TargetType PriorityTargetType,IHasPosition unit);
    IHasPosition EvaluateUnits(List<IHasPosition> units, IHasPosition currentUnit);
    IHasPosition EvaluateBuildings(List<IHasPosition> buildings, IHasPosition currentUnit);
    public bool EvaluateChangeable(IHasPosition target,IHasPosition currentTarget,float switchCost);
    public IHasPosition EvaluateHelpUnit(List<BaseUnit> targets, IHasPosition currentUnit);
}

public enum TargetType
{
    Unit,
    Building
}
public class BrainDecision : MonoBehaviour, ITargetEvaluation
{
    [SerializeField] private float minDistanceDifference = 0.8f; // 距离差异阈值（世界单位）

    /// <summary>
    /// 优先目标类型,然后优先距离最近,可以选择单位或建筑
    /// 不需要BaseUnit的属性,所以使用IHasPosition接口
    /// 输入的单位要求可见和可攻击,该方法不检查目标是否可见和可攻击
    /// </summary>
    public IHasPosition EvaluateUnitAndBuilding(List<IHasPosition> targets, TargetType PriorityTargetType,IHasPosition unit)
    {
        IHasPosition bestTarget = null;
        List<IHasPosition> Units = new List<IHasPosition>();
        List<IHasPosition> Buildings = new List<IHasPosition>();
        foreach(var target in targets)
        {
            if(target is BaseUnit)
                Units.Add(target);
            else if(target is BaseBuilding)
                Buildings.Add(target);
        }
        switch (PriorityTargetType)
        {
            case TargetType.Unit:
                bestTarget = GetBestTarget(Units, unit);
                break;
            case TargetType.Building:
                bestTarget = GetBestTarget(Buildings, unit);
                break;
            default:
                return null;
        }
        
        if (bestTarget == null)
        {
            switch (PriorityTargetType)
            {
                case TargetType.Unit:
                    bestTarget = GetBestTarget(Buildings, unit);
                    break;
                case TargetType.Building:
                    bestTarget = GetBestTarget(Units, unit);
                    break;
                default:
                    return null;
            }
        }
        // 没有当前目标，直接返回最佳目标
        if (bestTarget == null)
            return null;
        return bestTarget;
    }
    /// <summary>
    /// 只评估单位目标,不评估建筑目标
    /// </summary>
    /// <param name="units"></param>
    /// <param name="currentUnit"></param>
    /// <returns></returns>
    public IHasPosition EvaluateUnits(List<IHasPosition> units, IHasPosition unit)
    {
        if(units==null || !units.Any())
            return null;
        return GetBestTarget(units,unit);
    }
    /// <summary>
    /// 只评估建筑目标,不评估单位目标
    /// </summary>
    /// <param name="Buildings"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public IHasPosition EvaluateBuildings(List<IHasPosition> Buildings, IHasPosition unit)
    {
        if(Buildings==null || !Buildings.Any())
            return null;
        return GetBestTarget(Buildings,unit);
    }
    /// <summary>
    /// 输入的前提是目标比当前目标近,没有这个前提则没有判断的意义.
    /// 明确输入不为null,且存活
    /// </summary>
    /// <param name="target"></param>
    /// <param name="currentTarget"></param>
    /// <param name="switchCost"></param>
    /// <returns></returns>
    public bool EvaluateChangeable(IHasPosition target,IHasPosition currentTarget,float switchCost)
    {
        if (Vector2.Distance(target.GridPos, currentTarget.GridPos) > minDistanceDifference)
        {
            return true;
        }
        return false;
    }
    public IHasPosition EvaluateHelpUnit(List<BaseUnit> targets, IHasPosition currentUnit)
    {
        //这是一个打分评估,基于距离和缺失血量两项,距离近,缺失血量多的优先选择
        IHasPosition bestTarget = null;//维持一个最佳目标,分数最高的目标
        float bestTargetScore = float.MinValue;//维持一个最佳目标的分数,初始值为最小值
        //考虑到切换目标的损耗,距离的权重要大于缺失血量
        float distanceWeight = 1f;
        float missingWeight = 0.5f;
        //然后是归一化的处理,希望的距离范围是0-50之间,血量的范围是0-50之间,大于的按上限处理.

        foreach (var target in targets)
        {
            float distance= Vector2.Distance(target.GridPos, currentUnit.GridPos);
            if (distance > 50)
            {
                distance = 50;
            }
            distance = distance / 50;
            float missingHealth = target.attributes.MaxHealth - target.attributes.CurrentHealth;
            if (missingHealth > 50)
            {
                missingHealth = 50;
            }
            missingHealth = missingHealth / 50;
            //注意是距离近分数高.
            float score = distanceWeight * (1-distance) + missingWeight * missingHealth;
            if (bestTarget == null || score > bestTargetScore)
            {
                bestTarget = target;
                bestTargetScore = score;
            }
        }
        return bestTarget;
    }
    /// <summary>
    /// 获取最佳目标：优先选择最近的单位，
    /// </summary>
    private IHasPosition GetBestTarget(IEnumerable<IHasPosition> targets, IHasPosition unit)
    {
        if(targets==null || !targets.Any())
            return null;
        // 过滤空引用和已销毁对象
        var validTargets = targets.Where(t => IsAlive(t)).ToList();

        if (!validTargets.Any())
            return null;

        return validTargets.OrderBy(u => Vector2.Distance(unit.GridPos, u.GridPos)).First();
    }


    // 辅助方法：安全判断接口对象是否存活（Unity对象未被销毁）
    private bool IsAlive(IHasPosition obj)
    {
        return (obj as MonoBehaviour) != null;//嗯，这两好像没区别吧，那突然待机的bug是怎么回事呢？
        //return obj is MonoBehaviour mb && mb != null;
    }
    // 安全比较两个接口是否指向同一个存活对象
    private bool AreSameTarget(IHasPosition a, IHasPosition b)
    {
        if (!IsAlive(a) || !IsAlive(b)) return false;
        return a == b; // 此时两者都存活，可以用引用比较
    }
}