using UnityEngine;

public class MoveToBorderState : IState
{
    private StepMover stepMover;

    private Vector2 startPos;           // 起始视觉位置
    private Vector2 borderMidPoint;     // 边界中点（世界坐标）
    private float elapsed;
    private float duration;             // 移动总时间

    public MoveToBorderState(StepMover stepMover)
    {
        this.stepMover = stepMover;
    }

    public void Enter()
    {
        if (stepMover.direction == Vector2Int.zero)
        {
            // 无方向，直接退出
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
            return;
        }

        // 计算边界中点
        Vector2 currentCenter = (Vector2)stepMover.CurrentPos;
        Vector2 targetCenter = currentCenter + (Vector2)stepMover.direction;
        borderMidPoint = (currentCenter + targetCenter) / 2f;

        startPos = stepMover.playerSprite.transform.position;
        elapsed = 0f;

        // 计算移动时间（速度恒定）
        float distance = Vector2.Distance(startPos, borderMidPoint);
        duration = distance / stepMover.MoveSpeed;

        // 可选：检查目标格子是否可走，若不可走则直接回退（转为移动到原格子的边界中点）
        Vector2Int targetGrid = stepMover.CurrentPos + stepMover.direction;
        if (!stepMover.moveApplyWorld.IsWalkable(stepMover.baseUnit, targetGrid))
        {
            // 回退：将边界中点改为原格子与自身的边界中点（即反向）
            borderMidPoint = (currentCenter + currentCenter) / 2f; // 实际上就是 currentCenter，但为了统一，仍用中点概念
            // 重新计算距离和时间
            distance = Vector2.Distance(startPos, borderMidPoint);
            duration = distance / stepMover.MoveSpeed;
            // 注意：此时 direction 仍然指向原目标，但移动目标已是原格子中心（实际上到原格子中心），下面逻辑会处理
        }
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // 平滑移动
        Vector2 newPos = Vector2.Lerp(startPos, borderMidPoint, t);
        stepMover.playerSprite.transform.position = newPos;

        if (t >= 1f)
        {
            // 到达边界中点，立即更新逻辑位置
            TryUpdateLogicPosition();
            stepMover.transform.position = startPos+stepMover.direction;
            stepMover.playerSprite.transform.position = borderMidPoint;
            stepMover.direction = Vector2Int.zero; //无论如何，到达边界了就要清除方向
            // 切换到回归状态
            stepMover.stateMachine.ChangeState(StepMover.MainState.MovingCenter);//无论是否占领成功，都要回归
        }
    }

    private void TryUpdateLogicPosition()
    {
        Vector2Int current = stepMover.CurrentPos;
        Vector2Int target = current + stepMover.direction;
        //不要乱加防御性编程，影响观感，该状态的语义是移动到边界，不是回归自身。

        if (stepMover.moveApplyWorld.TryOccupy(stepMover.baseUnit, current, target))
        {
            stepMover.ChangeCurrentPos(target);
        }
    }

    public void Exit() { }
    public void Reset() { Enter(); }
}