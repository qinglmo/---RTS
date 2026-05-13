using UnityEngine;
using System.Collections.Generic;

public static class SpaceGet
{
    public static List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(cell.x + 1, cell.y),
            new Vector2Int(cell.x - 1, cell.y),
            new Vector2Int(cell.x, cell.y + 1),
            new Vector2Int(cell.x, cell.y - 1)
        };
    }
    public static IEnumerable<Vector2Int>  GetCellsInCircle(Vector2Int center, float radius)
    {
        // 计算需要检查的格子范围（基于半径 / 格子大小，向上取整）
        int cellRadius = (int)radius;

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {

                Vector2Int cell = new Vector2Int(center.x + x, center.y + y);
                if (cell == center)
                {
                    continue;//排除自身
                }
                // 获取格子中心的世界坐标
                Vector2 cellCenter = GridManager.Instance.CellToWorld(cell);
                // 如果格子中心在圆内，则返回该格子
                if (Vector2.Distance(cellCenter, center) <= radius)
                {
                    yield return cell;

                }
            }
        }
    }
}