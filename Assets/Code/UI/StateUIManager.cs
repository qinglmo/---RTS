using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateUIManager : MonoBehaviour
{
    private static StateUIManager instance;
    public static StateUIManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void ChangeStateColor(BaseUnit unit,StateMachine stateMachine)
    {
        var sprite =unit.StateSprite;
        if (sprite != null)
        {
            switch (stateMachine.CurrentStateEnum)
            {
                case UnitState.ActionToTarget:
                {
                    switch ((stateMachine.GetState(UnitState.ActionToTarget) as ActionToTargetState).action)
                    {
                        case UnitAction.Attack:
                            sprite.color = Color.red;
                            break;
                        default:
                            sprite.color = Color.green;
                            break;
                    }
                }
                    break;
                case UnitState.MoveToPos:sprite.color = Color.green;
                    break;
                case UnitState.Stunned:sprite.color = Color.yellow;
                    break;
                case UnitState.Idle:sprite.color = Color.gray;
                    break;
                default:sprite.color=Color.white;
                    break;
            }
        }
    }

    
}
