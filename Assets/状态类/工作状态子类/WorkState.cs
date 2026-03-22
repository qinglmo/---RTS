using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class WorkState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval = 1f;//检查间隔
    public WorkState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {
        timer = 0f;
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            
        }
    }

    public void Exit()
    {

    }

    public void Reset()
    {
        timer = 0f;
    }
}


