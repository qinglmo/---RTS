using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class NothingState_StepMover : IState
{
    private StepMover stepMover;

    public NothingState_StepMover(StepMover stepMover) { this.stepMover = stepMover; }

    public void Enter()
    {
        // 进入空闲状态，无需特殊初始化
    }

    public void Update()
    {
        // 每帧检查方向，若有输入则切换到移动状态
        if (stepMover.direction != Vector2Int.zero)
        {
            stepMover.stateMachine.ChangeState(StepMover.MainState.Moving);
        }
    }

    public void Exit()
    {
        // 退出空闲状态，无需清理
    }

    public void Reset()
    {
        // 重置时重新进入
        Enter();
    }
}