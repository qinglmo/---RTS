using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class FindingState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval = 1f;//检查间隔
    private IStateMachine stateMachine = null;
    public FindingState(BaseUnit unit,IStateMachine stateMachine) { this.unit = unit;this.stateMachine = stateMachine; }

    public void Enter()
    {
        timer = 0f;
        if (unit.detector.lastPos != null)
            RouteDecision.UniversalRouteDecision(unit, unit.detector.lastPos.Value);
        else if (unit.detector.lastMessagePos != null && unit.detector.lastMessagePos.Value != null)
            RouteDecision.UniversalRouteDecision(unit, (Vector2Int)unit.detector.lastMessagePos.Value);
    }

    public void Update()
    {
        if (unit.GridPos == unit.detector.lastPos)
        {
            unit.detector.lastPos = null;  
        }
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer = 0f;
            if (unit.detector.lastPos != null)
                RouteDecision.UniversalRouteDecision(unit, unit.detector.lastPos.Value);
            else if (unit.detector.lastMessagePos != null&& unit.detector.lastMessagePos.Value != null)
                RouteDecision.UniversalRouteDecision(unit, (Vector2Int)unit.detector.lastMessagePos.Value);
            else
            {
                stateMachine.ChangeState(UnitState.Nothing);
            }
        }
    }

    public void Exit()
    {

    }

    public void Reset()
    {
        timer = 0f;
        if (unit.detector.lastPos != null)
            RouteDecision.UniversalRouteDecision(unit, unit.detector.lastPos.Value);
        else if (unit.detector.lastMessagePos != null && unit.detector.lastMessagePos.Value != null)
            RouteDecision.UniversalRouteDecision(unit, (Vector2Int)unit.detector.lastMessagePos.Value);
    }
}


