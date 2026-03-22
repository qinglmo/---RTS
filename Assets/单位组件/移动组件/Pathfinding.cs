using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 寻路服务类：提供 A* 寻路、BFS 搜索等静态方法
/// </summary>
public static class Pathfinding
{
    /// <summary>
    /// A* 寻路，返回从 start 到 goal 的路径（包含起点和终点），若无法到达则返回 null
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="goal">终点</param>
    /// <param name="isWalkable">判断格子是否可行走的委托（需自行处理边界）</param>
    public static List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal,
        BaseUnit unit, System.Func<Vector2Int,BaseUnit, bool> isWalkable)
    {
        if (!isWalkable(start, unit) || !isWalkable(goal, unit))
            return null;

        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        // 如果当前移动目标与起点相邻且可行走，则将其以低代价加入openSet（方向偏好）
        if (unit.movement.stepMover.direction != Vector2Int.zero)
        {
            Vector2Int target = start + unit.movement.stepMover.direction;
            if (Math.Abs(target.x - start.x) + Math.Abs(target.y - start.y) == 1 && isWalkable(target, unit))
            {
                // 给予比正常移动（代价1）更低的代价，例如0.5，以优先考虑该方向
                float lowCost = 0.5f;
                if (!gScore.ContainsKey(target) || lowCost < gScore[target])
                {
                    cameFrom[target] = start;
                    gScore[target] = lowCost;
                    fScore[target] = lowCost + Heuristic(target, goal);
                    openSet.Add(target);
                }
            }
        }
        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.OrderBy(node => fScore.GetValueOrDefault(node, float.MaxValue)).First();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!isWalkable(neighbor, unit))
                    continue;

                float tentativeG = gScore[current] + 1;
                if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 返回从 start 到 goal 相邻格子的最短路径（包含起点，不包含 goal），若无法到达任何相邻格子则返回 null
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="goal">目标点（通常被占领，不要求可行走）</param>
    /// <param name="unit">移动类型（用于行走性判断）</param>
    /// <param name="isWalkable">判断格子是否可行走的委托（需自行处理边界）</param>
    public static List<Vector2Int> FindPathToAdjacentAStar(Vector2Int start, Vector2Int goal,
        BaseUnit unit, System.Func<Vector2Int, BaseUnit, bool> isWalkable, System.Func<Vector2Int, BaseUnit, bool> IsPreOccupyable)
    {
        // 起点必须可行走
        if (!isWalkable(start, unit))
            return null;

        // 起点与 goal 重合时无法找到“前一步”
        if (start == goal)
            return null;

        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        // 如果当前移动目标与起点相邻且可行走，则将其以低代价加入openSet（方向偏好）
        if (unit.movement.stepMover.direction!=Vector2Int.zero)
        {
            Vector2Int target = start+ unit.movement.stepMover.direction;
            if (Math.Abs(target.x - start.x) + Math.Abs(target.y - start.y) == 1 && isWalkable(target, unit))
            {
                // 给予比正常移动（代价1）更低的代价，例如0.5，以优先考虑该方向
                float lowCost = 0.5f;
                if (!gScore.ContainsKey(target) || lowCost < gScore[target])
                {
                    cameFrom[target] = start;
                    gScore[target] = lowCost;
                    fScore[target] = lowCost + Heuristic(target, goal);
                    openSet.Add(target);
                }
            }
        }
        while (openSet.Count > 0)
        {
            // 取出当前 f 值最小的节点
            Vector2Int current = openSet.OrderBy(node => fScore.GetValueOrDefault(node, float.MaxValue)).First();

            // 如果当前节点是 goal 的邻居，则找到路径（最优）
            if (Math.Abs(current.x - goal.x) + Math.Abs(current.y - goal.y) == 1)
            {
                if(IsPreOccupyable(current,unit))//且终点必须可预占领。
                    return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                // 只考虑可行走的邻居（goal 本身即使不可走也不会被加入）
                if (!isWalkable(neighbor, unit))
                    continue;

                float tentativeG = gScore[current] + 1;
                if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // 无可达的相邻格子
    }
    /// <summary>
    /// BFS 搜索所有可达位置，返回从起点出发能到达的所有格子列表（包含起点）
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="isWalkable">可通行判断委托</param>
    /// <param name="maxSteps">最大搜索步数（曼哈顿距离限制，小于等于0表示不限制）</param>
    public static HashSet<Vector2Int> FindReachablePositionsBFS(Vector2Int start, System.Func<Vector2Int, bool> isWalkable, int maxSteps = -1)
    {
        var visited = new HashSet<Vector2Int> { start };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!isWalkable(neighbor))
                    continue;

                if (visited.Contains(neighbor))
                    continue;

                if (maxSteps > 0 && Heuristic(start, neighbor) > maxSteps)
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
        return visited;
    }

    /// <summary>
    /// 寻找从起点出发，满足条件 predicate 的最近格子（曼哈顿距离最短）
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="isWalkable">可通行判断委托</param>
    /// <param name="predicate">条件判断函数</param>
    /// <returns>满足条件的最近格子，若不存在则返回 null</returns>
    public static Vector2Int? FindClosestPositionSatisfying(Vector2Int start, System.Func<Vector2Int, bool> isWalkable, System.Func<Vector2Int, bool> predicate)
    {
        var visited = new HashSet<Vector2Int> { start };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // 检查当前格子是否满足条件（起点也可能满足）
            if (predicate(current))
                return current;

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!isWalkable(neighbor))
                    continue;

                if (visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
        return null;
    }

    // ---------- 辅助方法 ----------
    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}
