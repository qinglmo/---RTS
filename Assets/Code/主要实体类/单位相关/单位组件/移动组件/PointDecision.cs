using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class PointDecision
{
    /// <summary>
    /// 寻找目标格子周围10格范围内的空闲格子,用于修复占领,避免违法重叠
    /// </summary>
    public static Vector2Int? FindAlternativeOccupyCell(Vector2Int start,BaseUnit unit)
    {
        int maxSearchDist = 10; // 最大搜索步数（可根据需要调整）

        // 1. 从起点开始 BFS，标记所有在 maxSearchDist 步内可通行的格子
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();

        if (MovementRules.IsWalkable(start, unit)) // 起点自身必须可行走
        {
            queue.Enqueue(start);
            reachable.Add(start);
            distance[start] = 0;
        }

        // 四个移动方向（假设使用 Vector2Int 表示）
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            // 检查当前格子是否可占领（不需要排除起点，如果起点可以占领，那么刚好完成任务）
            if (MovementRules.IsOccupyable(current,unit))
            {
                return current; // BFS 保证第一个找到的是最近的
            }
            int currentDist = distance[current];
            if (currentDist >= maxSearchDist) continue; // 超过步数限制不再扩展
            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!MovementRules.IsWalkable(neighbor, unit)) continue;          // 不可行走（障碍物等）
                if (reachable.Contains(neighbor)) continue;   // 已访问

                reachable.Add(neighbor);
                distance[neighbor] = currentDist + 1;
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }
    /// <summary>
    /// 这个版本起点可以不用可行走
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public static Vector2Int? FindAlternativeOccupyCell2(Vector2Int start, BaseUnit unit)
    {
        int maxSearchDist = 10; // 最大搜索步数（可根据需要调整）

        // 1. 从起点开始 BFS，标记所有在 maxSearchDist 步内可通行的格子
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        reachable.Add(start);
        distance[start] = 0;

        // 四个移动方向（假设使用 Vector2Int 表示）
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            // 检查当前格子是否可占领（排除起点自身，因为起点通常已被单位占据）
            if (MovementRules.IsPreOccupyable(current, unit) && current != start)
            {
                return current; // BFS 保证第一个找到的是最近的
            }
            int currentDist = distance[current];
            if (currentDist >= maxSearchDist) continue; // 超过步数限制不再扩展
            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!MovementRules.IsWalkable(neighbor, unit)) continue;          // 不可行走（障碍物等）
                if (reachable.Contains(neighbor)) continue;   // 已访问

                reachable.Add(neighbor);
                distance[neighbor] = currentDist + 1;
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }
    /// <summary>
    /// 使用BFS寻找到达可以攻击敌方的最近的所有位置
    /// 适合连通目标，返回的目标格子应该和单位在同一个连通区域。
    /// 该方法的使用场景，是目标被层层包围了，如果被隔空包围了，可能效果会不及预期。
    /// </summary>
    public static HashSet<Vector2Int> FindClosestValidRing(
    Vector2Int goal,
    BaseUnit unit,
    Func<Vector2Int, BaseUnit, bool> isWalkable,
    Func<Vector2Int, BaseUnit, bool> IsPreOccupyable,
    int maxRange = 5)
    {
        var result = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(goal);
        visited.Add(goal);

        while (queue.Count > 0)
        {
            int count = queue.Count;
            bool foundValidInRing = false;

            // 一次处理一整圈
            for (int i = 0; i < count; i++)
            {
                Vector2Int current = queue.Dequeue();

                // 只要可预占，就加入目标集（包括 goal 本身）
                if (IsPreOccupyable(current, unit))
                {
                    result.Add(current);
                    foundValidInRing = true;
                }

                // 这圈还没找到有效点，才继续扩散下一圈
                if (!foundValidInRing)
                {
                    foreach (var neighbor in GetNeighbors(current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            // 只要这一圈找到任意可站位置 → 直接结束！
            if (foundValidInRing)
                break;
        }

        return result;
    }
    private static List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(cell.x + 1, cell.y),
            new Vector2Int(cell.x - 1, cell.y),
            new Vector2Int(cell.x, cell.y + 1),
            new Vector2Int(cell.x, cell.y - 1)
        };
    }

}
