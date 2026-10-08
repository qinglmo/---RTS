using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;



public interface IStateMachine
{
    void ChangeState(UnitState newState , ChangeSource source = ChangeSource.Intention);

}

public enum ChangeSource
{
    Intention,
    StateSelfTransition,
    External
}
// 泛型状态机组件，挂载在任意 GameObject 上，T 为实体类型（必须是 MonoBehaviour）
public class StateMachine:IStateMachine
{
    private IState currentState;//IState表示状态实例（接口），TState表示状态枚举类型。
    private UnitState currentStateEnum=UnitState.Idle;
    private BaseUnit unit;
    private Dictionary<UnitState, IState> statePool = new Dictionary<UnitState, IState>();
    public UnitState CurrentStateEnum { get { return currentStateEnum; } }
    public StateMachine(BaseUnit unit)
    {
        this.unit = unit;
        statePool[UnitState.ActionToTarget]=new ActionToTargetState(unit);
        statePool[UnitState.Idle]=new IdleState(unit);
        statePool[UnitState.MoveToPos]=new MoveToPosState(unit,this);
        statePool[UnitState.Stunned]=new StunState(unit);
        currentState = statePool[UnitState.Idle];
    }

    /// <summary> 初始化状态机，必须在使用前调用 </summary>
    public void Initialize(UnitState initialState)
    {
        TransitionTo(initialState);
    }
    // 为方便状态类调用，提供一个公开的切换方法
    public void ChangeState(UnitState newState, ChangeSource source=ChangeSource.Intention)
    {
        if (source == ChangeSource.StateSelfTransition)
        {
            TransitionTo(newState);
        }
        else if (source == ChangeSource.Intention)
        {
            if (!IsChangeable(newState))
            {
                return;
            }
            TransitionTo(newState);
        }
        else if (source == ChangeSource.External)
        {
            TransitionTo(newState);
        }
    }
    private bool IsChangeable(UnitState newState)
    {
        if(currentStateEnum==UnitState.Stunned)
        {
            return false;
        }
        return true;
    }
    public void Update()
    {
        currentState?.Update();
    }
    /// <summary> 切换到指定枚举状态 </summary>
    private void TransitionTo(UnitState newStateEnum)
    {
        currentState?.Exit();
        currentStateEnum =newStateEnum;
        currentState = GetState(newStateEnum);
        currentState?.Enter();
    }

    public void ResetCurrentState()
    {
        currentState?.Reset();
    }

    public IState GetCurrentState()
    {
        return currentState;
    }

    public IState GetState(UnitState stateEnum)
    {
        if (statePool.ContainsKey(stateEnum))
        {
            return statePool[stateEnum];
        }
        return null;
    }

}
