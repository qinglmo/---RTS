using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MoveState_StepMover : IState
{
    private StepMover stepMover;
    public Vector2Int startGrid;
    public Vector2Int targetGrid;
    public float elapsed;
    public MoveState_StepMover(StepMover stepMover) { this.stepMover = stepMover; }

    public void Enter()
    {
        // 快速检查：如果一开始就有敌人，直接放弃（80%时还会再检查，此检查为可选优化）
        startGrid=stepMover.CurrentPos;
        targetGrid=stepMover.CurrentPos+stepMover.direction;
        elapsed = 0f;
        if (!stepMover.moveApplyWorld.IsWalkable(stepMover.baseUnit, targetGrid))
        {
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
            return;
        }
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / stepMover.MoveDeration;
        // 80% 进度时处理格子
        if (t >= 0.8f)
        {
            if (!stepMover.moveApplyWorld.TryOccupy(stepMover.baseUnit, startGrid, targetGrid))
            {
                // 处理失败（已回滚）则终止移动
                stepMover.direction = Vector2Int.zero;
                stepMover.stateMachine.ChangeState(StepMover.MainState.Reverting);
                return;
            }
            else
            {
                stepMover.ChangeCurrentPos(targetGrid);
            }
        }
        // 平滑移动
        stepMover.playerSprite.transform.position = Vector2.Lerp(startGrid, targetGrid, t);
        if (elapsed > stepMover.MoveDeration)
        {
            //移动成功
            stepMover.transform.position = (Vector2)targetGrid;
            stepMover.playerSprite.transform.position = (Vector2)targetGrid;
            stepMover.direction = Vector2Int.zero;
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
            stepMover.OnStepCompletedTrriger();
        }
    }

    public void Exit()
    {
        
    }

    public void Reset()
    {
        // 快速检查：如果一开始就有敌人，直接放弃（80%时还会再检查，此检查为可选优化）
        startGrid = stepMover.CurrentPos;
        targetGrid = stepMover.CurrentPos + stepMover.direction;
        elapsed = 0f;
        if (!stepMover.moveApplyWorld.IsWalkable(stepMover.baseUnit, targetGrid))
        {
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
            return;
        }
    }

}
