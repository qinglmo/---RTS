using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static StepMover;


// 泛型状态机组件，挂载在任意 GameObject 上，T 为实体类型（必须是 MonoBehaviour）
public class StepMoverStateMachine
{
    public IState currentState;//IState表示状态实例（接口），TState表示状态枚举类型。
    private StepMover.MainState currentStateEnum;
    private Func<StepMover.MainState, IState> stateFactory; // 根据枚举创建状态实例的委托
    private StepMover stepMover;
    private Dictionary<StepMover.MainState, IState> statePool = new Dictionary<StepMover.MainState, IState>();//状态对象池
    public StepMover.MainState CurrentStateEnum { get { return currentStateEnum; } }
    public StepMoverStateMachine(Func<StepMover.MainState, IState> stateFactory, StepMover stepMover)
    {
        this.stateFactory = stateFactory;
        this.stepMover = stepMover;
    }
    /// <summary> 初始化状态机，必须在使用前调用 </summary>
    public void Initialize(StepMover.MainState initialState)
    {
        TransitionTo(initialState);
    }
    // 为方便状态类调用，提供一个公开的切换方法
    public void ChangeState(StepMover.MainState newState) => TransitionTo(newState);

    public void Update()
    {
        currentState?.Update();
    }
    /// <summary> 切换到指定枚举状态 </summary>
    private void TransitionTo(StepMover.MainState newStateEnum)
    {
        currentStateEnum = newStateEnum;
        currentState?.Exit();
        if (!statePool.TryGetValue(newStateEnum, out var newState))
        {
            newState = stateFactory(newStateEnum);
            statePool[newStateEnum] = newState;
            currentState = newState;
            currentState?.Enter();
        }
        else
        {
            currentState = newState;
            currentState?.Reset();
        }

    }
}
