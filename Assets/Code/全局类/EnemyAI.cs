using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 控制相同阵营的所有单位，为其指定总攻击目标
/// </summary>
public class EnemyAI :SingletonMono<EnemyAI>
{
    public Faction Faction;
    public Vector2Int? totalTarget;

    private void Start()
    {
        if (totalTarget == null)
        {
            totalTarget=FactionManager.Instance?.mainSettlement?.GridPos;
        }
        EventBus_Unit.OnUnitActivated += SetTarget_Unit;//监听单位激活事件，为单位设置总攻击目标

    }
    private void OnDestroy()
    {
        EventBus_Unit.OnUnitActivated -= SetTarget_Unit;
    }
    private void Update()
    {
        if (totalTarget == null)
        {
            totalTarget = FactionManager.Instance?.mainSettlement?.GridPos;
        }
    }
    private void SetTarget_Unit(BaseUnit unit)
    {
        if (unit.Faction == Faction)
        {
            unit.AttackTarget = totalTarget;
        }
    }
}
