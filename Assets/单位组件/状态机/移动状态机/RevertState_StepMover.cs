using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class RevertState_StepMover : IState
{
    private StepMover stepMover;
    // 将原来的结构体字段展开
    private Vector2 startPos;
    private Vector2 targetPos;
    private float duration;
    private float elapsed;

    public RevertState_StepMover(StepMover stepMover) { this.stepMover = stepMover; }

    public void Enter()
    {
        // 初始化回撤参数：从当前精灵位置回到单元的实际网格位置
        startPos = stepMover.playerSprite.transform.position;
        targetPos = new Vector2(stepMover.CurrentPos.x, stepMover.CurrentPos.y);
        float distance = Vector2.Distance(startPos, targetPos);
        // 根据距离动态计算回撤时长（与原移动时长成比例）
        duration = distance * stepMover.MoveDeration;
        elapsed = 0f;
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed < duration)
        {
            float t = elapsed / duration;
            stepMover.playerSprite.transform.position = Vector2.Lerp(startPos, targetPos, t);
        }
        else
        {
            // 回撤完成，确保位置精确对齐网格
            stepMover.playerSprite.transform.position = targetPos;

            // 根据是否有待处理的方向决定下一个状态
            if (stepMover.direction != Vector2Int.zero)
            {
                stepMover.stateMachine.ChangeState(StepMover.MainState.Moving);
            }
            else
            {
                stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
            }
        }
    }

    public void Exit()
    {
        // 无需额外清理
    }

    public void Reset()
    {
        // 重置时重新执行进入逻辑
        Enter();
    }
}