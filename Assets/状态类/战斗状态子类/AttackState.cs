using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static EnemyUnit;
using static UnityEngine.RuleTile.TilingRuleOutput;

public  class AttackState : IState
{
    private BaseUnit unit;
    private int attackCount = 0;
    
    private float timer;

    public AttackState(BaseUnit unit) { this.unit = unit; }

    public void Enter()
    {
        attackCount = 0;
        timer = 0f;
        unit.detector.StartDetection();
        unit.movement.CancelMove();
        if (unit.detector.CurrentTarget != null)
        {
            // 执行攻击动画和伤害
            unit.unitVisualEffect.AttackAnimation(unit.detector.CurrentTarget);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= unit.attributes.attackInterval)
        {
            timer = 0f;
            unit.detector.StartDetection();
            if (unit.detector.CurrentTarget != null )
            {
                // 执行攻击动画和伤害
                unit.unitVisualEffect.AttackAnimation(unit.detector.CurrentTarget);
            }
        }
    }

    public void Exit()
    {
    }

    public void Reset()
    {
        attackCount = 0;
        timer = 0f;
        unit.detector.StartDetection();
        unit.movement.CancelMove();
        if (unit.detector.CurrentTarget != null)
        {
            // 执行攻击动画和伤害
            unit.unitVisualEffect.AttackAnimation(unit.detector.CurrentTarget);
        }
    }
}