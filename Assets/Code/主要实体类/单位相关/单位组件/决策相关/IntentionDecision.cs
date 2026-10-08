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
        Chase,
        Find,
        Idle
    }
    public IntentType currentIntent = IntentType.Idle;

    private List<IHasPosition> targets = new List<IHasPosition>();
    #region 序列化字段
    [Header("检测设置")]
    [SerializeField] private int chaseRange = 20; //追击范围
    [SerializeField] private int attackRange = 1; //攻击范围
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
        targets.Clear();//清空目标列表,为下一次决策做准备
        if(IsAttackIntent())
        {
            currentIntent = IntentType.Attack;
        }
        else if(IsChaseIntent())
        {
            currentIntent = IntentType.Chase;
        }
        else if(IsFindIntent())
        {
            currentIntent = IntentType.Find;
        }
        else
        {
            currentIntent = IntentType.Idle;
        }

        Debug.Log($"当前意图：{currentIntent} 单位:{(unit as MonoBehaviour).name}");

        //基于意图评估当前目标
        if(currentIntent==IntentType.Attack)
        {
            var target = targetEvaluation.EvaluateUnitAndBuilding(this.targets, TargetType.Unit, unit);
            CurrentTarget = TargetChoose(target,currentTarget);
            decisionExecution.ActionToTarget(UnitAction.Attack,currentTarget);
        }
        else if(currentIntent==IntentType.Chase)
        {
            var target = targetEvaluation.EvaluateUnitAndBuilding(this.targets, TargetType.Unit, unit);
            CurrentTarget = TargetChoose(target,currentTarget);
            //找到最佳移动位置
            var attackPoint = PointDecision.FindAttackPoint(unit as BaseUnit,currentTarget,mask,attackRange,MovementRules.IsWalkable,MovementRules.IsPreOccupyable,IsAttackable_Pos);
            if (attackPoint != null)
            {
                Vector2Int gridPos = attackPoint.Value;
                Debug.Log($"追击位置：{gridPos} 当前单位:{(unit as MonoBehaviour).name} 当前位置:{unit.GridPos} 目标位置:{currentTarget.GridPos}");
                decisionExecution.MoveToPos(gridPos);
            }
        }
        else if(currentIntent==IntentType.Find)
        {
            var detectortarget = targetEvaluation.EvaluateUnitAndBuilding(this.targets, TargetType.Unit, unit);
            //评估是否值得更换目标
            CurrentTarget = TargetChoose(detectortarget,currentTarget);
            //找到最佳移动位置
            var targetPos = PointDecision.FindOccupiablePoint(unit as BaseUnit,currentTarget,MovementRules.IsWalkable,MovementRules.IsPreOccupyable);
            if (targetPos != null)
            {
                Vector2Int gridPos = targetPos.Value;
                decisionExecution.MoveToPos(gridPos);
            }
        }
        else if(currentIntent==IntentType.Idle)
        {
            CurrentTarget = null;
            decisionExecution.Idle();
        }
    }
    public bool IsAttackIntent()//攻击意图判断
    {
        //基于攻击范围检查是否有敌人
        var cells = GridRange.Get(unit.GridPos, attackRange, GridRangeShape.Circle);
        var detectortarget = targetProvider.GetAllTargetsInRange(unit.Faction.enemies[0], cells);
        targets = targetFiltering.VisionFiltering(detectortarget, mask);//视线过滤
        //因为当前只要能看见敌人且在攻击范围内，就会攻击，所以不需要可攻击性判断
        if(this.targets.Count>0)
        {
            return true;
        }
        return false;
    }
    public bool IsChaseIntent()//追击意图判断
    {
        //基于追击范围检查是否有敌人
        var chasecells = GridRange.Get(unit.GridPos, chaseRange, GridRangeShape.Circle);
        var attackcells = GridRange.Get(unit.GridPos, attackRange, GridRangeShape.Circle);
        var cells = GridRange.Subtract(chasecells, attackcells);
        var detectortargets = targetProvider.GetAllTargetsInRange(unit.Faction.enemies[0], cells);
        detectortargets = targetFiltering.VisionFiltering(detectortargets, mask);//视线过滤
        foreach(var detectortarget in detectortargets)//可攻击性判断
        {
            if(IsAttackable(unit as BaseUnit,detectortarget ,attackRange))
            {
                this.targets.Add(detectortarget);
            }
        }
        if(this.targets.Count>0)
        {
            return true;
        }
        return false;
    }
    public bool IsFindIntent()//查找意图判断
    {
        //基于查找范围检查是否有敌人
        var detectortargets = FactionManager.Instance.GetUnits(unit.Faction.enemies[0]);
        foreach(var detectortarget in detectortargets)//可攻击性判断
        {
            if(IsAttackable(unit as BaseUnit,detectortarget ,attackRange))
            {
                this.targets.Add(detectortarget);
            }
        }
        if(this.targets.Count>0)
        {
            return true;
        }
        return false;
    }

    
    public bool IsAttackable(IFactionMember unit,IHasPosition targetUnit,int attackRange,GridRangeShape attackShape = GridRangeShape.Circle)
    {
        List<Vector2Int> attackPositions = GridRange.Get(targetUnit.GridPos,attackRange,attackShape);
        
        foreach (Vector2Int pos in attackPositions)
        {
            if (MovementRules.IsPreOccupyable(pos, unit)&&IsAttackable_Pos(mask,pos,targetUnit.GridPos,attackRange))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsAttackable_Pos(LayerMask mask,Vector2Int start,Vector2Int goal,int attackRange)//对于当前位置能否攻击到目标位置的判断
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