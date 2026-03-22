using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum UnitState
{
    Top_Attack,//战斗状态
    Top_Heal,//恢复状态，残血从战斗状态进入恢复状态，目前是逃跑状态。需要屏蔽发现敌人，直到满血才退出该状态。
    Top_Work,//非战斗状态
    Top_Stun,//眩晕状态


    Chaseing,   // 追击
    Attacking,   // 攻击
    Finding,//寻找敌人最后消失的位置
    //战斗状态也持有空闲状态，由玩家切换是否参与战斗，让空闲状态可以平滑切换到追击或者攻击或者寻敌状态。

    Escaping,//逃跑状态

    Wandering,   // 游荡
    Nothing,     //空闲状态。
    Working,

    Moving,      // 被命令移动中
    Stunned   // 新增眩晕状态
}
public enum StateEvent
{
    AttackTargetFound,   // 发现可攻击目标
    ChaseTargetFound,    // 发现可追击目标
    ChaseTargetLost,     // 追击目标丢失
    EscapeTriggered,     // 触发逃跑（如血量过低）
    StunBegin,           // 眩晕开始
    LifeFullyRestored,//生命完全恢复
    FriendlyMessage,//友军消息
    MoveCommandReceived, // 收到移动命令
    IdleTimeout,         // 空闲超时（用于从Nothing/Finding自动切换）
    // 你可以根据需要继续添加
}
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
    private Action onAttackTargetFound;
    private Action onChaseTargetFound;
    private Action onChaseTargetLost;
    private Action onEscapeEvent;
    private Action onLifeFullyRestored;
    private Action<float> onStunBegin;

    //眩晕用字段
    public float stunDuration;
    public UnitState lastState;//眩晕后记录当前状态

    private UnitState secondaryState;//次级状态
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
                _ => throw new ArgumentException("无效状态")
            };
        }, unit, 1);
        //订阅事件
        // 可能抛出异常的代码
        stateMachine.OnUnitStateChanged += StateUIManager.Instance.ChangeStateColor;
        onAttackTargetFound += () => stateMachine.HandleEvent(StateEvent.AttackTargetFound);
        onChaseTargetFound += () => stateMachine.HandleEvent(StateEvent.ChaseTargetFound);
        onChaseTargetLost += () => stateMachine.HandleEvent(StateEvent.ChaseTargetLost);
        onEscapeEvent += () => stateMachine.HandleEvent(StateEvent.EscapeTriggered);
        onLifeFullyRestored += () => stateMachine.HandleEvent(StateEvent.LifeFullyRestored);
        onStunBegin += (float stunDuration) =>
        {
            this.stunDuration = stunDuration;
            lastState = stateMachine.CurrentStateEnum;
            stateMachine.HandleEvent(StateEvent.StunBegin);
        };
        unit.detector.OnAttackTargetFound += onAttackTargetFound;
        unit.detector.OnChaseTargetFound += onChaseTargetFound;
        unit.detector.OnChaseTargetLost += onChaseTargetLost;
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
        unit.detector.OnAttackTargetFound -= onAttackTargetFound;
        //unit.detector.OnChaseTargetFound -= onChaseTargetFound;//目前生命周期相同，不需要特别取消订阅
        //unit.detector.OnChaseTargetLost -= onChaseTargetLost;
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
