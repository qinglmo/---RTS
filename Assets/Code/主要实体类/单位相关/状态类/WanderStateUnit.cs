using System.Collections;
using UnityEngine;
using static Unit;

public class WanderStateUnit : IState
{
    private BaseUnit unit;
    private IStateMachine stateMachine;
    private float timer;
    private bool isMoving;
    [Header("游荡设置")]
    public float wanderInterval = 1f;
    public float wanderRadius = 5f;
    public WanderStateUnit(BaseUnit unit,IStateMachine stateMachine)
    { 
        this.unit = unit;this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        timer = 0f;
        isMoving = false;
    }

    public void Update()
    {
        if (isMoving)
        {

        }
        else
        {
        }
    }
    public void Exit()
    {

    }
    // 复制自 Unit 类的辅助方法
    private Vector2Int GetRandomWanderTarget()
    {
        Vector2Int current = unit.movement.stepMover.CurrentPos;
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            Vector2 worldPoint = (Vector2)unit.transform.position + randomOffset;
            Vector2 snapped = GridManager.Instance.WorldToCell(worldPoint);
            Vector2Int grid = new Vector2Int(Mathf.RoundToInt(snapped.x), Mathf.RoundToInt(snapped.y));
            if (grid != current)
                return grid;
        }
        return current + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
    }

    public void Reset()
    {
        timer = 0f;
        isMoving = false;
    }
}