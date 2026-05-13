using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static EnemyUnit;
using static UnityEngine.RuleTile.TilingRuleOutput;

public  class AttackState : IState
{
    private EnemyUnit unit;
    private int attackCount = 0;
    
    private float timer;
    private bool hasAttacked=false;
    public AttackState(EnemyUnit unit) { this.unit = unit; }

    public void Enter()
    {
        attackCount = 0;
        timer = 0f;
        hasAttacked=false;
        unit.detector.StartDetection();
        unit.movement.CancelMove();
        if (unit.detector.CurrentTarget != null)
        {
            // 执行攻击动画
            unit.unitVisualEffect.AttackWindup(unit.detector.CurrentTarget.GridPos-unit.GridPos, 0.1f);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= 0.1f&& hasAttacked==false)//攻击前摇结束，执行攻击
        {
            unit.detector.StartDetection();
            if (unit.detector.CurrentTarget != null)
            {
                unit.DamageCalculation(unit.detector.CurrentTarget);
                hasAttacked = true;
            }
            // 执行攻击后摇动画
            unit.unitVisualEffect.AttackRecovery(0.1f);

        }

        if (timer >= unit.attributes.attackInterval)
        {
            timer = 0f;
            hasAttacked = false;
            if (unit.detector.CurrentTarget != null)
            {
                // 执行攻击动画
                unit.unitVisualEffect.AttackWindup(unit.detector.CurrentTarget.GridPos - unit.GridPos, 0.1f);
            }
        }
    }

    public void Exit()
    {
        // 攻击退出动画
        unit.unitVisualEffect.AttackRecovery(0.1f);
    }

    public void Reset()
    {
        Enter();
    }
}