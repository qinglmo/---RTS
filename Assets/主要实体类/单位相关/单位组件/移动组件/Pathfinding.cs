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
    public static List<Vector2Int> FindPathToAdjacentAStar( Vector2Int goal,
        BaseUnit unit, System.Func<Vector2Int, BaseUnit, bool> isWalkable, System.Func<Vector2Int, BaseUnit, bool> IsPreOccupyable)
    {
        var start=unit.GridPos;
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
    /// 返回从 start 到 goal 相邻格子的最短路径（包含起点，不包含 goal），若无法到达任何相邻格子，
    /// 则返回从起点到距离 goal 最近且代价最低的可达格子的路径（包含起点和该格子）
    /// </summary>
    public static List<Vector2Int> FindPathToAdjacentAStar2(
        Vector2Int goal,
        BaseUnit unit,
        System.Func<Vector2Int, BaseUnit, bool> isWalkable,
        System.Func<Vector2Int, BaseUnit, bool> IsPreOccupyable)
    {
        var start = unit.GridPos;

        if (!isWalkable(start, unit))
            return null;
        if (start == goal)
            return null;

        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        // 用于记录 fallback 的最佳节点（距离 goal 最近且代价最低的可预占领节点）
        Vector2Int? bestFallbackNode = null;

        // 评估并更新最佳 fallback 节点（仅当节点可预占领时考虑）
        void UpdateBestFallback(Vector2Int node)
        {
            // 只考虑可预占领的节点
            if (!IsPreOccupyable(node, unit))
                return;

            if (bestFallbackNode == null)
            {
                bestFallbackNode = node;
                return;
            }

            float curDist = Heuristic(node, goal);
            float bestDist = Heuristic(bestFallbackNode.Value, goal);
            float curG = gScore.GetValueOrDefault(node, float.MaxValue);
            float bestG = gScore.GetValueOrDefault(bestFallbackNode.Value, float.MaxValue);

            // 优先选择距离更近的，距离相同时选择代价更低的
            if (curDist < bestDist || (Math.Abs(curDist - bestDist) < 0.001f && curG < bestG))
            {
                bestFallbackNode = node;
            }
        }

        // 起点作为候选（起点应该可预占领，因为单位已站在上面）
        UpdateBestFallback(start);

        // 方向偏好
        if (unit.movement.stepMover.direction != Vector2Int.zero)
        {
            Vector2Int target = start + unit.movement.stepMover.direction;
            if (Math.Abs(target.x - start.x) + Math.Abs(target.y - start.y) == 1 &&
                isWalkable(target, unit))
            {
                float lowCost = 0.5f;
                if (!gScore.ContainsKey(target) || lowCost < gScore[target])
                {
                    cameFrom[target] = start;
                    gScore[target] = lowCost;
                    fScore[target] = lowCost + Heuristic(target, goal);
                    openSet.Add(target);
                    UpdateBestFallback(target);
                }
            }
        }

        while (openSet.Count > 0)
        {
            // 取出 f 最小的节点
            Vector2Int current = openSet.OrderBy(node => fScore.GetValueOrDefault(node, float.MaxValue)).First();

            // 如果当前节点是 goal 的邻居且可预占领，直接返回路径
            if (Math.Abs(current.x - goal.x) + Math.Abs(current.y - goal.y) == 1 &&
                IsPreOccupyable(current, unit))
            {
                return ReconstructPath(cameFrom, current);
            }

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
                    {
                        openSet.Add(neighbor);
                        UpdateBestFallback(neighbor);
                    }
                }
            }
        }

        // 如果没有找到 goal 的邻居，则使用最佳 fallback 节点（已确保可预占领）
        if (bestFallbackNode.HasValue)
        {
            return ReconstructPath(cameFrom, bestFallbackNode.Value);
        }

        return null; // 没有任何可预占领的可达节点
    }
    public static List<Vector2Int> FindPathMultiTarget(
        Vector2Int start,                // 起点
        HashSet<Vector2Int> targetSet,   // 目标点集合（你提前选好的合法停靠点）
        Vector2Int goal,                 // 目标点（你最终要到达的位置）
        BaseUnit unit,
        System.Func<Vector2Int, BaseUnit, bool> isWalkable,
        bool useDirectionBonus = true)   // 保留你原来的方向偏好
    {
        // 0. 安全判断
        if (targetSet == null || targetSet.Count == 0)
            return null;
        if (!isWalkable(start, unit))
            return null;
        if (targetSet.Contains(start))
            return new List<Vector2Int> { start };

        // ----------------------
        // A* 标准结构（完全纯净）
        // ----------------------
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float>();

        // 启发函数：用目标集合里最近的点计算（多目标A*标准做法）
        //float HeuristicToGoal(Vector2Int pos)
        //{
        //    float minDist = float.MaxValue;
        //    foreach (var target in targetSet)
        //    {
        //        float dist = Heuristic(pos, target);
        //        if (dist < minDist) minDist = dist;
        //    }
        //    return minDist;
        //}
        // 启发式：到 goal 的曼哈顿距离 × 最小步长（可采纳）
        float HeuristicToGoal(Vector2Int pos)
        {
            return 1 * (Math.Abs(pos.x - goal.x) + Math.Abs(pos.y - goal.y));
        }

        fScore[start] = HeuristicToGoal(start);

        // ----------------------
        // 保留你原有的【方向偏好】（你原来的逻辑不动）
        // ----------------------
        if (useDirectionBonus && unit.movement.stepMover.direction != Vector2Int.zero)
        {
            Vector2Int preferredDir = start + unit.movement.stepMover.direction;
            if (IsNeighbor(start, preferredDir) && isWalkable(preferredDir, unit))
            {
                float cost = 0.5f;
                if (!gScore.ContainsKey(preferredDir) || cost < gScore[preferredDir])
                {
                    cameFrom[preferredDir] = start;
                    gScore[preferredDir] = cost;
                    fScore[preferredDir] = cost + HeuristicToGoal(preferredDir);
                    openSet.Add(preferredDir);
                }
            }
        }

        // ----------------------
        // A* 主循环（纯净！无任何业务污染）
        // ----------------------
        while (openSet.Count > 0)
        {
            // 取出 f 值最小的节点（你原来的逻辑，保持兼容）
            Vector2Int current = openSet.OrderBy(node => fScore.GetValueOrDefault(node, float.MaxValue)).First();

            // ======================
            // 多目标终点判断：一句话搞定！
            // ======================
            if (targetSet.Contains(current))
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            // 遍历邻居（你原函数不动）
            foreach (var neighbor in GetNeighbors(current))
            {
                if (!isWalkable(neighbor, unit))
                    continue;

                float tentativeG = gScore[current] + 1;
                float neighborG = gScore.GetValueOrDefault(neighbor, float.MaxValue);

                if (tentativeG < neighborG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + HeuristicToGoal(neighbor);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // 没有可达路径
        return null;
    }
    /// <summary>
    /// 直接用目标四邻域作为终点，不考虑终点是否可占用。
    /// 如果四邻域都不可通行，会返回null
    /// 可用于静态寻路，传递对应的静态可通行判断方法即可。静态前提下，如果和目标连通，一定有路径。
    /// 建议长路径，可连通，静态寻路，三者都满足的情景下使用。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="targetSet"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public static List<Vector2Int> FindPath_OneTarget(Vector2Int goal,
        BaseUnit unit,
        System.Func<Vector2Int, BaseUnit, bool> isWalkable)
    {
        HashSet<Vector2Int> targetSet = new HashSet<Vector2Int>();
        foreach(var neighbor in GetNeighbors(goal))
        {
            targetSet.Add(neighbor);
        }
        return FindPathMultiTarget(unit.GridPos, targetSet, goal,unit,isWalkable);
    }
    // ---------- 辅助方法 ----------
    private static bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) == 1;
    }
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
