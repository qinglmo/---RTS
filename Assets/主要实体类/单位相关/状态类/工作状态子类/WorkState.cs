using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class WorkState : IState
{
    private WorkUnit unit;
    private float timer;
    private float inspectionInterval = 1f;//检查间隔
    public WorkState(BaseUnit unit) { this.unit = unit as WorkUnit; }

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
            timer = 0f;
            if(unit.currentResourceTarget!=null)
            {
                var(type,num)=unit.currentResourceTarget.GatherResource();
                unit.resourceType = type;
                unit.GatherNum += num;
            }
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


