// Faction.cs
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewFaction", menuName = "Game/Faction")]
public class Faction : ScriptableObject
{
    [Tooltip("阵营的唯一标识（类似Tag）")]
    public string factionTag;           // 也可以使用枚举，但ScriptableObject引用更灵活

    [Tooltip("阵营的显示名称")]
    public string displayName;

    [Tooltip("阵营的代表颜色，用于UI、粒子等")]
    public Color factionColor = Color.white;

    [Header("外交关系")]
    public List<Faction> allies;
    public List<Faction> enemies;

    private Dictionary<BuildingHospital, Vector2Int> hospitals = new Dictionary<BuildingHospital, Vector2Int>();
    // 判断是否敌对
    public bool IsEnemy(Faction other)
    {
        if (other == null) return false;
        if (other == this) return false; // 自己不算敌人
        return enemies.Contains(other);
    }

    // 判断是否友好（包括自己）
    public bool IsAlly(Faction other)
    {
        if (other == null) return false;
        if (other == this) return true;   // 自己视为盟友
        return allies.Contains(other);
    }
}