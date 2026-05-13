using UnityEngine;


namespace BehaviorTree
{
    public class SupportNode : ActionNode
    {
        public SupportNode(Blackboard blackboard, string name) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            if(blackboard.unit.detector.SupportTarget!=null)
            {
                blackboard.TrrigerEvent_AllTier(StateEvent.SupportFriendly);
                return NodeState.Success;
            }
            return NodeState.Failure;

        }
    }

}
