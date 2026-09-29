using UnityEngine;
namespace BehaviorTree
{
    public class NothingNode : ActionNode
    {
        public enum StateTier
        {
            MainState,
            BranchState,
        }
        private StateTier tier;
        public NothingNode(Blackboard blackboard, string name,StateTier tier=StateTier.MainState) : base(name, blackboard) { this.tier=tier; }
        protected override NodeState OnEvaluate()
        {
            if (tier == StateTier.MainState)
            {
                blackboard.TrrigerEvent_MainState(StateEvent.Idle);
            }
            else if (tier == StateTier.BranchState)
            {
                blackboard.TriggerEvent_BranchState(StateEvent.Idle);
            }
            return NodeState.Success;

        }
    }

}