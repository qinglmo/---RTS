using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 可驻扎建筑，提供驻扎功能
/// </summary>
public interface IStationIn 
{
    public bool TryEnter(BaseUnit unit);
    public bool CanOccupy();
}


/// <summary>
/// 医院建筑：可驻扎单位，自动治疗内部单位，满血单位自动释放，禁止满血单位进入
/// </summary>
public class BuildingHospital : BaseBuilding, IStationIn
{
    [Header("驻扎设置")]
    public int maxGarrison = 5;                      // 最大驻扎人数
    private List<BaseUnit> garrisonUnits = new List<BaseUnit>(); // 当前驻扎单位列表

    [Header("治疗设置")]
    public int healAmount = 1;                       // 每次治疗的生命值
    public float healInterval = 2f;                    // 治疗间隔（秒）

    private Coroutine healCoroutine;                   // 治疗协程引用

    protected override void Start()
    {
        base.Start();
        //在全局管理器中注册医院，方便单位查询
        FactionManager.FactionHospitals[Faction].Add(this) ;
        // 启动治疗协程
        healCoroutine = StartCoroutine(HealGarrisonRoutine());
    }

    void OnDestroy()
    {
        // 停止治疗协程
        if (healCoroutine != null)
            StopCoroutine(healCoroutine);

        // 释放所有驻扎单位（尝试在周围空位生成）
        ReleaseAllGarrison();
        //取消注册
        FactionManager.FactionHospitals[Faction].Remove(this);
    }

    // ==================== 驻扎相关逻辑 ====================

    /// <summary>
    /// 尝试让一个友军单位驻扎进医院
    /// </summary>
    public bool TryEnter(BaseUnit unit)
    {
        if (unit == null) return false;
        if (garrisonUnits.Count >= maxGarrison) return false;          // 已满
        if (unit.Faction != this.Faction) return false;                // 阵营不同不能驻扎
        if (unit.attributes.CurrentHealth >= unit.attributes.MaxHealth) return false;                // 满血单位不能进入

        // 将单位从世界中移除（隐藏并释放其占用的格子）
        unit.gameObject.SetActive(false);

        garrisonUnits.Add(unit);
        Debug.Log($"{name} 驻扎了 {unit.name}，当前驻军 {garrisonUnits.Count}/{maxGarrison}");
        return true;
    }
    public List<BaseUnit> ReadUnitList()
    {
        return garrisonUnits; 
    }
    public bool CanOccupy()
    {
        return garrisonUnits.Count < maxGarrison;
    }

    // ==================== 治疗逻辑 ====================

    /// <summary>
    /// 治疗协程：定期治疗所有驻扎单位，满血单位自动释放
    /// </summary>
    private IEnumerator HealGarrisonRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);

            // 注意：治疗过程中可能释放单位，需要倒序遍历避免修改集合异常
            for (int i = garrisonUnits.Count - 1; i >= 0; i--)
            {
                BaseUnit unit = garrisonUnits[i];
                if (unit == null)
                {
                    garrisonUnits.RemoveAt(i);
                    continue;
                }

                // 治疗单位
                HealUnit(unit);

                // 如果单位满血，尝试释放
                if (unit.attributes.CurrentHealth >= unit.attributes.MaxHealth)
                {
                    TryReleaseUnit(unit);
                }
            }
        }
    }

    /// <summary>
    /// 治疗单个单位
    /// </summary>
    private void HealUnit(BaseUnit unit)
    {
        if (unit == null) return;

        // 假设 BaseUnit 有 Heal 方法，如果没有则直接修改 HP
        unit.attributes.Heal(healAmount);

        Debug.Log($"{unit.name} 在 {name} 中接受了治疗，当前生命 {unit.attributes.CurrentHealth}/{unit.attributes.CurrentHealth}");
    }

    /// <summary>
    /// 尝试释放一个单位到周围空闲格子
    /// </summary>
    private bool TryReleaseUnit(BaseUnit unit)
    {
        if (unit == null || !garrisonUnits.Contains(unit)) return false;

        // 寻找周围空闲格子
        Vector2Int? freePos = FindFreeNeighbor();
        if (freePos.HasValue)
        {
            unit.gameObject.SetActive(true);
            unit.transform.position = new Vector3(freePos.Value.x, freePos.Value.y, 0);

            garrisonUnits.Remove(unit);
            Debug.Log($"{name} 释放了满血单位 {unit.name} 到 {freePos.Value}");
            return true;
        }
        else
        {
            // 周围没有空位，暂不释放，等待下一次机会
            Debug.LogWarning($"{name} 周围无空位，无法释放满血单位 {unit.name}，单位将暂时留在医院");
            return false;
        }
    }

    /// <summary>
    /// 释放所有驻扎单位（医院销毁时调用）
    /// </summary>
    private void ReleaseAllGarrison()
    {
        // 注意：不能直接遍历并删除，需要拷贝列表
        List<BaseUnit> unitsCopy = new List<BaseUnit>(garrisonUnits);
        foreach (var unit in unitsCopy)
        {
            // 强制释放，如果找不到空位则销毁
            Vector2Int? freePos = FindFreeNeighbor();
            if (freePos.HasValue)
            {
                unit.gameObject.SetActive(true);
                unit.transform.position = new Vector3(freePos.Value.x, freePos.Value.y, 0);
                Debug.Log($"{name} 释放了 {unit.name} 到 {freePos.Value}");
            }
            else
            {
                Debug.LogWarning($"{name} 周围无空位，单位 {unit.name} 将被销毁");
                Destroy(unit.gameObject);
            }
            garrisonUnits.Remove(unit);
        }
    }

    /// <summary>
    /// 寻找一个周围（上下左右）空闲的格子坐标
    /// </summary>
    private Vector2Int? FindFreeNeighbor()
    {
        if (currentGrid == null) return null;

        Vector2Int[] dirs = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        foreach (var dir in dirs)
        {
            Vector2Int neighbor = currentGrid.Value + dir;
            if (!GridManager.IsOccupied(neighbor))
                return neighbor;
        }
        return null;
    }
}