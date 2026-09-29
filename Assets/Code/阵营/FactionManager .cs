
// FactionManager.cs
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

    [Header("所有阵营列表")]
    public List<Faction> factions;
    public Faction EnemyFaction{get{return factions[1];}}


    // 快速查找的数据结构：字典嵌套字典
    public static Dictionary<Faction, Dictionary<Vector2Int, IFactionMember>> FactionOccupyCells
    = new Dictionary<Faction, Dictionary<Vector2Int, IFactionMember>>();
    public static Dictionary<Faction, List< BuildingHospital>> FactionHospitals
    = new Dictionary<Faction, List<BuildingHospital>>();
    public BuildingMainSettlement mainSettlement;

    public Dictionary<Faction,List<BaseUnit>> FactionUnits//通过订阅全局单位出现消失事件，维护不同阵营的单位列表。
    = new Dictionary<Faction, List<BaseUnit>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach(var faction in factions)
        {
            FactionOccupyCells[faction] = new Dictionary<Vector2Int, IFactionMember>();
            FactionHospitals[faction] = new List<BuildingHospital>();
            FactionUnits[faction] = new List<BaseUnit>();
        }
        EventBus_Unit.OnUnitActivated += AddUnit;
        EventBus_Unit.OnUnitDeactivated += RemoveUnit;
    }
    private void OnDestroy()
    {
        EventBus_Unit.OnUnitActivated -= AddUnit;
        EventBus_Unit.OnUnitDeactivated -= RemoveUnit;
    }
    
    private void AddUnit(BaseUnit unit)
    {
        if (!FactionUnits.ContainsKey(unit.Faction))
            FactionUnits[unit.Faction] = new List<BaseUnit>();
        FactionUnits[unit.Faction].Add(unit);
    }
    private void RemoveUnit(BaseUnit unit)
    {
        if (FactionUnits.TryGetValue(unit.Faction, out var units))
            units.Remove(unit);
    }

    public List<BaseUnit> GetUnits(Faction faction)
    {
        return FactionUnits[faction];
    }

}
