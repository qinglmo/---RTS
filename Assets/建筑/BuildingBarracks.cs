using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位生成数据（可在 Inspector 中配置）
/// </summary>
[System.Serializable]
public class UnitSpawnData
{
    public string unitName;             // 单位名称（用于 UI 显示）
    public GameObject prefab;           // 单位预制体
    public int health;                  // 初始生命值
    public int attack;                  // 初始攻击力
    // 可根据需要添加更多字段，如图标、描述等
}
public class BuildingBarracks : BaseBuilding
{
    [Header("单位预制体列表")]
    public List<UnitSpawnData> units;
    public int ID=0;
    /// <summary>
    /// 获取所有可生成的单位预制体列表
    /// </summary>
    public List<GameObject> ReadUnitList()
    {
        var list = new List<GameObject>();
        foreach (var unit in units)
        {
            list.Add(unit.prefab);
        }
        return list;
    }

    /// <summary>
    /// 尝试在周围四个方向生成指定索引的单位
    /// </summary>
    /// <param name="prefabIndex">预制体在列表中的索引</param>
    /// <param name="health">初始生命值</param>
    /// <param name="attack">初始攻击力</param>
    /// <param name="faction">单位阵营（若不指定，则使用兵营自身的阵营）</param>
    /// <returns>是否成功生成</returns>
    public void GenerateUnit(int prefabIndex)
    {
        Debug.Log("生成单位"+ prefabIndex);
        // 检查索引是否有效
        if (units == null || prefabIndex < 0 || prefabIndex >= units.Count)
        {
            Debug.LogError($"单位预制体索引 {prefabIndex} 无效");
            return;
        }

        UnitSpawnData data = units[prefabIndex];
        // 使用兵营自身的阵营（假设 BaseBuilding 中已有 Faction 属性）
        TrySpawnUnit(data.prefab, data.health, data.attack, this.Faction);
    }

    /// <summary>
    /// 核心生成逻辑：检查周围四个方向是否有空位，若有则实例化并初始化单位
    /// </summary>
    private bool TrySpawnUnit(GameObject prefab, int health, int attack, Faction faction)
    {
        if (currentGrid == null)
        {
            Debug.LogWarning("兵营尚未初始化网格位置");
            return false;
        }

        // 定义四个方向：上、下、左、右（假设y轴向上）
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = currentGrid.Value + dir;
            // 检查该格子是否已被占用
            if (!GridManager.Instance.IsOccupied(neighbor))
            {
                // 生成单位并设置其位置
                GameObject unitObj = Instantiate(prefab);
                BaseUnit baseUnit = unitObj.GetComponent<BaseUnit>();
                if (baseUnit == null)
                {
                    Debug.LogError("生成的预制体上没有 BaseUnit 组件");
                    Destroy(unitObj);
                    return false;
                }
                
                // 初始化单位（生命值、攻击力、阵营）
                baseUnit.Initialize(health, attack, faction);
                unitObj.name = "单位" + ID.ToString();
                ID++;
                unitObj.transform.position = new Vector3(neighbor.x, neighbor.y, 0);

                Debug.Log($"{name} 在 {neighbor} 生成了一个单位");
                return true; // 成功生成一个后退出
            }
        }

        // 所有四个方向都被占用
        Debug.Log($"{name} 周围没有空位，无法生成单位");
        return false;
    }

    // 移除原有的自动生成协程相关代码（Start、OnDestroy、GenerateRoutine 等）
}