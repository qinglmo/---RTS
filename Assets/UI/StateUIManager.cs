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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void ChangeStateColor(BaseUnit unit,UnitState state)
    {
        var sprite =unit.StateSprite;
        if (sprite != null)
        {
            switch (state)
            {
                case UnitState.Top_Work:sprite.color = Color.green;
                    break;
                case UnitState.Top_Attack:sprite.color = Color.red;
                    break;
                case UnitState.Chaseing:sprite.color = Color.yellow;
                    break;
                case UnitState.Wandering:sprite.color = Color.blue;
                    break;
                case UnitState.Top_Stun:sprite.color = Color.black;
                    break;
                case UnitState.Top_Heal:sprite.color = Color.gray;
                    break;
                default:sprite.color=Color.white;
                    break;
            }
        }
    }
}
