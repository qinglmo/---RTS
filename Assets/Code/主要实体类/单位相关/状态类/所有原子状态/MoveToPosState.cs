using System.Collections;
using UnityEngine;
using static Unit;

public class MoveToPosState : IState
{
    private BaseUnit unit;
    private IStateMachine stateMachine;
    private float timer;
    public bool IsChangeState { get { return true; } }
    public float Interval = 0.5f;

    public Vector2Int targetPos;
    public MoveToPosState(BaseUnit unit, IStateMachine stateMachine)
    { 
        this.unit = unit;
        this.stateMachine = stateMachine;
    }
    public void Enter()
    {
        timer = 0f;
        RouteDecision.UniversalRouteDecision(unit, targetPos);
    }

    public void Update()
    {
        if(unit.GridPos==targetPos)
        {
            stateMachine.ChangeState(UnitState.Idle);
        }
        timer += Time.deltaTime;
        if(timer>=Interval)//重试防止路径阻塞
        {
            timer = 0f;
            RouteDecision.UniversalRouteDecision(unit, targetPos);
        }
        
    }
    public void Exit()
    {
        
    }

    public void Reset()
    {
        timer = 0f;
    }

    public void SetPos(Vector2Int pos)
    {
        targetPos = pos;
    }
    public bool IsEqualState(Vector2Int pos)
    {
        return targetPos==pos;
    }
}