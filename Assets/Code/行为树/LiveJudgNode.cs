using UnityEngine;
using static TargetDetector;


namespace BehaviorTree
{
    /// <summary>
    /// 该节点检查目标是否存活或者隐藏，存活则表示执行成功，否则执行失败并然后清空目标
    /// </summary>
    public class LiveJudgNode : ActionNode
    {
        public LiveJudgNode(Blackboard blackboard, string name) : base(name, blackboard) { }
        protected override NodeState OnEvaluate()
        {
            IHasPosition currentTarget=blackboard.CurrentTarget;
            var mono = currentTarget as MonoBehaviour;//隐藏检查
            if (mono != null)
            {
                if (mono.gameObject.activeInHierarchy == false)
                {
                    return NodeState.Failure;
                }
                return NodeState.Success;
            }
            else
            {
                return NodeState.Failure;
            }
        }
    }
}
