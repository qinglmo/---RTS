using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;

public class NothingState : IState
{
    private BaseUnit unit;
    private float idleTime = 0.5f;
    private float timer;
    public NothingState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {
        //Debug.Log($"{unit.name }Enter Nothing");
        timer = 0f;
        if(unit.mainState.StateMachine.CurrentStateEnum is UnitState.Top_Attack)
            unit.detector.OpenFriendlyDetector();//∆Ù∂Ø÷ß‘ÆºÏ≤‚
    }

    public void Update()
    {
        timer += Time.deltaTime;
        // ø’œ–¬ﬂº≠...

        if (timer >= idleTime)
        {
            timer = 0;
        }
    }

    public void Exit()
    {
        unit.detector.CloseFriendlyDetector();//πÿ±’÷ß‘ÆºÏ≤‚
    }

    public void Reset()
    {
        Enter();
    }
}
