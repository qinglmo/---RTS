using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HeroUnit : BaseUnit
{
    UnitSkillsManager UnitSkillsManager { get; set; }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        attributes.Initialize(100, 100, 1, 1);
        unitVisualEffect.OnAttackAnimationCompleted += DamageCalculation;
        attributes.OnTakeDamageChanged += CancelMove;

        UnitSkillsManager=GetComponent<UnitSkillsManager>();
    }

    protected override void OnDestroy()
    {
    }

    private void DamageCalculation(IFactionMember target)
    {
        if (target == null)
        {
            return;
        }
        if (target is BaseUnit)
        {
            var unit = (BaseUnit)target;
            unit.attributes.TakeDamage(attributes.BaseAttack, 0.5f);
        }
        if (target is BaseBuilding)
        {
            BaseBuilding baseBuilding = target as BaseBuilding;
            baseBuilding.TakeDamage(attributes.BaseAttack);
        }
    }
    private void CancelMove()
    {
        movement.CancelMove();
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        Debug.Log("œ‘ æ¡À");
        if (isSelected)
            SkillUI.Instance.SetUnit(UnitSkillsManager);
        else
            SkillUI.Instance.ClearSkills();
        
    }

}
