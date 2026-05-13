using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class BuildingProduction : MonoBehaviour
{
    BaseBuilding building;

    private List<UnitSpawnData> productionQueue;      // 生产队列
    private float currentProductionTimer;               // 当前生产剩余时间
    private bool isProducing;                           // 是否正在生产
    [Header("生产设置")]
    public float defaultProductionTime = 3f;            // 默认生产时间（秒）
    
    [Header("单位预制体列表")]
    public List<UnitSpawnData> units;
    public int ID = 0;

    public event Action OnOneUnitComplete;//单个生产完成事件，用于通知外部生产完成
    public event Action OnAddNewUnit;//加入新单位事件，用于通知外部加入新单位
    private void Awake()
    {
        building = GetComponent<BaseBuilding>();
        productionQueue = new List<UnitSpawnData>();
    }
    private void Update()
    {
        if (!isProducing) return;

        currentProductionTimer -= Time.deltaTime;
        if (currentProductionTimer <= 0f)
            CompleteProduction();
    }
    /// <summary>
    /// 获取当前生产进度（0-1之间）,0表示生产完成
    /// </summary>
    /// <returns></returns>
    public float GetProductionTimeRate()
    {
        UnitSpawnData nextUnit = productionQueue[0];
        // 获取生产时间（假设 UnitSpawnData 有 productionTime 字段，否则用默认值）
        float prodTime = (nextUnit.productionTime > 0) ? nextUnit.productionTime : defaultProductionTime;
        return currentProductionTimer / prodTime;
    }
    /// <summary>
    /// 获取所有可生成的单位预制体列表
    /// </summary>
    public List<UnitSpawnData> ReadUnitList()
    {
        var list = new List<UnitSpawnData>();
        foreach (var unit in units)
        {
            list.Add(unit);
        }
        return list;
    }
    /// <summary>
    /// 获取当前生产中的单位预制体
    /// </summary>
    /// <returns></returns>
    public List<UnitSpawnData> GetProductionQueue()
    {
        return productionQueue;
    }

    /// </summary>
    /// <summary>
    /// 将单位加入生产队列（会立即预扣资源）
    /// </summary>
    public void AddToQueue(UnitSpawnData unit)
    {
        // 检查资源是否足够（预扣前）
        if (!ResourceSystem.TryBuild(unit.foodCost, unit.woodCost, unit.stoneCost))
        {
            Debug.Log("资源不足，无法加入队列");
            return;
        }
        productionQueue.Add(unit);

        if (!isProducing)
            StartProduction();
        OnAddNewUnit?.Invoke();//通知加入新单位事件
    }
    /// <summary>
    /// 开始生产队列中的第一个单位
    /// </summary>
    private void StartProduction()
    {
        if (productionQueue.Count == 0) return;

        isProducing = true;
        UnitSpawnData nextUnit = productionQueue[0];

        // 获取生产时间（假设 UnitSpawnData 有 productionTime 字段，否则用默认值）
        float prodTime = (nextUnit.productionTime > 0) ? nextUnit.productionTime : defaultProductionTime;
        currentProductionTimer = prodTime;
    }

    /// <summary>
    /// 尝试在周围四个方向生成指定索引的单位
    /// </summary>
    /// <param name="prefabIndex">预制体在列表中的索引</param>
    /// <param name="health">初始生命值</param>
    /// <param name="attack">初始攻击力</param>
    /// <param name="faction">单位阵营（若不指定，则使用兵营自身的阵营）</param>
    /// <returns>是否成功生成</returns>
    public void GenerateUnit(UnitSpawnData unit)
    {
        AddToQueue(unit);
    }


    /// <summary>
    /// 生产完成，尝试生成单位
    /// </summary>
    private void CompleteProduction()
    {
        UnitSpawnData unit = productionQueue[0];
        productionQueue.RemoveAt(0);

        // 尝试生成单位（使用 skipResourceCheck = true，因为资源已预扣）
        bool success = TrySpawnUnit(unit.prefab, unit.health, unit.attack, building.Faction, unit);

        if (!success)
        {
            // 生成失败（无空位等），返还资源
            ResourceSystem.AddResources(unit.foodCost, unit.woodCost, unit.stoneCost);
            Debug.Log($"生产失败，返还 {unit.name} 的资源");
        }
        OnOneUnitComplete?.Invoke();//通知单个生产完成事件
        // 继续下一个
        if (productionQueue.Count > 0)
            StartProduction();
        else
            isProducing = false;
    }
    /// <summary>
    /// 核心生成逻辑：检查周围四个方向是否有空位，若有则实例化并初始化单位
    /// </summary>
    private bool TrySpawnUnit(GameObject prefab, int health, int attack, Faction faction,UnitSpawnData data)
    {

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
            Vector2Int neighbor = building.GridPos + dir;
            // 检查该格子是否已被占用
            if (GridManager.Instance.IsOccupied(neighbor))
            {
                var unit = GridManager.Instance.GetUnit(neighbor);
                if (unit != null && unit.Faction == building.Faction)
                {
                    unit.MoveToOther();
                    continue;
                }
                else
                {
                    continue;
                }
            }
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
        // 所有四个方向都被占用
        Debug.Log($"{name} 周围没有空位，无法生成单位");
        return false;
    }

    // 移除原有的自动生成协程相关代码（Start、OnDestroy、GenerateRoutine 等）

}
