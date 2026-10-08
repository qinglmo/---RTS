using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;

public class StunState: IState
{
    private BaseUnit unit;
    private float timer;

    public bool IsChangeState => true;
    public StunState(BaseUnit unit) 
    { 
        this.unit = unit;
    }

    public void Enter()
    {
        timer = 0f;
    }

    public void Update()
    {
        timer += Time.deltaTime;

    }

    public void Exit()
    {
    }

    public void Reset()
    {
        timer = 0f;
    }
}
