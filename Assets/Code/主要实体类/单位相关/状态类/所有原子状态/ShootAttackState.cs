using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAttackState : IState
{
    private Unit unit;
    private float aimTimer;
    private float coolTimer;
    private bool isShoot;//false��ʾ������׼��true��ʾ��Ҫ��ȴ
    [Header("�Զ���������")]
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
    //     if(!isShoot && unit.detector.CurrentTarget as MonoBehaviour!=null) 
    //         aimTimer += Time.deltaTime;
    //     else if(isShoot)
    //         coolTimer += Time.deltaTime;
    //     if (aimTimer > aimTime)
    //     {
    //         aimTimer = 0;
    //         isShoot=true;
    //         if(unit.detector.CurrentTarget as MonoBehaviour !=null)
    //             unit.ShootAt(unit.detector.CurrentTarget.Position);//�ӵ����䣬�����ں�Ŀ���Ƿ���ڡ�
    //     }
    //     if (coolTimer > attackCooldown)
    //     {
    //         coolTimer = 0;
    //         isShoot = false;
    //     }
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
