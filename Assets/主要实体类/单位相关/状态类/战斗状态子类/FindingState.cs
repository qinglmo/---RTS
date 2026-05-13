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
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer = 0f;
            if (unit.detector.lastPos != null)
                RouteDecision.UniversalRouteDecision(unit, unit.detector.lastPos.Value);
        }
        //如果离敌人最后消失的地点小于等于2格，则认为到达最后搜寻点
        if (DistanceCalculate.Heuristic(unit.detector.lastPos.Value, unit.GridPos) > 2)
            stateMachine.ChangeState(UnitState.Nothing);
    }

    public void Exit()
    {

    }

    public void Reset()
    {
        Enter();

    }
}


