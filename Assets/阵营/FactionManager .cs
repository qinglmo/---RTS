
// FactionManager.cs
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

    [Header("所有阵营列表")]
    public List<Faction> factions;

    [Header("所有单位类型列表")]
    public List<UnitType> unitTypes;

    [Header("阵营单位数据覆盖表（在Inspector中配置）")]
    public List<FactionUnitOverrideEntry> factionUnitOverrides;

    // 快速查找的数据结构：字典嵌套字典
    private Dictionary<Faction, Dictionary<UnitType, FactionUnitOverride>> overrideDict;

    public static Dictionary<Faction, Dictionary<Vector2Int, BaseUnit>> FactionOccupyCells
    = new Dictionary<Faction, Dictionary<Vector2Int, BaseUnit>>();
    public static Dictionary<Faction, List< BuildingHospital>> FactionHospitals
    = new Dictionary<Faction, List<BuildingHospital>>();


    [System.Serializable]
    public class FactionUnitOverrideEntry
    {
        public Faction faction;
        public UnitType unitType;
        public float healthMultiplier = 1f;
        public float attackMultiplier = 1f;
        public float speedMultiplier = 1f;
    }
    [System.Serializable]
    public class FactionUnitOverride
    {
        public UnitType unitType;
        public float healthMultiplier = 1f;       // 血量倍率
        public float attackMultiplier = 1f;       // 攻击倍率
        public float speedMultiplier = 1f;
        // 或者直接覆盖数值：healthOverride = -1 表示不使用覆盖
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverrideDictionary();
        
        foreach(var faction in factions)
        {
            FactionOccupyCells[faction] = new Dictionary<Vector2Int, BaseUnit>();
            FactionHospitals[faction] = new List<BuildingHospital>();
        }
    }

    private void BuildOverrideDictionary()
    {
        overrideDict = new Dictionary<Faction, Dictionary<UnitType, FactionUnitOverride>>();
        foreach (var entry in factionUnitOverrides)
        {
            if (!overrideDict.ContainsKey(entry.faction))
                overrideDict[entry.faction] = new Dictionary<UnitType, FactionUnitOverride>();

            var overrideData = new FactionUnitOverride
            {
                unitType = entry.unitType,
                healthMultiplier = entry.healthMultiplier,
                attackMultiplier = entry.attackMultiplier,
                speedMultiplier = entry.speedMultiplier
            };
            overrideDict[entry.faction][entry.unitType] = overrideData;
        }
    }

    // 查询最终属性
    public UnitStats GetFinalStats(Faction faction, UnitType unitType)
    {
        // 基础属性
        float health = unitType.baseHealth;
        float attack = unitType.baseAttack;
        float speed = unitType.baseSpeed;

        // 检查是否有阵营覆盖
        if (overrideDict.TryGetValue(faction, out var unitOverrides))
        {
            if (unitOverrides.TryGetValue(unitType, out var overrideData))
            {
                health *= overrideData.healthMultiplier;
                attack *= overrideData.attackMultiplier;
                speed *= overrideData.speedMultiplier;
            }
        }

        return new UnitStats(health, attack, speed);
    }
}

// 简单的数据结构传递最终值
public struct UnitStats
{
    public float health;
    public float attack;
    public float speed;

    public UnitStats(float health, float attack, float speed)
    {
        this.health = health;
        this.attack = attack;
        this.speed = speed;
    }
}