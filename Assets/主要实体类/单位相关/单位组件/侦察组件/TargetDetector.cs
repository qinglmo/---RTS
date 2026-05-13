using BehaviorTree;
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
    #region 序列化字段
    [Header("检测设置")]
    [SerializeField] private int scanRange = 20; //扫描范围
    [Header("运行时数据")]
    [SerializeField] private IHasPosition currentTarget;                  // 当前攻击目标

    [Header("检测频率")]
    [SerializeField] private float scanInterval = 0.5f;   // 扫描新敌人间隔
    [SerializeField] private float checkInterval = 0.1f;  // 检测当前目标间隔

    private float scanTimer;
    private float checkTimer;
    [Header("切换阈值")]
    [SerializeField] private float minDistanceDifference = 1f;   // 切换目标所需的最小距离差距
    private bool isDetectionActive = true;
    private bool openEnemyDetector=true;
    private bool openFriendlyDetector=false;
    #endregion
    #region 公开属性
    public IHasPosition CurrentTarget
    {
        get { return currentTarget; }
        set 
        { 
            if(currentTarget!=value)
            {
                OnTargetChanged?.Invoke(value);
                currentTarget = value;
            }
        }
    }                // 只读，供外部获取攻击目标
    public IHasPosition SupportTarget;
    public Vector2Int? lastPos;//记录敌人的最后一个位置,用于记忆追踪
    /// <summary>
    /// 当前实体的网格坐标（用于网格模式下的攻击范围判定）。
    /// 需要外部每帧更新（例如由 GridMovement 组件赋值）。
    /// </summary>
    public Vector2Int CurrentGridPosition { get { return unit.GridPos; } set { } }
    #endregion
    
    // 在 TargetDetector 类中添加
    private IFactionMember unit;
    public event Action OnFriendlyMessage;//接收到友军通知
    public event Action<IHasPosition> OnTargetChanged;

    private ITargetProvider targetProvider;
    private ITargetFiltering targetFiltering;
    private ITargetEvaluation targetEvaluation;
    
    private int mask;
    public void StartDetection()
    {
        isDetectionActive = true;
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
        mask = LayerMask.GetMask("Mountain", "Building");
    }

    private void Update()
    {
        if (!isDetectionActive) return;
        //低频扫描新敌人
        
        scanTimer += Time.deltaTime;
        
        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            UpdateEnemy();
            UpdateFriendly();
        }
        
        // 高频检测当前目标
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            UpdateAttationState();
        }
    }
    private void UpdateEnemy()
    {
        if(!openEnemyDetector) return;
        var targets = targetProvider.GetAllTargetsInRange(unit.GridPos,scanRange, unit.Faction.enemies[0]);
        targets = targetFiltering.VisionFiltering(targets, mask);//视线过滤
        var target = targetEvaluation.Evaluate(targets, currentTarget, unit);
        CurrentTarget = target;
    }
    private void UpdateFriendly()//寻找处于攻击状态的友军，提供支援
    {
        if(!openFriendlyDetector) return;
        //收集友军的时候需要注意排除自身，因为自身也满足条件，
        var targets = targetProvider.GetAllTargetsInRange(unit.GridPos, scanRange, unit.Faction);
        var target = FriendlySupportEvaluation.Evaluate(targets.Units, unit);
        SupportTarget= target;
    }
    public void OpenEnemyDetector()
    {
        openEnemyDetector = true;
    }
    public void CloseEnemyDetector()
    {
        openEnemyDetector = false;
    }
    public void OpenFriendlyDetector()
    {
        openFriendlyDetector = true;
    }
    public void CloseFriendlyDetector()
    {
        openFriendlyDetector = false;
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
        if (!targetFiltering.IsVisiableToOne(currentTarget, mask))
        {
            currentTarget = null;
        }
        else
        {
            lastPos=currentTarget.GridPos;
        }
    }
}