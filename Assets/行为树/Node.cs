using System.Collections.Generic;

namespace BehaviorTree
{
    // 行为节点执行状态
    public enum NodeState
    {
        Success,
        Failure,
        Running
    }

    // 行为节点基类
    public abstract class Node
    {
        protected string name;
        public Node(string name = "Node")
        {
            this.name = name;
        }

        // 执行节点，返回状态
        public abstract NodeState Evaluate();

        // 可选：重置节点（用于Running状态的处理）
        public virtual void Reset() { }
    }

    // 组合节点：顺序执行子节点，全部成功则成功，任一失败则失败
    public class Sequence : Node
    {
        private List<Node> children = new List<Node>();
        private int currentChildIndex = 0;

        public Sequence(string name = "Sequence") : base(name) { }

        public Sequence AddChild(Node child)
        {
            children.Add(child);
            return this;
        }

        public override NodeState Evaluate()
        {
            // 从当前索引开始执行
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                NodeState state = children[i].Evaluate();
                switch (state)
                {
                    case NodeState.Running:
                        currentChildIndex = i;
                        return NodeState.Running;
                    case NodeState.Failure:
                        currentChildIndex = 0;
                        return NodeState.Failure;
                    case NodeState.Success:
                        continue;
                }
            }
            currentChildIndex = 0;
            return NodeState.Success;
        }

        public override void Reset()
        {
            currentChildIndex = 0;
            foreach (var child in children)
                child.Reset();
        }
    }

    // 组合节点：选择执行子节点，任一成功则成功，全部失败则失败
    public class Selector : Node
    {
        private List<Node> children = new List<Node>();
        private int currentChildIndex = 0;

        public Selector(string name = "Selector") : base(name) { }

        public Selector AddChild(Node child)
        {
            children.Add(child);
            return this;
        }

        public override NodeState Evaluate()
        {
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                NodeState state = children[i].Evaluate();
                switch (state)
                {
                    case NodeState.Running:
                        currentChildIndex = i;
                        return NodeState.Running;
                    case NodeState.Success:
                        currentChildIndex = 0;
                        return NodeState.Success;
                    case NodeState.Failure:
                        continue;
                }
            }
            currentChildIndex = 0;
            return NodeState.Failure;
        }

        public override void Reset()
        {
            currentChildIndex = 0;
            foreach (var child in children)
                child.Reset();
        }
    }

    // 装饰节点：反转子节点结果（Success <-> Failure，Running不变）
    public class Inverter : Node
    {
        private Node child;

        public Inverter(Node child, string name = "Inverter") : base(name)
        {
            this.child = child;
        }

        public override NodeState Evaluate()
        {
            NodeState state = child.Evaluate();
            if (state == NodeState.Success) return NodeState.Failure;
            if (state == NodeState.Failure) return NodeState.Success;
            return NodeState.Running;
        }

        public override void Reset()
        {
            child.Reset();
        }
    }

    // 动作节点基类：用户需继承并实现OnEvaluate
    public abstract class ActionNode : Node
    {
        protected ActionNode(string name) : base(name) { }

        public sealed override NodeState Evaluate()
        {
            return OnEvaluate();
        }

        protected abstract NodeState OnEvaluate();
    }

    // 条件节点基类：用户需继承并实现OnEvaluate
    public abstract class ConditionNode : Node
    {
        protected ConditionNode(string name) : base(name) { }

        public sealed override NodeState Evaluate()
        {
            return OnEvaluate() ? NodeState.Success : NodeState.Failure;
        }

        protected abstract bool OnEvaluate();
    }
}