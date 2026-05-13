using System;
using UnityEngine;

/// <summary>
/// 只负责最机械最原始的移动，不负责寻路，仅负责单位占用初始化
/// 实际位置离散跳跃，视觉表现连续。
/// 实现以下细节：逻辑位置在视觉对象到达格子边界中心后更新,将移动拆分为边界移动和回归两个状态。外部任何指令只会触发移动和回归。
/// </summary>
public class StepMover: MonoBehaviour
{
    public enum MainState
    {
        MovingBorder,//正在移动状态,
        MovingCenter,//回归状态，
        Nothing,//外部可以通过检查Nothing状态发送新的方向,实际上也会发送单步完成事件，外部订阅即可。
        Pushing//物理推动，不允许打断，
    }
    //注意处理视觉对象和主物体之间的关系，当主物体跳变时，视觉对象需要对齐主物体
    [Header("移动设置")]
    public float MoveSpeed = 3;//每s移动多少格
    public float moveDuration = 0.3f; // 
    public float moveMagnification = 1;//移动速度倍率
    public float MoveDeration
    {
        set { moveDuration = value; }
        get { return moveDuration / moveMagnification; }
    }
    public Vector2Int direction=Vector2Int.zero;//当前移动方向，zero表示未移动或者正在回归中
    public Vector2Int CurrentPos { get;private set;}//该位置用于位置检测，避免连续位置检测范围来回震荡，单步移动更新。
    public bool IsOccupy { get { return moveApplyWorld.IsOccupy(baseUnit); } }
    public event System.Action<Vector2Int> OnStepCompleted; // 每移动一格后触发
    
    public event Action<bool> OnOccupyChanged;
    public SpriteRenderer playerSprite;
    public BaseUnit baseUnit;
    public StepMoverStateMachine stateMachine;
    public ISnapToGrid snapToGrid;
    public IMoveApplyWorld moveApplyWorld;
    public void OnStepCompletedTrriger()
    {
        OnStepCompleted?.Invoke(CurrentPos);
    }
    public void ChangeCurrentPos(Vector2Int currentPos)
    {
        CurrentPos = currentPos; ;
    }
    public void OnOccupyChangedTrriger(bool isOccupy)
    {
        OnOccupyChanged?.Invoke(isOccupy);
    }
    public void InitializeOccupancy(BaseUnit baseUnit)
    {
        this.baseUnit = baseUnit;
        snapToGrid = GridManager.Instance;
        moveApplyWorld = GridWorldService.Instance;
        CurrentPos = snapToGrid.WorldToCell(transform.position);
        moveApplyWorld.FristOccupy(baseUnit, CurrentPos);
        playerSprite = transform.Find("Character").GetComponent<SpriteRenderer>();
        stateMachine =new StepMoverStateMachine(stateEnum =>//状态池化后只会创建一次，所有动态数据不要通过这个创建
        {
            return stateEnum switch
            {
                MainState.Nothing => new NothingState_StepMover(this),
                MainState.MovingBorder => new MoveToBorderState( this),
                MainState.MovingCenter => new MoveToCenterState(this),
                MainState.Pushing => new PushState_StepMover(this),
                _ => throw new ArgumentException("无效状态")
            };
        }, this);
        stateMachine.Initialize(MainState.Nothing);
    }
    private void OnDestroy()
    {
        // 释放占用的格子
        moveApplyWorld.LastRelease(baseUnit, CurrentPos);
    }
    void OnDisable()
    {
        // 释放占用的格子
        moveApplyWorld.LastRelease(baseUnit, CurrentPos);
    }
    public void Update()
    {
        stateMachine.Update();
    }
    public void NextStepDerection(Vector2Int target)
    {
        var direction=target - CurrentPos;
        // 确保方向是相邻格子（单位向量）
        if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1)
        {
            Debug.LogWarning("NextStepDirection: target must be adjacent cell");
            return;
        }

        if (direction == this.direction&&this.direction != Vector2Int.zero)
        {
            //当前正在移动
            return;
        }
        if (direction!=Vector2Int.zero)
        {
            this.direction = direction;//提供方向即可
            stateMachine.ChangeState(MainState.MovingBorder);//状态机允许自切换
        }
    }
    /// <summary>
    /// 取消当前移动（仅当不在回撤中时有效）
    /// </summary>
    public void CancelMove()
    {
        stateMachine.ChangeState(MainState.MovingCenter);
    }

    public Vector2Int pushTarget;
    /// <summary>
    /// 外部调用：强制推动单位到指定网格坐标（无视阻挡，固定时长）
    /// </summary>
    /// <param name="target">目标网格坐标</param>
    public void PushTo(Vector2Int target)
    {

        pushTarget = target;
        // 强制切换到推动状态
        if(stateMachine.CurrentStateEnum==MainState.Nothing)
            stateMachine.ChangeState(MainState.Pushing);
        else
        {
            Debug.LogWarning("异常弹开");
        }
    }
    /// <summary>
    /// 尝试将单位沿指定方向推开一格（用于被挤占、击退等场景）
    /// </summary>
    /// <param name = "direction" > 推开方向（应为单位向量，如 Vector2Int.up）</param>
    /// <returns>是否成功移动（若目标格子均被占用则返回 false）</returns>
    public bool TryShiftFromDirection(Vector2Int direction)
    {

        return false;
    }
    /// <summary>
    /// 将单位瞬间传送到指定网格坐标（用于跳跃/传送技能）
    /// </summary>
    /// <param name="targetGrid">目标网格坐标</param>
    /// <returns>是否成功传送</returns>
    public bool TeleportToGrid(Vector2Int targetGrid)
    {
        // 取消正在进行的移动
        CancelMove();
        return false;
    }
}
