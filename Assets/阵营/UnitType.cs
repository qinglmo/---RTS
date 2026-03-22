// UnitType.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    public string unitName;
    public float baseHealth = 100f;
    public float baseAttack = 10f;
    public float baseSpeed = 5f;
    // 其他基础属性...
}