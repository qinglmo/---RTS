using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAction{Attack,Help,Collect}
public static  class ActionCompleted 
{
    public static void ActionExecute(UnitAction action,BaseUnit unit,IHasPosition target)
    {
        switch (action)
        {
            case UnitAction.Attack:
                {
                    if(unit is Unit)
                    {
                        (unit as Unit).ShootAt(target.Position);
                    }
                    else if(target is BaseUnit)
                        AttackAction(unit, target as BaseUnit);
                    else if(target is BaseBuilding)
                    {
                        AttackAction_Building(unit, target as BaseBuilding);
                    }
                    break;
                }
                
            case UnitAction.Help:
                HelpAction(unit, target as BaseUnit);
                break;
            // case UnitAction.Collect:
            //     CollectAction(unit, target as IResource);
            //     break;
            default:
                break;
        }
    }

    private static void AttackAction(BaseUnit unit,BaseUnit target)
    {
        target.attributes.TakeDamage(unit.attributes.BaseAttack,0.1f);
    }
    private static void AttackAction_Building(BaseUnit unit,BaseBuilding target)
    {
        target.TakeDamage(unit.attributes.BaseAttack);
    }
    private static void HelpAction(BaseUnit unit,BaseUnit target)
    {
        target.attributes.Heal(1);//目前没有这个属性，所以只能临时处理为1
        SpecialEffectsManager.instance.CreateHealEffect(target,1);
    }
}
