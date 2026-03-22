using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyUnit;
using static StepMover;

public class StunState: IState
{
    private BaseUnit unit;
    private float timer;
    private MainState mainState;

    public StunState(BaseUnit unit, MainState mainState) 
    { 
        this.unit = unit;
        this.mainState = mainState; 
    }

    public void Enter()
    {
        timer = 0f;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= mainState.stunDuration)
        {
            timer = 0;
            mainState.StateMachine.ChangeState(mainState.lastState);
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
