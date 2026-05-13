using UnityEngine;
namespace BehaviorTree
{
    public class Attack_RouteNode : ActionNode
    {
        public Attack_RouteNode(Blackboard blackboard, string name="推进节点") : base(name, blackboard)
        { }

        protected override NodeState OnEvaluate()
        {
            if(blackboard.unit.AttackTarget!=null)
            {
                blackboard.TrrigerEvent_AllTier(StateEvent.Top_Advance);//进入推进任务
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }
}