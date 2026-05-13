using UnityEngine;

namespace BehaviorTree
{

    public class AttackNode : ActionNode
    {
        public enum RangeMode
        {
            Circle,
            Line
        }
        public RangeMode attackRangeMode;// 攻击范围判定方式
        public AttackNode(Blackboard blackboard,string name ) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            int mask = LayerMask.GetMask("Mountain", "Building");
            IHasPosition currentTarget = blackboard.unit.detector.CurrentTarget;
            IFocusTarget focusTarget = blackboard.focusTarget;
            ITargetFiltering targetFiltering = blackboard.targetFiltering;
            var attackRange = blackboard.attackRange;
            var CurrentGridPosition = blackboard.unit.GridPos;
            var unit = blackboard.unit;
            if (focusTarget.IsTargetInRange(currentTarget, attackRange, CurrentGridPosition, attackRangeMode)
            && targetFiltering.IsVisiableToOne(currentTarget, mask))
            {
                if (!unit.movement.stepMover.IsOccupy)
                    return NodeState.Failure;
                blackboard.TrrigerEvent_AllTier(StateEvent.AttackTargetFound);
                //无脑发送即可，接收端自行处理转换，该事件的意思就是提供需要进入攻击状态或者保持攻击状态的信息。
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }
}
