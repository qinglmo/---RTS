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
    /// 使用BFS寻找到达可以攻击敌方的最优位置
    /// （搜索直到找到既可达又能攻击到敌方单位的位置）
    /// </summary>
    public static Vector2Int? FindOptimalAttackPosition(Vector2Int enemyGridPos, int desiredAttackRange)
    {
        return null;
    }
}
