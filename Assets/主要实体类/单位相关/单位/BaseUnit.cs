using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BaseUnit : MonoBehaviour ,IFactionMember, IOccupyEnity
{
    [SerializeField] private Faction faction = null;
    public Faction Faction { get { return faction; } 
        set { 
            faction = value;
            gameObject.tag=faction.factionTag;
        } 
    }
    public Vector2 Position => transform.position;
    public Vector2Int GridPos => movement.stepMover.CurrentPos;
    [Header("基础组件")]
    public UnitMovement movement;
    [SerializeReference] public UnitAttributes attributes = new UnitAttributes();
    public TargetDetector detector;//这个组件激活时依赖阵营属性，未来需要考虑顺序问题
    public UnitVisualEffect unitVisualEffect; 
    public UnitCommander UnitCommander;
    public MainState mainState;//这个有大量引用，最好放在最后面初始化
    public BehaviorTreeManager behaviorTreeManager;

    public Vector2Int? AttackTarget; //外部提供,决策树检查，有值则执行对应的命令。
    public Vector2Int? DefenseTarget; //外部提供
    [Header("战斗属性")]
    [SerializeField]private int attackRange;
    [Header("选择高亮")]
    public SpriteRenderer selectSprite;
    protected bool isSelected = false;
    [Header("状态高亮")]
    public SpriteRenderer StateSprite;

    [Header("状态配置")]
    public int chaseRange_Advance;//更少的追击范围配置
    public int chaseRange_Normal;//正常追击范围配置
    [Header("状态上下文")]
    //位置目标，暂时放在这里，这是状态间的交互数据，移动状态会读取该数据，并前往该目标
    public Vector2Int? targetPosition=null;
    //决策树读取该数据
    public int chaseRange_Current;//追击范围上下文
    //移动用
    public Vector2Int? commandMovePosition;
    public BaseUnit commandAttackTarget;
    private bool isInitialize=false;
    public virtual void Initialize(int hp, int ack,Faction faction)
    {
        attributes.Initialize(hp, hp, ack, 1);
        isInitialize = true;
        Faction = faction;
        GetComponentInChildren<SpriteRenderer>().color = Faction.factionColor;
        EventBus_Unit.InvokeActivated(this);
    }
    protected virtual void Awake()
    {
        // 自动获取或添加必要组件
        movement = GetComponent<UnitMovement>() ?? gameObject.AddComponent<UnitMovement>();
        if(attributes==null)
            attributes = new UnitAttributes();
        detector = GetComponent<TargetDetector>() ?? gameObject.AddComponent<TargetDetector>();
        UnitCommander = GetComponent<UnitCommander>() ?? gameObject.AddComponent<UnitCommander>();
        unitVisualEffect = GetComponent<UnitVisualEffect>()??gameObject.AddComponent<UnitVisualEffect>();
        mainState = GetComponent<MainState>() ?? gameObject.AddComponent<MainState>();
        behaviorTreeManager = GetComponent<BehaviorTreeManager>() ?? gameObject.AddComponent<BehaviorTreeManager>(); ;

    }
    protected void OnEnable()
    {
        if(faction!=null)
            EventBus_Unit.InvokeActivated(this);
    }
    protected void OnDisable()
    {
        EventBus_Unit.InvokeDeactivated(this);
    }
    protected virtual void Start()
    {
        
        movement.Initialize();
        transform.position = (Vector2)movement.stepMover.CurrentPos;
        attributes.OnDestroyEvent += OnDeathEvent;
        mainState.Initialize(this);
    }
    public virtual void Update()
    {

    }
    
    public virtual void SetSelected(bool selected)
    {
        isSelected = selected;
        if(selectSprite != null)
            selectSprite.enabled = selected;
    }
    protected void OnDeathEvent()
    {
        attributes.OnDestroyEvent -= OnDeathEvent;
        Destroy(gameObject);
    }
    protected virtual void OnDestroy()
    {
        
    }
    public virtual BehaviorTree.Node CreateTree(out BehaviorTree.Blackboard blackboard)
    {
        var focusTarget = GetComponent<IFocusTarget>() ?? gameObject.AddComponent<FocusedPerception>();
        var targetFiltering = GetComponent<ITargetFiltering>() ?? gameObject.AddComponent<ViewFiltering>();
        blackboard = new BehaviorTree.Blackboard(this, focusTarget, targetFiltering, attackRange);
        var parallel=new BehaviorTree.Parallel(blackboard);
        var selector = new BehaviorTree.Selector(blackboard);
        parallel.AddChild(selector);
        //selector.AddChild(new BehaviorTree.LiveJudgNode(blackboard, "隐藏检查"));
        selector.AddChild(new BehaviorTree.AttackNode(blackboard, "攻击节点"));
        selector.AddChild(new BehaviorTree.ChaseNode(blackboard, "追击节点"));
        //selector.AddChild(new BehaviorTree.FindingNode(blackboard, "寻敌节点"));
        //selector.AddChild(new BehaviorTree.SupportNode(blackboard, "支援节点"));
        //selector.AddChild(new BehaviorTree.AdvancementNode(blackboard, "推进节点"));
        selector.AddChild(new BehaviorTree.NothingNode(blackboard, "默认节点",BehaviorTree.NothingNode.StateTier.BranchState));//不同的复合状态可能有不同的默认逻辑。
        var selector2 = new BehaviorTree.Selector(blackboard);
        parallel.AddChild(selector2);
        selector2.AddChild(new BehaviorTree.Attack_RouteNode(blackboard, "推进任务节点"));
        selector2.AddChild(new BehaviorTree.NothingNode(blackboard, "默认节点",BehaviorTree.NothingNode.StateTier.MainState));//不同的复合状态可能有不同的默认逻辑。这里是自由战斗的意思
        return parallel;
    }
    public void MoveToOther()//让单位移动到其他位置
    {
        RouteDecision.MoveToOther(this);
    }

    public void UpdateAbility(float multiplier)//更新单位能力
    {
        attributes.MaxHealth = Mathf.RoundToInt(attributes.base_maxHealth*multiplier);
    }
}
