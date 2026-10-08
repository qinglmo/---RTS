using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class IdleState : IState
{
    private BaseUnit unit;
    private float idleTime = 0.5f;
    private float timer;
    public bool IsChangeState { get; private set; } = true;
    public IdleState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {

    }

    public void Update()
    {
        timer += Time.deltaTime;
        // 空闲逻辑...

        if (timer >= idleTime)
        {
            timer = 0;
        }
    }

    public void Exit()
    {
    }

    public void Reset()
    {
        Enter();
    }
}
