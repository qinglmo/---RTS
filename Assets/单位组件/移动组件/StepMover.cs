using System;
using UnityEngine;

/// <summary>
/// 只负责最机械最原始的移动，不负责寻路，仅负责单位占用初始化
/// 实际位置离散跳跃，视觉表现连续。
/// 实现以下细节：当单位移动中时可以根据新的目标位置灵活决定是取消移动转向，还是刚好继续移动。
/// 视觉表现上单位不再跳跃，在取消移动时必须等待视图归位才能继续移动。
/// </summary>
public class StepMover: MonoBehaviour
{
    public enum MainState
    {
        Moving,//正在移动状态,
        Reverting,//指因为改变路径或者被中断而不得不先回到原位，然后再发起移动的状态。通过维护移动方向，告诉回撤状态，下一步的方向。
        //在Reverting状态时，不会响应外部的任何需求。
        Nothing,//外部可以通过检查Nothing状态发送新的方向
        Pushing//物理推动，不允许打断，
    }
    //注意处理视觉对象和主物体之间的关系，当主物体跳变时，视觉对象需要对齐主物体
    [Header("移动设置")]
    public float moveDuration = 0.3f; // 每格移动耗时
    public float moveMagnification = 1;//移动速度倍率
    public float MoveDeration
    {
        set { moveDuration = value; }
        get { return moveDuration / moveMagnification; }
    }
    public Vector2Int direction=Vector2Int.zero;//当前移动方向，zero表示未移动
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
        stateMachine=new StepMoverStateMachine(stateEnum =>//状态池化后只会创建一次，所有动态数据不要通过这个创建
        {
            return stateEnum switch
            {
                MainState.Nothing => new NothingState_StepMover(this),
                MainState.Moving => new MoveState_StepMover( this),
                MainState.Reverting => new RevertState_StepMover(this),
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
        if (this.direction == Vector2Int.zero)
        {
            // 空闲，直接开始移动
            this.direction = direction;//提供方向即可，状态机会自动切换.
            if (stateMachine.CurrentStateEnum ==MainState.Nothing)//避免卡顿，立刻切换
            {
                stateMachine.ChangeState(MainState.Moving);
            }
        }
        else
        {
            //正在移动，需要回撤
            this.direction = direction;
            if(stateMachine.CurrentStateEnum !=MainState.Reverting)
                stateMachine.ChangeState(MainState.Reverting);
        }
    }
    /// <summary>
    /// 取消当前移动（仅当不在回撤中时有效）
    /// </summary>
    public void CancelMove()
    {
        this.direction=Vector2Int.zero;
        if(stateMachine.CurrentStateEnum!=MainState.Reverting)
            stateMachine.ChangeState(MainState.Reverting);
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
