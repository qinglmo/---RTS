using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CombatState : BaseBranchState
{
    private Action onFriendlyMessage;
    public CombatState(BaseUnit unit) : base(unit) { }
    public override void Enter()
    {
        stateMachine = new StateMachine(stateEnum =>
        {
            return stateEnum switch
            {

                UnitState.Attacking => unit is EnemyUnit ? new AttackState(unit as EnemyUnit) : new ShootAttackState(unit as Unit),
                UnitState.Chaseing => new ApproachState(unit,stateMachine),
                UnitState.Finding => new FindingState(unit,stateMachine),
                UnitState.Supporting=>new ApproachState(unit,stateMachine),
                UnitState.TargetAdvancement=>new ApproachState(unit,stateMachine),
                UnitState.Nothing => new NothingState(unit),

                _ => throw new ArgumentException("无效状态")
            };
        }, unit,2);
        stateMachine.Initialize(UnitState.Nothing);
        AddRules();
        unit.chaseRange_Current = unit.chaseRange_Normal;
    }
    public override void Reset()
    {
        base.Reset();
        unit.chaseRange_Current = unit.chaseRange_Normal;
    }
    private void AddRules()
    {
        //攻击状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue=false, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        //追击状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        //寻敌状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.FindTarget, ToState = UnitState.Finding });
        //空闲状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.Idle, ToState = UnitState.Nothing });
        //新增支援状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.SupportFriendly, ToState = UnitState.Supporting });
        //推进状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.TargetAdvancement, ToState = UnitState.TargetAdvancement });
    }

}

