using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NonCombatState : IBranchState
{
    private StateMachine stateMachine;
    public StateMachine StateMachine { get; }
    private BaseUnit unit;
    
    public NonCombatState(BaseUnit unit)
    {
        this.unit = unit;
    }
    void IState.Enter()
    {
        stateMachine = new StateMachine(stateEnum =>
        {
            return stateEnum switch
            {
                UnitState.Nothing => new NothingState(unit),
                UnitState.Wandering => new WanderStateUnit(unit, stateMachine),

                _ => throw new ArgumentException("无效状态")
            };
        }, unit,2);
        stateMachine.Initialize(UnitState.Nothing);
    }
    void IState.Update()
    {
        stateMachine.Update();
    }

    void IState.Exit()
    {

    }

    public void Reset()
    {

    }
    private void AddRules()
    {
        //攻击状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Attacking, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Attacking, TriggerEvent = StateEvent.ChaseTargetLost, ToState = UnitState.Finding });
        //追击状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Chaseing, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Chaseing, TriggerEvent = StateEvent.ChaseTargetLost, ToState = UnitState.Finding });
        //寻敌状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Finding, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Finding, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        //空闲状态转出
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        stateMachine.AddRule(new TransitionRule { FromState = UnitState.Nothing, TriggerEvent = StateEvent.FriendlyMessage, ToState = UnitState.Finding });
    }
}

