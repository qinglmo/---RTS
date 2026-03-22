using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public interface IMoveApplyWorld
{
    public bool IsWalkable(BaseUnit unit, Vector2Int target);
    /// <summary>
    /// 单位尝试入驻，自动管理占用关系，返回true表示单位可以继续移动，返回false表示单位应该回撤
    /// 只管理移动时候的占用，单位的激活隐藏初始化死亡需要自己管理。
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryOccupy(BaseUnit unit, Vector2Int start, Vector2Int target);
    public void FristOccupy(BaseUnit unit, Vector2Int grid);
    public void LastRelease(BaseUnit unit, Vector2Int grid);
    public bool IsOccupy(BaseUnit unit);
}

/// <summary>
/// 管理世界所有单位在网格中的占用，避免占用冲突
/// 实现细节：检查所有非占用状态的单位，如果其在同一个格子时长超过1s，将其强制推开，这属于简化版的物理层。
/// </summary>
public class GridWorldService : MonoBehaviour, IMoveApplyWorld
{
    public static GridWorldService Instance;
    private IOccupyGridManager gridOccupancy;
    private IGridEntityProvider gridEntityProvider;
    private IGridPassableCheck gridPassableCheck;
    //维护一个未占用单位列表，在单位申请入驻但是有友军时尝试捕获，也在单位申请入驻成功时删除已捕获对象。设计一个定时器，每s检查所有单位，先检查存活，再检查位置是否更新。
    //如果未更新，则尝试在周围寻找空位，将其强制弹开，无论多远。
    //注意，推开的时候可以检查自身位置，有时候自身位置也可能为空但是却被标记了未占用。
    //关于重叠单位，还可以先查询网格的占用单位，如果不是自己，那就是重叠单位。
    //注意一些细节，当单位在移动时不要弹开，有时候如果单位刚走到友军位置就触发定时器的话，如果这个时候没有移动，也可能弹开。(概率小，可以先不管)
    private HashSet<BaseUnit> noOccupyUnits=new HashSet<BaseUnit>();
    //不知道为什么，依然不太稳定，也不知道什么原因，就是出现了两个单位一起占用，所以我决定再加一层检查
    //只要该单位处于占用状态但是在网格上没有找到对应值，就认为出bug，将其强制弹开。
    private HashSet<BaseUnit> allUnit=new HashSet<BaseUnit>();



    private float timer = 0;
    private float inspectionInterval = 0.2f;
    private void Initialize(IOccupyGridManager gridOccupancy, IGridEntityProvider gridEntityProvider, IGridPassableCheck gridPassableCheck)
    {
        this.gridOccupancy =   gridOccupancy;
        this.gridEntityProvider = gridEntityProvider;
        this.gridPassableCheck = gridPassableCheck;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        Initialize(GridManager.Instance, GridManager.Instance, GridManager.Instance);
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer>=inspectionInterval)
        {
            timer = 0;
            UpdatePhysicalEasy();
            UpdatePhysicalEasyAll();
        }
    }
    public bool IsOccupy(BaseUnit unit)
    {
        return !noOccupyUnits.Contains(unit);
    }
    public void FristOccupy(BaseUnit unit, Vector2Int grid)
    {
        if (!gridOccupancy.TryOccupy(grid, unit))
        {
            noOccupyUnits.Add(unit);//
        }
        allUnit.Add(unit);
    }
    public void LastRelease(BaseUnit unit, Vector2Int grid)
    {
        gridOccupancy.Release(grid, unit);
        if (noOccupyUnits.Contains(unit))//释放单位
        {
            noOccupyUnits.Remove(unit);
        }
        if(allUnit.Contains(unit))
            allUnit.Remove(unit);
    }
    //单位申请该位置是否可移动
    public bool IsWalkable(BaseUnit unit,Vector2Int target)
    {
        //位置有友军或者为空，则可通行
        if (!gridPassableCheck.IsOccupied(target))
            return true;
        var targetunit = gridEntityProvider.GetUnit(target);
        if (targetunit!=null && targetunit.Faction==unit.Faction)
            return true;
        //位置有非友军，不可通行实体则不可通行。
        return false;
    }
    //单位尝试入驻，自动管理占用关系，返回true表示单位可以继续移动，返回false表示单位应该回撤
    //注意先释放，再占领，考虑到起始格子会等于终点格子的情况
    public bool TryOccupy(BaseUnit unit,Vector2Int start ,Vector2Int target)
    {
        //位置有友军或者为空，则可通行
        if (!gridPassableCheck.IsOccupied(target))
        {
            gridOccupancy.Release(start, unit);
            unit.movement.stepMover.OnOccupyChangedTrriger(true);
            gridOccupancy.TryOccupy(target, unit);
            if (noOccupyUnits.Contains(unit))//释放单位
            {
                noOccupyUnits.Remove(unit);
            }
            
            return true; 
        }
        var targetunit = gridEntityProvider.GetUnit(target);
        if (targetunit != null && targetunit.Faction == unit.Faction)
        {
            unit.movement.stepMover.OnOccupyChangedTrriger(false);
            noOccupyUnits.Add(unit);
            gridOccupancy.Release(start, unit);
            return true;
        }
        //位置有非友军，不可通行实体则不可通行。
        return false;
    }
    private void UpdatePhysicalEasy()
    {
        List<BaseUnit> toRemove = new List<BaseUnit>();
        List<BaseUnit> toPush = new List<BaseUnit>();
        foreach(var unit in noOccupyUnits)
        {
            //移除已经失效单位
            if(unit == null||unit.gameObject.activeInHierarchy==false)
            {
                toRemove.Add(unit);
                continue;
            }
            //目前的逻辑很简单，只要被捕获了同时又检测到没有移动，就强制弹开。
            if (unit.movement.stepMover.direction == Vector2Int.zero)
            {
                toPush.Add(unit);
                continue;
            }
        }
        foreach(var unit in toRemove)
        {
            noOccupyUnits.Remove(unit);
        }
        foreach(var unit in toPush)
        {
            noOccupyUnits.Remove(unit);
            //执行弹开逻辑
            var target = PointDecision.FindAlternativeOccupyCell(unit.GridPos, unit);
            if (target!=null)
            {
                unit.movement.stepMover.PushTo((Vector2Int)target);
            }
        }
    }
    private void UpdatePhysicalEasyAll()
    {
        List<BaseUnit> toPush = new List<BaseUnit>();
        foreach (var unit in allUnit)
        {
            if (noOccupyUnits.Contains(unit)||unit.movement.stepMover.direction!=Vector2.zero
                ||unit.movement.stepMover.stateMachine.CurrentStateEnum==StepMover.MainState.Pushing
                || unit.movement.stepMover.stateMachine.CurrentStateEnum == StepMover.MainState.Reverting)
                continue;
            if (gridEntityProvider.GetUnit(unit.GridPos)!= unit)
            {
                toPush.Add(unit);
                Debug.LogWarning("发现异常单位" + unit.name);
            }
        }
        foreach (var unit in toPush)
        {
            noOccupyUnits.Remove(unit);
            //执行弹开逻辑
            var target = PointDecision.FindAlternativeOccupyCell(unit.GridPos, unit);
            if (target != null)
            {
                unit.movement.stepMover.PushTo((Vector2Int)target);
            }
        }
    }
}
