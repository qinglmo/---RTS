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
        
    }

    public void Update()
    {
        timer += Time.deltaTime;
        // ¿ÕÏÐÂß¼­...

        if (timer >= idleTime)
        {
            timer = 0;
        }
    }

    public void Exit()
    {
        //Debug.Log($"{unit.name}Exit Nothing");
    }

    public void Reset()
    {
        timer = 0f;
    }
}
