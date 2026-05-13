using UnityEngine;

//回归逻辑实现非常简单，只需要知道当前视觉对象位置，和当前逻辑位置即可。
public class MoveToCenterState : IState
{
    private StepMover stepMover;

    private Vector2 startPos;
    private Vector2 targetCenter;
    private float elapsed;
    private float duration;

    public MoveToCenterState(StepMover stepMover)
    {
        this.stepMover = stepMover;
    }

    public void Enter()
    {
        // 起始位置只要在逻辑格子内即可，在正常移动时，这个起始位置通常是边界的中心点
        startPos = stepMover.playerSprite.transform.position;
        // 正常情况下，逻辑位置已经更新为新格子，所以 CurrentPos 就是新格子
        targetCenter = (Vector2)stepMover.CurrentPos;

        elapsed = 0f;
        float distance = Vector2.Distance(startPos, targetCenter);
        if (distance < 0.01f)
        {
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
        }
        duration = distance / stepMover.MoveSpeed;
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        Vector2 newPos = Vector2.Lerp(startPos, targetCenter, t);
        stepMover.playerSprite.transform.position = newPos;

        if (t >= 1f)
        {
            // 到达格子中心
            stepMover.transform.position = targetCenter;
            stepMover.playerSprite.transform.position = targetCenter;
            stepMover.OnStepCompletedTrriger();
            // 进入空闲
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
        }
    }

    public void Exit() { }
    public void Reset() { Enter(); }
}