using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class ApproachState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval = 1f;//检查间隔
    private IHasPosition resourceTarger;
    private IStateMachine stateMachine = null;
    public ApproachState(BaseUnit unit, IStateMachine stateMachine) { this.unit = unit; this.stateMachine = stateMachine; }

    public void Enter()
    {
        if(unit is WorkUnit workUnit)
        {
            resourceTarger=workUnit.currentTarget as IHasPosition;
        }
        timer = 0f;
        if(resourceTarger != null)
        {
           RouteDecision.TargetApproaching(unit, resourceTarger.GridPos);
        }
        else
        {
            Debug.LogError("预料之外的错误，资源类没有实现位置接口");
            stateMachine.ChangeState(UnitState.Nothing); // 或者切换回默认状态
        }
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer = 0f;
            if (resourceTarger != null)
            {
                if (Heuristic(unit.GridPos, resourceTarger.GridPos) <= 1)
                {
                    stateMachine.ChangeState(UnitState.Working);
                }
                else
                    RouteDecision.TargetApproaching(unit, resourceTarger.GridPos);
            }
            
        }
    }

    public void Exit()
    {

    }

    public void Reset()
    {

        if (unit is WorkUnit workUnit)
        {
            resourceTarger = workUnit.currentTarget as IHasPosition;
        }
        timer = 0f;
        if (resourceTarger != null)
        {
            RouteDecision.TargetApproaching(unit, resourceTarger.GridPos);
        }
    }
    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}


