using UnityEngine;

/// <summary>
/// 单位生成数据（可在 Inspector 中配置）
/// </summary>
[CreateAssetMenu(fileName = "UnitSpawnData", menuName = "Game/UnitSpawnData")]
public class UnitSpawnData:ScriptableObject
{
    public string unitName;             // 单位名称（用于 UI 显示）
    public GameObject prefab;           // 单位预制体
    public int health;                  // 初始生命值
    public int attack;                  // 初始攻击力
    public int foodCost;
    public int woodCost;
    public int stoneCost;
    public int productionTime;
    // 可根据需要添加更多字段，如图标、描述等
}