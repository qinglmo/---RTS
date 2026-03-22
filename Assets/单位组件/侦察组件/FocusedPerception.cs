using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TargetDetector;
using static UnityEngine.GraphicsBuffer;

public interface IFocusTarget
{
    public bool IsTargetInRange(IFactionMember target, int range, RangeMode RangeMode,Vector2Int currPos);//现在统一攻击范围和追击范围，根据需要配置
}
/// <summary>
/// 提供更高性能的检查服务。
/// </summary>
public class FocusedPerception: MonoBehaviour,IFocusTarget
{

    /// <summary>
    /// 判断目标是否在范围内。
    /// 圆形模式：直接欧几里得距离。
    /// 直线模式：先检查是否在同一行/列，再计算格子曼哈顿距离 ≤ 转换后的格子阈值。
    /// </summary>
    public bool IsTargetInRange(IFactionMember target, int range, RangeMode RangeMode, Vector2Int currentGrid)
    {
        if ((target as MonoBehaviour) == null) return false;
        switch (RangeMode)
        {
            case RangeMode.Circle:
                return Vector2.Distance(currentGrid, target.GridPos) <= range;
            case RangeMode.Line:
                // 将世界范围 range 转换为最大允许的格子数（向上取整）
                return IsTargetInAttackRangeGrid(target.GridPos, currentGrid, range);
            default:
                return false;
        }
    }

    /// <summary>
    /// 基于网格的直线方向判定：
    /// </summary>
    private bool IsTargetInAttackRangeGrid(Vector2Int targetGrid, Vector2Int currentGrid,int range)
    {
        if(targetGrid.x == currentGrid.x || targetGrid.y == currentGrid.y)
        {
            int gridDistance = Mathf.Abs(targetGrid.x - currentGrid.x) + Mathf.Abs(targetGrid.y - currentGrid.y);
            return gridDistance <= range;
        }
        return false;
    }
}
