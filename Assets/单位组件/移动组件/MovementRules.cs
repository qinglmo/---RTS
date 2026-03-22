using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MovementRules
{
    private static GridManager gridManager;
    private static GridManager GridManager
    {
        get
        { 
            if(gridManager==null)
                gridManager = GridManager.Instance;
            return gridManager;
        }
    }
    /// <summary>
    /// 判断指定格子是否可通行
    /// </summary>
    /// <param name="cell">目标格子坐标</param>
    /// <param name="self">请求判断的单位（可选，用于处理自身占用的特殊情况）</param>
    /// <returns>true 表示可通行，false 表示不可通行</returns>
    public static bool IsWalkable(Vector2Int cell, BaseUnit self = null)
    {
        // 1. 边界检查
        if (!GridManager.BoundsChecking(cell))
            return false;
        // 4. 实体占用判断，包括单位和建筑,自然地形
        if (GridManager.IsOccupied(cell))
        {
            //如果被友军占领，依然视为可通行
            if(self != null&& GridManager.GetUnit(cell)?.Faction==self.Faction)
                return true;
            // 否则被其他单位占用 → 不可通行
            return false;
        }
        // 5. 无任何阻挡 → 可通行
        return true;
    }
    public static bool IsOccupyable(Vector2Int cell,BaseUnit self = null)
    {
        // 1. 边界检查
        if (!GridManager.BoundsChecking(cell))
            return false;
        // 4. 单位占用判断
        if (GridManager.IsOccupied(cell))
        {
            //自身占领认为可占领
            if (self != null && GridManager.GetUnit(cell) == self)
                return true;
            return false;
        }
        // 5. 无任何阻挡 → 可占领
        return true;
    }
    /// <summary>
    /// 用于寻路,
    /// 实现了细节：自己的预占领认为可预占领
    /// 自己的占领认为可占领
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    public static bool IsPreOccupyable(Vector2Int cell, BaseUnit self)//是否可以预占领，是否可以抵达。
    {
        if (!IsOccupyable(cell,self))
        {
            return false ;
        }
        if (IsFactionOccupy(cell,self))
        {
            return false;
        }
        return true;

    }
    /// <summary>
    /// FactionOccupyCells确保初始化正确
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    private static bool IsFactionOccupy(Vector2Int cell, BaseUnit self)//被占领了返回true,没被占领返回false
    {
        if (FactionManager.FactionOccupyCells[self.Faction].ContainsKey(cell))
        {
            if (FactionManager.FactionOccupyCells[self.Faction][cell]!=self)//这是一个坑啊，自己的预占领应该忽视
                return true;
            else 
                return false;
        }
        else
        {
            return false;
        }
    }
}
