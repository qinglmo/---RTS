using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAttackState : IState
{
    private Unit unit;
    private float aimTimer;
    private float coolTimer;
    private bool isShoot;//false表示可以瞄准，true表示需要冷却
    [Header("自动攻击设置")]
    public float aimTime = 1f;
    public float attackCooldown = 1f;
    public ShootAttackState(Unit unit) { this.unit = unit; }
    public void Enter()
    {
        aimTimer = 0f;
        coolTimer = 0f;
        isShoot = false;
        unit.movement.CancelMove();
    }
    public void Update()
    {
        if(!isShoot && unit.detector.CurrentTarget as MonoBehaviour!=null) 
            aimTimer += Time.deltaTime;
        else if(isShoot)
            coolTimer += Time.deltaTime;
        if (aimTimer > aimTime)
        {
            aimTimer = 0;
            isShoot=true;
            if(unit.detector.CurrentTarget as MonoBehaviour !=null)
                unit.ShootAt(unit.detector.CurrentTarget.Position);//子弹发射，并不在乎目标是否存在。
        }
        if (coolTimer > attackCooldown)
        {
            coolTimer = 0;
            isShoot = false;
        }
    }
    public void Exit()
    {
        
    }

    public void Reset()
    {
        aimTimer = 0f;
        coolTimer = 0f;
        isShoot = false;
        unit.movement.CancelMove();
    }
}
