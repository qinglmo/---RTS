using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 决策系统：根据当前状态和目标，选择合适的决策（IntentType）。
/// 决策和信息收集不分开,在决策系统中同时处理。
/// </summary>
public class IntentionDecision : MonoBehaviour
{
    public enum IntentType
    {
        Attack,
        Help
    }
    public IntentType currentIntentType = IntentType.Attack;

    public enum IntentStatus
    {
        Action,
        Chase,
        Find,
        Idle
    }
    public IntentStatus currentIntentStatus = IntentStatus.Idle;

    #region 序列化字段
    [Header("检测设置")]
    [SerializeField] private int chaseRange = 20; //追击范围
    [SerializeField] private int actionRange = 1; //动作范围
    [Header("运行时数据")]
    private IHasPosition currentTarget;// 当前目标

    [Header("检测频率")]
    [SerializeField] private float decisionInterval = 0.5f;   // 决策间隔
    [SerializeField] private float checkInterval = 0.1f;  // 检查间隔
    private float decisionTimer;
    private float checkTimer;

    [Header("切换阈值")]
    [SerializeField] private float minDistanceDifference = 1f;   // 切换目标所需的最小距离差距
    #endregion
    #region 公开属性
    public IHasPosition CurrentTarget
    {
        get { return currentTarget; }
        set 
        { 
            if(currentTarget!=value)
            {
                currentTarget = value;
                OnTargetChanged?.Invoke(value);
            }
        }
    }                // 只读，供外部获取攻击目标
    #endregion
    
    // 在 TargetDetector 类中添加
    private IFactionMember unit;
    public event Action<IHasPosition> OnTargetChanged;

    private ITargetProvider targetProvider;
    private ITargetFiltering targetFiltering;
    private ITargetEvaluation targetEvaluation;
    
    public DecisionExecution decisionExecution;
    private int mask;
    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        targetProvider = GetComponent<ITargetProvider>() ?? gameObject.AddComponent<BackgroundScan>();
        targetFiltering = GetComponent<ITargetFiltering>() ?? gameObject.AddComponent<ViewFiltering>();
        targetEvaluation = GetComponent<ITargetEvaluation>() ?? gameObject.AddComponent<BrainDecision>();
        mask = LayerMask.GetMask("Mountain", "Building");
        decisionExecution = GetComponent<DecisionExecution>() ?? gameObject.AddComponent<DecisionExecution>();
        decisionExecution.Init(unit as BaseUnit);
    }

    private void Update()
    {
        decisionTimer += Time.deltaTime;
        checkTimer += Time.deltaTime;
        //检查间隔
        if(checkTimer>=checkInterval)//快速检查当前目标,修正异常
        {
            checkTimer = 0;
            UpdateAttationState();
            if (currentTarget == null)
            {
                Decision();
            }
        }
        //决策间隔
        if(decisionTimer>=decisionInterval)
        {
            decisionTimer = 0;
            Decision();
        }
        
    }

    private void Decision()
    {
        var targets = new List<IHasPosition>();
        if(IsAction(out targets))
        {
            currentIntentStatus = IntentStatus.Action;
        }
        else if(IsChaseIntent(out targets))
        {
            currentIntentStatus = IntentStatus.Chase;
        }
        else if(IsFindIntent(out targets))
        {
            currentIntentStatus = IntentStatus.Find;
        }
        else
        {
            currentIntentStatus = IntentStatus.Idle;
        }

        Debug.Log($"当前意图：{currentIntentStatus} 单位:{(unit as MonoBehaviour).name}");


        if(currentIntentStatus==IntentStatus.Idle)
        {
            CurrentTarget = null;
            decisionExecution.Idle();
            return;
        }
        IHasPosition target=null;
        //基于意图评估当前目标
        if(currentIntentType==IntentType.Attack)
        {
            target = targetEvaluation.EvaluateUnitAndBuilding(targets, TargetType.Unit, unit);
            CurrentTarget = TargetChoose(target,currentTarget);
        }
        else if(currentIntentType==IntentType.Help)
        {
            var units=new List<BaseUnit>();
            foreach (var item in targets)
            {
                units.Add(item as BaseUnit);
            }
            target = targetEvaluation.EvaluateHelpUnit(units, unit);
            CurrentTarget = target;
        }

        if(currentIntentStatus==IntentStatus.Action)
        {
            if(currentIntentType==IntentType.Attack)
            {
                decisionExecution.ActionToTarget(UnitAction.Attack,currentTarget);
            }
            else if(currentIntentType==IntentType.Help)
            {
                decisionExecution.ActionToTarget(UnitAction.Help,currentTarget);
            }
        }
        else if(currentIntentStatus==IntentStatus.Chase)
        {
            //找到最佳移动位置
            var attackPoint = PointDecision.FindAttackPoint(unit as BaseUnit,currentTarget,mask,actionRange,MovementRules.IsWalkable,MovementRules.IsPreOccupyable,IsActionable_Pos);
            if (attackPoint != null)
            {
                Vector2Int gridPos = attackPoint.Value;
                //Debug.Log($"追击位置：{gridPos} 当前单位:{(unit as MonoBehaviour).name} 当前位置:{unit.GridPos} 目标位置:{currentTarget.GridPos}");
                decisionExecution.MoveToPos(gridPos);
            }
        }
        else if(currentIntentStatus==IntentStatus.Find)
        {
            //找到最佳移动位置
            var targetPos = PointDecision.FindOccupiablePoint(unit as BaseUnit,currentTarget,MovementRules.IsWalkable,MovementRules.IsPreOccupyable);
            if (targetPos != null)
            {
                Vector2Int gridPos = targetPos.Value;
                decisionExecution.MoveToPos(gridPos);
            }
        }
    }
    public bool IsAction(out List<IHasPosition> targets)//是否需要执行动作
    {
        targets=new List<IHasPosition>();//使用out的话,就多new一次,总之out的语义挺清晰的:即不管输入是什么,输出一个新列表.
        //先判断自身是否处于合法位置
        if (!(unit as BaseUnit).movement.stepMover.IsOccupy)
        {
            return false;
        }
        //基于动作执行范围检查是否有敌人
        var cells = GridRange.Get(unit.GridPos, actionRange, GridRangeShape.Circle);
        targets = CollectTargets(currentIntentType,cells);
        targets = IntentUniqueFilter(targets,currentIntentType);//意图独有过滤
        targets = targetFiltering.VisionFiltering(targets, mask);//视线过滤
        //因为当前只要能看见目标且在动作执行范围内，就可以执行动作，所以不需要判断
        return targets.Count > 0;
    }
    
    private List<IHasPosition> CollectTargets(IntentType intentType,List<Vector2Int> cells)
    {
        var targets = new List<IHasPosition>();
        switch (intentType)
        {
            case IntentType.Attack:targets= targetProvider.GetAllTargetsInRange(unit.Faction.enemies[0], cells);
                break;
            case IntentType.Help:
                {
                    targets= targetProvider.GetAllTargetsInRange(unit.Faction, cells);
                }
                break;
            default:
                break;
        }
        return targets;
    }
    private List<IHasPosition> IntentUniqueFilter(List<IHasPosition> targets,IntentType intentType)
    {
        var out_targets=new List<IHasPosition>();
        switch (intentType)
        {
            case IntentType.Attack:
                {
                    //目前没有可攻击性判断,所以直接返回所有目标
                }
                break;
            case IntentType.Help://残血判断
                {
                    foreach(var target in targets)//残血判断
                    {
                        var helpTarget = target as BaseUnit;
                        if(helpTarget!=null&&helpTarget.attributes.CurrentHealth<helpTarget.attributes.MaxHealth)
                        {
                            out_targets.Add(target);
                        }
                    }
                    targets=out_targets;
                }
                break;
            default:
                break;
        }
        return targets;
    }
    public bool IsChaseIntent(out List<IHasPosition> targets)//////追击意图判断
    {
        targets=new List<IHasPosition>();
        //基于追击范围检查是否有敌人
        var cells = GridRange.Get(unit.GridPos, chaseRange, GridRangeShape.Circle);
        //本来是想减掉攻击范围的,但是因为单位有可能因为自身原因处于非法攻击位置,所以攻击范围内的敌人也可能需要追击,所以不减掉攻击范围
        targets = CollectTargets(currentIntentType,cells);
        targets = IntentUniqueFilter(targets,currentIntentType);//意图独有过滤
        targets = targetFiltering.VisionFiltering(targets, mask);//视线过滤
        targets = ActionableFilter(targets);//可接近性判断
        if(targets.Count>0)
        {
            return true;
        }
        return false;
    }
    public bool IsFindIntent(out List<IHasPosition> targets)//////查找意图判断
    {
        targets=new List<IHasPosition>();
        var detectortargets=new List<BaseUnit>();
        //基于全地图范围检查是否有目标
        switch (currentIntentType)
        {
            case IntentType.Attack:detectortargets= FactionManager.Instance.GetUnits(unit.Faction.enemies[0]);
                break;
            case IntentType.Help:detectortargets= FactionManager.Instance.GetUnits(unit.Faction);
                break;
            default:
                break;
        }
        foreach(var detectortarget in detectortargets)//可接近性判断
        {
            if(IsActionable(unit as BaseUnit,detectortarget ,actionRange))
            {
                targets.Add(detectortarget);
            }
        }
        if(targets.Count>0)
        {
            return true;    
        }
        return false;
    }

    
    private List<IHasPosition> ActionableFilter(List<IHasPosition> targets)
    {
        var detectortargets = new List<IHasPosition>();
        foreach(var detectortarget in targets)//可接近性判断
        {
            if(IsActionable(unit as BaseUnit,detectortarget ,actionRange))
            {
                detectortargets.Add(detectortarget);
            }
        }
        return detectortargets;
    }
    public bool IsActionable(IFactionMember unit,IHasPosition targetUnit,int attackRange,GridRangeShape attackShape = GridRangeShape.Circle)
    {
        List<Vector2Int> attackPositions = GridRange.Get(targetUnit.GridPos,attackRange,attackShape);
        
        foreach (Vector2Int pos in attackPositions)
        {
            if (MovementRules.IsPreOccupyable(pos, unit)&&IsActionable_Pos(mask,pos,targetUnit.GridPos,attackRange))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsActionable_Pos(LayerMask mask,Vector2Int start,Vector2Int goal,int attackRange)//对于当前位置能否对目标位置执行动作的判断,这里也就是视线的判断.
    {
        //检查是否在攻击范围内
        if (Vector2Int.Distance(start, goal) > attackRange)
        {
            return false;
        }
        Vector2 worldStart = GridManager.Instance.CellToWorld(start);
        Vector2 worldGoal = GridManager.Instance.CellToWorld(goal);
        Vector2 direction = worldGoal - worldStart;
        float distance = direction.magnitude - 1.3f;//需要排除目标

        // 如果距离为0，直接返回true（已在目标点）
        if (distance <= 0f)
            return true;

        // 发射2D射线，只检测 obstacleLayer 指定的层
        RaycastHit2D hit = Physics2D.Raycast(worldStart, direction, distance, mask);

        // 如果没有碰到任何建筑/遮挡物，返回true；否则返回false
        return hit.collider == null;
    }
    private void UpdateAttationState()
    {
        var mono = currentTarget as MonoBehaviour;//隐藏检查
        if (mono != null)
        {
            if (mono.gameObject.activeInHierarchy == false)
            {
                //注意修改共享数据
                CurrentTarget = null;
                return;
            }
        }
        else
        {
            CurrentTarget = null;
            return;
        }
    }
    private IHasPosition TargetChoose(IHasPosition target , IHasPosition currentTarget)
    {
        //评估是否值得更换目标
        if(currentTarget==null)
        {
            return target;
        }
        else if(target!=null&&target!=currentTarget){
            if (targetEvaluation.EvaluateChangeable(target, currentTarget, minDistanceDifference))
            {
                return target;
            }
        }
        return currentTarget;
    }
}