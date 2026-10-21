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
    public float chargeUpTime;
    public float windupTime;
    public float recoveryTime;
    public float restTime;
}
public  class ActionToTargetState : IState
{
    private float timer;
    private BaseUnit unit;
    public bool IsChangeState { get; set;} = true;
    
    enum Phase { ChargeUp, Windup, Recovery,Rest,}
    private Phase currentPhase=Phase.Windup;
    public IHasPosition CurrentTarget;
    public UnitAction action;

    //任何动画时间都必须大于0，否则不播放动画
    public float chargeUpTime;
    public float windupTime;
    public float recoveryTime;
    public float restTime;
    public ActionToTargetState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {
        unit.movement.stepMover.CancelMove();//进入动作状态时取消移动，避免移动和动作冲突
        timer = 0f;
        if (CurrentTarget != null )
        {
            //执行蓄力动画
            unit.unitVisualEffect.ChargeUp(chargeUpTime);//时间小于等于0时，不播放动画
            currentPhase=Phase.ChargeUp;
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.ChargeUp:
                if (timer >= chargeUpTime)
                {
                    //执行动作前摇动画
                    unit.unitVisualEffect.AttackWindup(CurrentTarget.GridPos-unit.GridPos,windupTime);
                    currentPhase=Phase.Windup;
                    timer=0f;
                }
                break;
            case Phase.Windup:
                if (timer >= windupTime)
                {
                    
                    if (CurrentTarget != null)
                    {
                        ActionCompleted.ActionExecute(action,unit,CurrentTarget);
                    }
                    currentPhase=Phase.Recovery;
                    unit.unitVisualEffect.AttackRecovery(recoveryTime);
                    timer=0f;
                }
                break;
            case Phase.Recovery:
                if (timer >= recoveryTime)
                {
                    currentPhase=Phase.Rest;
                    timer=0f;
                }
                break;
            case Phase.Rest:
                if (timer >= restTime)
                {
                    if (CurrentTarget != null )
                    {
                        //执行蓄力动画
                        unit.unitVisualEffect.ChargeUp(chargeUpTime);
                        currentPhase=Phase.ChargeUp;
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
        chargeUpTime = actionConfig.chargeUpTime;
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