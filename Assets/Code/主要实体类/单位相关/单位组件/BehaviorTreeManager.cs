using UnityEngine;

public class BehaviorTreeManager :MonoBehaviour
{
    public BehaviorTree.Blackboard blackboard;
    public BehaviorTree.Node behaviorTree;
    private BaseUnit unit;
    private float executionInterval = 0.1f;
    private float timer=0f;
    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        behaviorTree = unit.CreateTree(out blackboard);
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > executionInterval)
        {
            timer = 0f;
            behaviorTree.Evaluate();
        }
    }
}

