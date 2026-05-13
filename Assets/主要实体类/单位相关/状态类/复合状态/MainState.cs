using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class MainState : MonoBehaviour
{
    private StateMachine stateMachine;
    public StateMachine StateMachine { get { return stateMachine; } }
    private BaseUnit unit;
    public RuleGroup transitionRules;
    public UnitState initializeState;//初始状态
    public MainState(BaseUnit unit)
    {
        this.unit = unit;
    }
    private Action onEscapeEvent;
    private Action onLifeFullyRestored;
    private Action<float> onStunBegin;

    //眩晕用字段
    public float stunDuration;

    [SerializeField]private UnitState main_State;
    [SerializeField]private UnitState secondaryState;//次级状态
    public UnitState Main_State
    {
        get { return main_State; }
        set { main_State = value;OnMainStateChanged?.Invoke(main_State); }
    }
    public Action<UnitState> OnMainStateChanged;
    public UnitState SecondaryState
    {
        get { return secondaryState; }
        set { secondaryState = value;OnSecondaryStateChanged?.Invoke(secondaryState); }
    }
    public Action<UnitState> OnSecondaryStateChanged;

    public void Initialize(BaseUnit unit)
    {
        this.unit = unit;
        stateMachine = new StateMachine(stateEnum =>//对象池化后只会创建一次，所有动态数据不要通过这个创建
        {
            return stateEnum switch
            {
                UnitState.Top_Attack => new CombatState(unit),
                UnitState.Top_Stun => new StunState(unit, this),
                UnitState.Top_Work => new NonCombatState(unit),
                UnitState.Top_Heal => new EscapingState(unit, stateMachine),
                UnitState.Top_Advance => new Attack_Route(unit),
                _ => throw new ArgumentException("无效状态")
            };
        }, unit, 1);
        //订阅事件
        // 可能抛出异常的代码
        stateMachine.OnUnitStateChanged += StateUIManager.Instance.ChangeStateColor;
        onEscapeEvent += () => stateMachine.HandleEvent(StateEvent.EscapeTriggered);
        onLifeFullyRestored += () => stateMachine.HandleEvent(StateEvent.LifeFullyRestored);
        onStunBegin += (float stunDuration) =>
        {
            this.stunDuration = stunDuration;
            stateMachine.PushState(UnitState.Top_Stun);
        };
        unit.attributes.OnEscapeEvent += onEscapeEvent;
        unit.attributes.OnStunned += onStunBegin;
        unit.attributes.OnLifeFullyRestored += onLifeFullyRestored;
        stateMachine.Initialize(initializeState);
        AddRules();
    }
    public void Update()
    {
        stateMachine.Update();
    }
    private void OnDestroy()
    {
        stateMachine.OnUnitStateChanged -= StateUIManager.Instance.ChangeStateColor;
        unit.attributes.OnEscapeEvent -= onEscapeEvent;
        unit.attributes.OnStunned -= onStunBegin;
        unit.attributes.OnLifeFullyRestored -= onLifeFullyRestored;
        //如果未来生命周期变动，记得管理状态机，状态机持有子状态，子状态可能有订阅，需要取消订阅。
    }
    private void AddRules()
    {
        foreach(var rule in transitionRules.rules)
        {
            stateMachine.AddRule(rule);
        }
        ////目前逻辑还不完善，更多支持战斗模块
        ////战斗状态转出
        //stateMachine.AddRule(new TransitionRule { FromState=UnitState.Top_Attack,TriggerEvent=StateEvent.EscapeTriggered,ToState=UnitState.Top_Heal} );
        //stateMachine.AddRule(new TransitionRule { FromState = UnitState.Top_Attack, TriggerEvent = StateEvent.StunBegin, ToState = UnitState.Top_Stun });
        ////恢复状态转出
        //stateMachine.AddRule(new TransitionRule { FromState = UnitState.Top_Heal, TriggerEvent = StateEvent.StunBegin, ToState = UnitState.Top_Stun });
        //stateMachine.AddRule(new TransitionRule { FromState = UnitState.Top_Heal, TriggerEvent = StateEvent.LifeFullyRestored, ToState = UnitState.Top_Attack });
        ////工作状态转出
        //stateMachine.AddRule(new TransitionRule { FromState = UnitState.Top_Work, TriggerEvent = StateEvent.EscapeTriggered, ToState = UnitState.Top_Heal });
        ////眩晕状态转出，眩晕状态内部自己处理
    }
}
