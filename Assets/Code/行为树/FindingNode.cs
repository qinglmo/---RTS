using UnityEngine;


namespace BehaviorTree
{
    public class FindingNode : ActionNode
    {
        public FindingNode( Blackboard blackboard,string name) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            if (blackboard.lastPos != null)
            {
                blackboard.TrrigerEvent_AllTier(StateEvent.FindTarget);//当前事件通常会让状态机转向搜寻状态
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }

}
