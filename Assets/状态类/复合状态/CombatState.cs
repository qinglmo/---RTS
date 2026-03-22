using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CombatState : IBranchState
{
    private StateMachine stateMachine;
    public StateMachine StateMachine { get; }
    private BaseUnit unit;
    private Action onAttackTargetFound;
    private Action onChaseTargetFound;
    private Action onChaseTargetLost;
    private Action onFriendlyMessage;
    public CombatState(BaseUnit unit)
    {
        this.unit = unit;
    }
    public void Reset()
    {
        unit.detector.OnAttackTargetFound += onAttackTargetFound;
        unit.detector.OnChaseTargetFound += onChaseTargetFound;
        unit.detector.OnChaseTargetLost += onChaseTargetLost;
        unit.detector.OnFriendlyMessage += onFriendlyMessage;
    }
    void IState.Enter()
    {
        stateMachine = new StateMachine(stateEnum =>
        {
            return stateEnum switch
            {

                UnitState.Attacking => unit is EnemyUnit ? new AttackState(unit) : new ShootAttackState(unit as Unit),
                UnitState.Chaseing => new ChaseState(unit),
                UnitState.Finding => new FindingState(unit,stateMachine),
                UnitState.Nothing => new NothingState(unit),
                _ => throw new ArgumentException("无效状态")
            };
        }, unit,2);
        
        onAttackTargetFound = () => stateMachine.HandleEvent(StateEvent.AttackTargetFound);
        onChaseTargetFound += () => stateMachine.HandleEvent(StateEvent.ChaseTargetFound);
        onChaseTargetLost += () => stateMachine.HandleEvent(StateEvent.ChaseTargetLost);
        onFriendlyMessage += () => stateMachine.HandleEvent(StateEvent.FriendlyMessage);
        unit.detector.OnAttackTargetFound += onAttackTargetFound;
        unit.detector.OnChaseTargetFound += onChaseTargetFound;
        unit.detector.OnChaseTargetLost += onChaseTargetLost;
        unit.detector.OnFriendlyMessage += onFriendlyMessage;
        stateMachine.Initialize(UnitState.Nothing);
        AddRules();
    }
    void IState.Update()
    {
        stateMachine.Update();
    }

    void IState.Exit()
    {
        unit.detector.OnAttackTargetFound -= onAttackTargetFound;
        unit.detector.OnChaseTargetFound -= onChaseTargetFound;
        unit.detector.OnChaseTargetLost -= onChaseTargetLost;
        unit.detector.OnFriendlyMessage -= onFriendlyMessage;
    }
    private void AddRules()
    {
        //攻击状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Attacking, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Attacking, TriggerEvent = StateEvent.ChaseTargetLost, ToState = UnitState.Finding });
        //追击状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Chaseing, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Chaseing, TriggerEvent = StateEvent.ChaseTargetLost,ToState = UnitState.Finding });
        //寻敌状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Finding, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Finding, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        //空闲状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.FriendlyMessage, ToState = UnitState.Finding });
    }
}

