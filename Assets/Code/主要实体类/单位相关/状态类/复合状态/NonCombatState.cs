using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class NonCombatState : BaseBranchState
{
    public NonCombatState(BaseUnit unit) : base(unit) { }
    public override void Enter()
    {
        stateMachine = new StateMachine(stateEnum =>
        {
            return stateEnum switch
            {
                UnitState.Nothing => new NothingState(unit),
                UnitState.ApprochResource=>new ApproachState(unit,stateMachine),
                UnitState.Working=>new WorkState(unit),
                UnitState.Wandering => new WanderStateUnit(unit, stateMachine),
                UnitState.ReturnToBase=>new ApproachState(unit,stateMachine),
                _ => throw new ArgumentException("无效状态")
            };
        }, unit,2);
        stateMachine.Initialize(UnitState.Nothing);
        AddRules();
    }
    public override void Update()
    {
        stateMachine.Update();
    }

    private void AddRules()
    {
        //返回基地转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.ReturnToBase, ToState = UnitState.ReturnToBase });
        //接近资源转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.CloseToResources, ToState = UnitState.ApprochResource });
        //工作转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.CollectResources, ToState = UnitState.Working });
        //空闲状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.Idle, ToState = UnitState.Nothing });
        //闲逛状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.SupportFriendly, ToState = UnitState.Wandering });
    }
}

