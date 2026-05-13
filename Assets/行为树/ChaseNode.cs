
using UnityEngine;
using static TargetDetector;

namespace BehaviorTree
{
    public class ChaseNode : ActionNode
    {
        public ChaseNode(Blackboard blackboard,string name) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            IHasPosition currentTarget=blackboard.unit.detector.CurrentTarget;
            IFocusTarget focusTarget = blackboard.focusTarget;
            var chaseRange = blackboard.unit.chaseRange_Current;
            var CurrentGridPosition = blackboard.unit.GridPos;
            if (focusTarget.IsTargetInRange(currentTarget, chaseRange, CurrentGridPosition))
            {
                blackboard.TrrigerEvent_AllTier(StateEvent.ChaseTargetFound);
                
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }
}
