using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static EnemyUnit;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class ActionConfig
{
    public UnitAction action;
    public IHasPosition target;
    public float windupTime;
    public float recoveryTime;
    public float restTime;
}
public  class ActionToTargetState : IState
{
    private float timer;
    private BaseUnit unit;
    public bool IsChangeState { get; set;} = true;
    
    enum Phase {   Windup, Recovery,Rest,}
    private Phase currentPhase=Phase.Windup;
    public IHasPosition CurrentTarget;
    public UnitAction action;
    public float windupTime;
    public float recoveryTime;
    public float restTime;
    public ActionToTargetState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {
        timer = 0f;
        if (CurrentTarget != null )
        {
            //执行动作前摇动画
            unit.unitVisualEffect.AttackWindup(CurrentTarget.GridPos-unit.GridPos,windupTime);
            currentPhase=Phase.Windup;
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.Windup:
                if (timer >= windupTime)
                {
                    currentPhase=Phase.Recovery;
                    if (CurrentTarget != null)
                    {
                        ActionCompleted.ActionExecute(action,unit,CurrentTarget);
                        
                    }
                    timer=0f;
                }
                break;
            case Phase.Recovery:
                if (timer >= recoveryTime)
                {
                    unit.unitVisualEffect.AttackRecovery(recoveryTime);
                    currentPhase=Phase.Rest;
                    timer=0f;
                }
                break;
            case Phase.Rest:
                if (timer >= restTime)
                {
                    if (CurrentTarget != null )
                    {
                        //执行动作前摇动画
                        unit.unitVisualEffect.AttackWindup(CurrentTarget.GridPos-unit.GridPos,windupTime);
                        currentPhase=Phase.Windup;
                    }
                    timer=0f;
                }
                break;
            default:
                break;
        }
    }

    public void Exit()
    {
        unit.unitVisualEffect.AttackRecovery(recoveryTime);
    }

    public void Reset()
    {
        Enter();
    }

    public void SetActionConfig(ActionConfig actionConfig)
    {
        CurrentTarget = actionConfig.target;
        action = actionConfig.action;
        windupTime = actionConfig.windupTime;
        recoveryTime = actionConfig.recoveryTime;
        restTime = actionConfig.restTime;
    }
    public bool IsEqualState(UnitAction action,IHasPosition target)//一般认为是动作和目标都相等时，状态才相等，否则状态不相等
    {
        return this.action == action && this.CurrentTarget == target;
        //相等的状态不会调用状态机的切换方法
    }
}