using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyUnit : BaseUnit
{
    
    protected override void Awake()
    {
        base.Awake();           
    }
    protected override void Start()
    {
        base.Start();
        
        attributes.OnTakeDamageChanged+=CancelMove;

    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (attributes != null)
        {
            attributes.OnTakeDamageChanged -= CancelMove;
        }
            
    }
    public void DamageCalculation(IHasPosition target)
    {
        if(target as MonoBehaviour == null)
        {
            return;
        }
        if (target is BaseUnit)
        {
            var unit = (BaseUnit)target;
            unit.attributes.TakeDamage(attributes.BaseAttack, 0.5f);
        }
        if(target is BaseBuilding)
        {
            BaseBuilding baseBuilding = target as BaseBuilding;
            baseBuilding.TakeDamage(attributes.BaseAttack);
        }
        
    }
    private void CancelMove()
    {
        movement.CancelMove();
    }
}
