using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTower : BaseBuilding, IStationIn
{

    [Header("基础属性")]
    public float baseAttackPower = 20f;           // 基础攻击力
    public float attackCooldown = 2f;              // 攻击间隔（秒）
    public float attackRange = 10f;                  // 射程（世界单位，假设格子大小为1）
    public GameObject bulletPrefab;                 // 子弹预制体
    public Transform firePoint;                     // 子弹发射点（可置于塔顶）

    [Header("驻扎设置")]
    public int maxGarrison = 3;                      // 最大驻扎人数
    public float garrisonBonusPerUnit = 10f;         // 每个驻扎单位增加的攻击力
    private List<BaseUnit> garrisonUnits = new List<BaseUnit>(); // 当前驻扎单位列表

    private Coroutine attackCoroutine;                 // 攻击协程引用

    protected override void Start()
    {
        base.Start();

        // 启动攻击协程
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    void OnDestroy()
    {
        // 停止攻击协程
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        // 释放所有驻扎单位（尝试在周围空位生成）
        ReleaseAllGarrison();
    }
    

    /// <summary>
    /// 攻击协程：每隔 attackCooldown 秒寻找并攻击敌人
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackCooldown);
            TryAttack();
        }
    }

    /// <summary>
    /// 尝试攻击：寻找射程内的敌人，发射子弹
    /// </summary>
    private void TryAttack()
    {
        if (garrisonUnits.Count == 0) return; // 无驻军时可选择不攻击，也可设定为仍可攻击（仅基础攻击力）

        // 寻找最近的敌人
        BaseUnit target = FindNearestEnemy();
        if (target != null)
        {
            // 计算当前总攻击力 = 基础攻击 + 驻军加成
            float totalAttackPower = baseAttackPower + garrisonUnits.Count * garrisonBonusPerUnit;
            ShootAt(target, totalAttackPower);
        }
    }

    /// <summary>
    /// 使用 Physics2D.OverlapCircle 寻找射程内最近的敌对单位
    /// </summary>
    private BaseUnit FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        BaseUnit nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            BaseUnit unit = hit.GetComponent<BaseUnit>();
            if (unit != null && unit.Faction != this.Faction) // 敌对阵营
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = unit;
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// 发射子弹攻击目标
    /// </summary>
    private void ShootAt(BaseUnit target, float attackPower)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(target.transform.position-gameObject.transform.position, (int)attackPower, this.Faction); // 子弹需要知道发射者阵营，避免误伤
        }
    }

    // ==================== 驻扎相关逻辑 ====================

    /// <summary>
    /// 尝试让一个友军单位驻扎进箭塔
    /// </summary>
    public bool TryEnter(BaseUnit unit)
    {
        if (unit == null) return false;
        if (garrisonUnits.Count >= maxGarrison) return false;          // 已满
        if (unit.Faction != this.Faction) return false;               // 阵营不同不能驻扎

        // 将单位从世界中移除（隐藏并释放其占用的格子）
        unit.gameObject.SetActive(false);

        garrisonUnits.Add(unit);
        Debug.Log($"{name} 驻扎了 {unit.name}，当前驻军 {garrisonUnits.Count}/{maxGarrison}");
        return true;
    }
    public bool CanOccupy()
    {
        if(garrisonUnits.Count< maxGarrison)
        {
            return true; 
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 释放一个驻扎单位到周围空闲格子（通常用于箭塔被摧毁时）
    /// </summary>
    private void ReleaseOneGarrison(BaseUnit unit)
    {
        if (unit == null) return;
        if (!garrisonUnits.Contains(unit)) return;

        // 寻找周围空闲格子
        Vector2Int? freePos = FindFreeNeighbor();
        if (freePos.HasValue)
        {
            unit.gameObject.SetActive(true);
            unit.transform.position = new Vector3(freePos.Value.x, freePos.Value.y, 0);

            garrisonUnits.Remove(unit);
            Debug.Log($"{name} 释放了 {unit.name} 到 {freePos.Value}");
        }
        else
        {
            // 周围没有空位，可选择将单位直接销毁或传送到较远位置
            Debug.LogWarning($"{name} 周围无空位，无法释放单位 {unit.name}，单位将被销毁");
            Destroy(unit.gameObject);
            garrisonUnits.Remove(unit);
        }
    }

    /// <summary>
    /// 释放所有驻扎单位（箭塔销毁时调用）
    /// </summary>
    private void ReleaseAllGarrison()
    {
        // 注意：不能直接遍历并删除，需要拷贝列表
        List<BaseUnit> unitsCopy = new List<BaseUnit>(garrisonUnits);
        foreach (var unit in unitsCopy)
        {
            ReleaseOneGarrison(unit);
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