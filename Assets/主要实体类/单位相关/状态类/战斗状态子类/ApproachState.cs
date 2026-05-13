using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

/// <summary>
/// 该状态同时复用于追击，支援，资源采集
/// </summary>
public class ApproachState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval=1f;//检查间隔
    private StateMachine stateMachine;//通过获取枚举，用于感知自身状态
    
    public ApproachState(BaseUnit unit ,StateMachine stateMachine) 
    { 
        this.unit = unit;
        this.stateMachine = stateMachine;
    }
    private IHasPosition GetCurrentTarget()
    {
        switch (stateMachine.CurrentStateEnum)
        {
            case UnitState.Supporting:
                return unit.detector.SupportTarget;

            case UnitState.Chaseing:
                return unit.detector.CurrentTarget;

            case UnitState.ApprochResource:
                if (unit is WorkUnit workUnit)
                    return workUnit.currentResourceTarget;
                return null;
            case UnitState.ReturnToBase:
                return FactionManager.Instance.mainSettlement;
            default:
                return null;
        }
    }
    public void Enter()
    {
        
        timer = 0f;
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer=0f;
            if (GetCurrentTarget() as MonoBehaviour == null)
            {
                return;
            }
            var tar = GetCurrentTarget().GridPos;
            RouteDecision.TargetApproaching_new(unit, tar);
        }
    }

    public void Exit()
    {
        if (stateMachine.CurrentStateEnum == UnitState.Supporting)
        {
            unit.detector.SupportTarget=null;//退出说明支援到位了，清空支援目标，避免过期支援
        }
    }

    public void Reset()
    {
        Enter();
    }

}


