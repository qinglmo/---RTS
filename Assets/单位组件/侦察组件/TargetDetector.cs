using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 目标检测器：定期扫描周围敌人，维护追击目标（chaseTarget）和攻击目标（currentTarget）。
/// 追击目标：chaseRange 内最近的敌人。
/// 攻击目标：attackRange 内最近的敌人（攻击范围判定方式可切换：距离或网格）。
/// </summary>
public class TargetDetector : MonoBehaviour
{
    #region 枚举
    /// <summary>
    /// 攻击范围判定方式
    /// </summary>
    public enum RangeMode
    {
        Circle,   
        Line   
    }
    #endregion
    #region 序列化字段
    [Header("检测设置")]
    [SerializeField] private int chaseRange = 20;          // 追击范围
    [SerializeField] private int attackRange = 1;             // 攻击距离（网格单位）
    [SerializeField] private RangeMode attackRangeMode;// 攻击范围判定方式
    [SerializeField] private RangeMode chaseRangeMode;// 追击范围判定方式

    [Header("运行时数据")]
    [SerializeField] private IFactionMember currentTarget;                  // 当前攻击目标

    [Header("检测频率")]
    [SerializeField] private float scanInterval = 0.5f;   // 扫描新敌人间隔
    [SerializeField] private float checkInterval = 0.1f;  // 检测当前目标间隔

    private float scanTimer;
    private float checkTimer;
    [Header("切换阈值")]
    [SerializeField] private float minDistanceDifference = 1f;   // 切换目标所需的最小距离差距
    private bool isDetectionActive = true;
    #endregion
    #region 公开属性
    public IFactionMember CurrentTarget => currentTarget;                   // 只读，供外部获取攻击目标


    public Vector2Int? lastPos;//记录敌人的最后一个位置,用于记忆追踪
    public AutoExpireNullable<Vector2Int> lastMessagePos;//记录最新一个敌军的位置。
    /// <summary>
    /// 当前实体的网格坐标（用于网格模式下的攻击范围判定）。
    /// 需要外部每帧更新（例如由 GridMovement 组件赋值）。
    /// </summary>
    public Vector2Int CurrentGridPosition { get { return unit.movement.stepMover.CurrentPos; } set { } }
    #endregion
    
    // 在 TargetDetector 类中添加
    private BaseUnit unit;
    //事件状态遵循易进易出，易进指进任意时刻有目标即可进攻击状态或者追击状态，易出指任意时刻无目标即可退出。需要配合每帧检查，重复发出事件。
    //需要避免只从有目标到无目标，只从无目标到有目标这样的条件触发，这样的循环非常脆弱。
    public event Action OnChaseTargetFound;//追击目标更新，当没有攻击目标且更新追击目标时触发（包括发现追击目标）。
    public event Action OnAttackTargetFound;//攻击目标更新，同理
    public event Action OnChaseTargetLost;//追击目标丢失
    public event Action OnFriendlyMessage;//接收到友军通知

    public event Action<IFactionMember> OnTargetChanged;
    public bool isCommand = false;//命令模式下，锁定目标

    private ITargetProvider targetProvider;
    private ITargetFiltering targetFiltering;
    private ITargetEvaluation targetEvaluation;
    private IFocusTarget focusTarget;
    
    private int mask;
    public void SetCommandTarget(IFactionMember target)
    {
        isCommand=true;
        currentTarget = target;
    }
    public void StartDetection()
    {
        isDetectionActive = true;
        Update();
    }
    public void StopDetection()
    {
        isDetectionActive = false;
        currentTarget = null;
    }
    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        targetProvider = GetComponent<ITargetProvider>() ?? gameObject.AddComponent<BackgroundScan>();
        targetFiltering = GetComponent<ITargetFiltering>() ?? gameObject.AddComponent<ViewFiltering>();
        targetEvaluation = GetComponent<ITargetEvaluation>() ?? gameObject.AddComponent<BrainDecision>();
        focusTarget = GetComponent<IFocusTarget>() ?? gameObject.AddComponent<FocusedPerception>();
        mask = LayerMask.GetMask("Mountain", "Building");
    }
    private void Start()
    {
        FactionMessage.Instance.GetChannel(unit.Faction).OnEventRaised += ReceiveNewTarget;
        //如果有池化需求的话，可以设置一个初始化函数，并记得入池时阵营数据置null，避免新激活对象使用旧阵营数据。
    }
    private void OnEnable()
    {
        if(unit.Faction!=null)
            FactionMessage.Instance.GetChannel(unit.Faction).OnEventRaised += ReceiveNewTarget;
    }
    private void OnDisable()
    {
        FactionMessage.Instance.GetChannel(unit.Faction).OnEventRaised -= ReceiveNewTarget;
    }
    
    private void Update()
    {
        if (!isDetectionActive) return;
        //低频扫描新敌人
        
        scanTimer += Time.deltaTime;
        
        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            UpdateTarget();
        }
        
        // 高频检测当前目标
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            UpdateAttationState();
        }
        if(currentTarget as MonoBehaviour != null)
        {
            lastPos = currentTarget.GridPos;
        }
    }
    private void UpdateTarget()
    {
        if(isCommand)
            return;
        if (currentTarget as MonoBehaviour != null)//发现敌人
        {
            FactionMessage.Instance.GetChannel(unit.Faction).RaiseEvent(new EnemySpottedMessage { enemy = currentTarget, spotter = unit });
        }
        var targets = targetProvider.GetAllTargetsInRange(chaseRange, RangeMode.Circle, unit.Faction.enemies[0]);
        targets=targetFiltering.VisionFiltering(targets,mask);//视线过滤
        var target = targetEvaluation.Evaluate(targets,currentTarget, unit, CurrentGridPosition);
        if(target != currentTarget)
        {
            OnTargetChanged?.Invoke(target);
        }
        currentTarget = target;
    }
    private void UpdateAttationState()
    {
        var mono = currentTarget as MonoBehaviour;//隐藏检查
        if (mono != null&& mono.gameObject.activeInHierarchy==false)
        {
            currentTarget = null;
            OnTargetChanged?.Invoke(null);
            ChangeState(AttationState.Nothing);
            OnChaseTargetLost?.Invoke();
            return;
        }

        if (focusTarget.IsTargetInRange(currentTarget, attackRange, attackRangeMode, CurrentGridPosition)
            &&targetFiltering.IsVisiableToOne(currentTarget, mask))
        {
            if (!unit.movement.stepMover.IsOccupy)
                return;
            ChangeState(AttationState.Attacking);
            OnAttackTargetFound?.Invoke();//无脑发送即可，接收端自行处理转换，该事件的意思就是提供需要进入攻击状态或者保持攻击状态的信息。
            return;
        }
        if (focusTarget.IsTargetInRange(currentTarget, chaseRange, chaseRangeMode, CurrentGridPosition))
        {
            ChangeState(AttationState.Chaseing);
            OnChaseTargetFound?.Invoke();

        }
        else
        {
            if(isCommand)//命令模式不丢失目标
                return;
            if(lastPos!=null) 
                ChangeState(AttationState.Finding);
            else
                ChangeState(AttationState.Nothing);
            OnChaseTargetLost?.Invoke();
        }
    }
    public enum AttationState
    {
        Attacking,
        Chaseing,
        Finding,//寻找敌人，试探性搜索敌人
        Nothing,//没有目标，但是依然在扫描
    }
    public void ChangeState(AttationState attationState)
    {
        currentState=attationState;
    }
    public AttationState currentState=AttationState.Nothing;

    private void ReceiveNewTarget(EnemySpottedMessage enemySpottedMessage)
    {
        if (currentState == AttationState.Nothing &&currentTarget==null)
        {
            
            if (lastMessagePos== null ||lastMessagePos.Value==null)
            {
                lastMessagePos = new AutoExpireNullable<Vector2Int>(enemySpottedMessage.enemy.GridPos, 15f);
                Debug.Log("接受到敌人信息");
                OnFriendlyMessage?.Invoke();
            }
        }
    }
}