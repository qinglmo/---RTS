
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 格子范围形状。
/// </summary>
public enum GridRangeShape
{
    Circle,
    Cross
}

/// <summary>
/// 用于获取指定中心点和范围内的格子集合,不包括中心点.
/// 只负责几何范围计算，不处理地图边界、障碍物、占用等问题。
/// </summary>
public static class GridRange
{
    /// <summary>
    /// 获取指定范围内的格子集合,不包括中心点.
    /// </summary>
    /// <param name="center">中心格子</param>
    /// <param name="range">范围，必须 >= 0</param>
    /// <param name="shape">范围形状</param>
    /// <returns>符合范围的格子集合</returns>
    public static List<Vector2Int> Get(
        Vector2Int center,
        int range,
        GridRangeShape shape)
    {
        range = Mathf.Max(0, range);

        switch (shape)
        {
            case GridRangeShape.Circle:
                return GetCircle(center, range);

            case GridRangeShape.Cross:
                return GetCross(center, range);

            default:
                return new List<Vector2Int>();
        }
    }

    /// <summary>
    /// 获取圆形范围。
    /// 使用欧氏距离判断。
    /// </summary>
    private static List<Vector2Int> GetCircle(
        Vector2Int center,
        int range)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        int rangeSqr = range * range;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // 欧氏距离平方
                if (x * x + y * y <= rangeSqr)
                {
                    result.Add(new Vector2Int(
                        center.x + x,
                        center.y + y
                    ));
                }
            }
        }
        // 移除中心点
        result.Remove(center);
        return result;
    }

    /// <summary>
    /// 获取十字形范围。
    /// 包含中心点，以及中心点上下左右 range 格。
    /// </summary>
    private static List<Vector2Int> GetCross(
        Vector2Int center,
        int range)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        // 中心点
        result.Add(center);

        for (int i = 1; i <= range; i++)
        {
            result.Add(center + new Vector2Int(i, 0));
            result.Add(center + new Vector2Int(-i, 0));
            result.Add(center + new Vector2Int(0, i));
            result.Add(center + new Vector2Int(0, -i));
        }
        // 移除中心点
        result.Remove(center);
        return result;
    }

    /// <summary>
    /// 从一个格子集合中排除另一个格子集合。
    /// 返回 A 中存在、但 B 中不存在的格子。
    /// </summary>
    public static List<Vector2Int> Subtract(
        List<Vector2Int> a,
        List<Vector2Int> b)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int pos in a)
        {
            if (!b.Contains(pos))
            {
                result.Add(pos);
            }
        }

        return result;
    }
}



