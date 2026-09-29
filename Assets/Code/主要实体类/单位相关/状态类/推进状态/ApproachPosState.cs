using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

/// <summary>
/// 该状态用于前往一个目标点
/// </summary>
public class ApproachPosState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval = 1f;//检查间隔
    private StateMachine stateMachine;//通过获取枚举，用于感知自身状态

    public ApproachPosState(BaseUnit unit, StateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }
    private Vector2Int? GetCurrentTarget()
    {
        return unit.targetPosition;
    }
    public void Enter()
    {

        timer = 1f;//进入立即触发，提高流畅度。
        
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer = 0f;
            if(GetCurrentTarget() == null)
            {
                return;
            }
            var tar = GetCurrentTarget().Value;
            RouteDecision.TargetApproaching_new(unit, tar);
        }
    }

    public void Exit()
    {
        
    }

    public void Reset()
    {
        Enter();
    }

}


