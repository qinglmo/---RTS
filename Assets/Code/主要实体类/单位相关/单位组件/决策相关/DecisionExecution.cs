using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 翻译决策意图，将其转换为实际的操作指令。
/// 提供决策模版，外部传入实际选择，由决策执行器根据模版和选择生成实际的操作指令。
/// </summary>
public class DecisionExecution : MonoBehaviour
{
    private StateMachine stateMachine;
    BaseUnit unit;
    public void Init(BaseUnit unit)
    {
        this.unit=unit;
        stateMachine=new StateMachine(unit);
        stateMachine.Initialize(UnitState.Idle);
        
    }
    void Update()
    {
        stateMachine.Update();
    }
    //动作决策模版
    public void ActionToTarget(UnitAction action,IHasPosition target)
    {
        //先判断意图和当前状态是否相等
        if (stateMachine.CurrentStateEnum == UnitState.ActionToTarget)
        {
            var state = stateMachine.GetState(UnitState.ActionToTarget) as ActionToTargetState;
            //然后判断意图是否完全等价
            if(state.IsEqualState(action, target))
            {
                return;
            }
            else
            {
                var config=new ActionConfig();
                config.action=action;
                config.target=target;
                config.chargeUpTime=unit.unitVisualEffect.chargeUpTime;
                config.windupTime=unit.unitVisualEffect.windupTime;
                config.recoveryTime=unit.unitVisualEffect.recoveryTime;
                config.restTime=unit.unitVisualEffect.restTime;
                state.SetActionConfig(config);
                stateMachine.ResetCurrentState();//重置当前状态，重新执行动作
            }
        }
        else
        {
            var state = stateMachine.GetState(UnitState.ActionToTarget) as ActionToTargetState;
            var config=new ActionConfig();
            config.action=action;
            config.target=target;
            config.chargeUpTime=unit.unitVisualEffect.chargeUpTime;
            config.windupTime=unit.unitVisualEffect.windupTime;
            config.recoveryTime=unit.unitVisualEffect.recoveryTime;
            config.restTime=unit.unitVisualEffect.restTime;
            state.SetActionConfig(config);
            stateMachine.ChangeState(UnitState.ActionToTarget);//先设置参数再切换状态，避免切换状态时参数未设置
        }
    }

    public void MoveToPos(Vector2Int pos)
    {
        if (stateMachine.CurrentStateEnum == UnitState.MoveToPos)
        {
            var state = stateMachine.GetState(UnitState.MoveToPos) as MoveToPosState;
            if(state.IsEqualState(pos))
            {
                return;
            }
            else
            {
                state.SetPos(pos);
                stateMachine.ResetCurrentState();
            }
        }
        else
        {
            var state = stateMachine.GetState(UnitState.MoveToPos) as MoveToPosState;
            state.SetPos(pos);
            stateMachine.ChangeState(UnitState.MoveToPos);
        }
    }

    public void Stun()
    {
        stateMachine.ChangeState(UnitState.Stunned);
    }

    public void Idle()
    {
        stateMachine.ChangeState(UnitState.Idle);
    }

    public UnitState GetCurrentState()
    {
        return stateMachine.CurrentStateEnum;
    }
}
