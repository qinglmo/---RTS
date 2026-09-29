namespace BehaviorTree
{
    public class AdvancementNode : ActionNode
    {
        public AdvancementNode(Blackboard blackboard, string name) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            if(blackboard.unit.AttackTarget!=null) 
                blackboard.TrrigerEvent_AllTier(StateEvent.TargetAdvancement);
            else
                return NodeState.Failure;
            return NodeState.Success;

        }
    }

}