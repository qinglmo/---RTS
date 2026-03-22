using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PushState_StepMover : IState
{
    private StepMover stepMover;
    // 展开原 PushContext 的字段
    private Vector2Int startGrid;
    private Vector2Int targetGrid;
    private float duration;
    private float elapsed;

    public PushState_StepMover(StepMover stepMover) { this.stepMover = stepMover; }

    public void Enter()
    {
        // 初始化推动参数：起始网格、目标网格、持续时间
        startGrid = stepMover.CurrentPos;
        targetGrid = stepMover.pushTarget;
        duration = stepMover.MoveDeration;  // 与原移动时长一致
        elapsed = 0f;

        // 可选：确保目标网格可行（但调用前应已验证）
        // if (!stepMover.moveApplyWorld.IsWalkable(stepMover.baseUnit, targetGrid))
        // {
        //     stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
        //     return;
        // }
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed < duration)
        {
            // 平滑移动精灵
            float t = elapsed / duration;
            stepMover.playerSprite.transform.position = Vector2.Lerp(startGrid, targetGrid, t);
        }
        else
        {
            // 推动完成：确保位置准确、更新状态、通知世界占用
            stepMover.direction = Vector2Int.zero;
            stepMover.transform.position = (Vector2)targetGrid;
            stepMover.playerSprite.transform.position = stepMover.transform.position;
            stepMover.ChangeCurrentPos(targetGrid);
            stepMover.moveApplyWorld.TryOccupy(stepMover.baseUnit, startGrid, targetGrid);
            // 切换到空闲状态
            stepMover.stateMachine.ChangeState(StepMover.MainState.Nothing);
        }
    }

    public void Exit()
    {
        // 无需额外清理
    }

    public void Reset()
    {
        // 重置时重新进入
        Enter();
    }
}