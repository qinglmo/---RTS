using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;



public interface IStateMachine
{
    void ChangeState(UnitState newState);
    void HandleEvent(StateEvent evt);
    // 可选的其他方法
}

// 泛型状态机组件，挂载在任意 GameObject 上，T 为实体类型（必须是 MonoBehaviour）
public class StateMachine:IStateMachine
{
    public IState currentState;//IState表示状态实例（接口），TState表示状态枚举类型。
    private UnitState currentStateEnum;
    private Func<UnitState, IState> stateFactory; // 根据枚举创建状态实例的委托
    private BaseUnit unit;
    private Dictionary<UnitState, IState> statePool = new Dictionary<UnitState, IState>();//状态对象池
    public UnitState CurrentStateEnum { get { return currentStateEnum; } }

    public event Action<BaseUnit,UnitState> OnUnitStateChanged;

    private int tier;//表示当前状态机所处的层级，主状态机为1，次一级为2
    public StateMachine(Func<UnitState, IState> stateFactory, BaseUnit unit, int tier)
    {
        this.stateFactory = stateFactory;
        this.unit = unit;
        this.tier = tier;
    }
    private List<TransitionRule> rules = new List<TransitionRule>();
    public void AddRule(TransitionRule rule) => rules.Add(rule);
    public void HandleEvent(StateEvent evt)
    {
        // 找出所有符合当前状态和事件的规则（允许 FromState 为 null 的规则匹配任意状态）
        var applicableRules = rules.Where(r =>
            (r.FromState == null || r.FromState == currentStateEnum) &&
            r.TriggerEvent == evt &&
            (r.Condition == null || r.Condition())).ToList();

        if (applicableRules.Count == 0) return;

        // 按优先级排序，选择最高的规则
        var bestRule = applicableRules.OrderByDescending(r => r.Priority).First();

        // 如果目标状态与当前状态相同，可以忽略（或者根据需求决定是否允许自转换）
        if (bestRule.ToState == currentStateEnum) return;

        TransitionTo(bestRule.ToState);
    }
    /// <summary> 初始化状态机，必须在使用前调用 </summary>
    public void Initialize(UnitState initialState)
    {
        TransitionTo(initialState);
    }
    // 为方便状态类调用，提供一个公开的切换方法
    public void ChangeState(UnitState newState) => TransitionTo(newState);

    public void Update()
    {
        currentState?.Update();
    }
    /// <summary> 切换到指定枚举状态 </summary>
    private void TransitionTo(UnitState newStateEnum)
    {
        currentStateEnum=newStateEnum;
        if (tier == 2)
        {
            unit.mainState.SecondaryState = currentStateEnum;
        }
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
        
        OnUnitStateChanged?.Invoke(unit, newStateEnum);
    }
}
