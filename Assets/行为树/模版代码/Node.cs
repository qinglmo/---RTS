using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;

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
        protected Blackboard blackboard;   // 新增黑板引用
        public Node(string name = "Node", Blackboard blackboard = null)
        {
            this.name = name;
            this.blackboard = blackboard;
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

        public Sequence(string name = "Sequence", Blackboard blackboard = null) : base(name, blackboard) { }

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

        public Selector( Blackboard blackboard= null,string name = "Selector") : base(name, blackboard) { }
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
    /// <summary>
    /// 决策树 - 并行节点
    /// 核心：一次性执行所有子节点，无Running状态，仅返回成功/失败
    /// </summary>
    public class Parallel : Node
    {
        // 子节点列表（决策树并行：必须执行所有子节点）
        private List<Node> children = new List<Node>();
        
        // 并行判定策略：
        // true = 所有子节点成功 → 整体成功
        // false = 任意一个子节点成功 → 整体成功
        private bool requireAllSuccess = true;

        // 构造函数
        public Parallel(Blackboard blackboard = null, string name = "Parallel", bool requireAllSuccess = true)
            : base(name, blackboard)
        {
            this.requireAllSuccess = requireAllSuccess;
        }

        // 链式添加子节点
        public Parallel AddChild(Node child)
        {
            if (child != null)
                children.Add(child);
            return this;
        }

        /// <summary>
        /// 决策树核心：一次性执行所有子节点，无Running，直接返回最终结果
        /// </summary>
        public override NodeState Evaluate()
        {
            // 安全判断：无子节点默认成功
            if (children.Count == 0)
                return NodeState.Success;

            int successCount = 0;

            // 🔥 决策树并行：强制执行【所有子节点】，不跳过、不缓存
            foreach (var child in children)
            {
                NodeState result = child.Evaluate();
                // 只统计成功（决策树无Running，子节点仅返回Success/Failure）
                if (result == NodeState.Success)
                    successCount++;
            }

            // 按策略返回最终结果（无任何Running状态）
            if (requireAllSuccess)
            {
                // 策略1：所有子节点都成功 → 整体成功
                return successCount == children.Count ? NodeState.Success : NodeState.Failure;
            }
            else
            {
                // 策略2：任意一个子节点成功 → 整体成功
                return successCount > 0 ? NodeState.Success : NodeState.Failure;
            }
        }

        // 决策树无状态，重置仅保留空实现（兼容父类）
        public override void Reset()
        {
            foreach (var child in children)
                child.Reset();
        }
    }
    // 装饰节点：反转子节点结果（Success <-> Failure，Running不变）
    public class Inverter : Node
    {
        private Node child;

        public Inverter(Node child, string name = "Inverter", Blackboard blackboard = null) : base(name, blackboard) 
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
        protected ActionNode(string name, Blackboard blackboard) : base(name, blackboard) { }

        public sealed override NodeState Evaluate()
        {
            return OnEvaluate();
        }

        protected abstract NodeState OnEvaluate();
    }

    // 条件节点基类：用户需继承并实现OnEvaluate
    public abstract class ConditionNode : Node
    {
        protected ConditionNode(string name, Blackboard blackboard = null) : base(name, blackboard) { }

        public sealed override NodeState Evaluate()
        {
            return OnEvaluate() ? NodeState.Success : NodeState.Failure;
        }

        protected abstract bool OnEvaluate();
    }
}