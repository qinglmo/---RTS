using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class ChaseState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval=1f;//检查间隔
    public ChaseState(BaseUnit unit) { this.unit = unit; }

    public void Enter()//追击状态的原则就是不断接近对方。
    {
        timer = 0f;
    }

    public void Update()
    {
        // 累加时间
        timer += Time.deltaTime;
        if (timer > inspectionInterval)
        {
            timer=0f;
            if(unit.detector.CurrentTarget as MonoBehaviour==null)
                return;
            var tar = (Vector2Int)unit.detector.CurrentTarget.GridPos;
            RouteDecision.TargetApproaching(unit, tar);
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


