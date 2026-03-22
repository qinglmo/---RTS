using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static EnemyUnit;

public class EscapingState : IState
{
    private BaseUnit unit;
    private float timer;
    private float inspectionInterval = 1f; // 重新寻路间隔

    private BuildingHospital targetHospital; // 当前目标医院
    private Vector2Int targetGrid;            // 目标医院所在格子

    private IStateMachine stateMachine;
    public EscapingState(BaseUnit unit, IStateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        FindNewHospital(); // 初始寻找医院
    }

    public void Update()
    {
        // 如果当前没有有效医院，尝试重新寻找
        if (targetHospital == null)
        {
            FindNewHospital();
            if (targetHospital == null) // 仍然找不到，退出状态
            {
                return;
            }
        }

        // 检查是否已到达医院附近（相邻或对角相邻）
        Vector2Int currentGrid = (Vector2Int)unit.GridPos; // 假设 BaseUnit 有 currentGrid 属性
        if (Mathf.Abs(currentGrid.x - targetGrid.x) <= 1 && Mathf.Abs(currentGrid.y - targetGrid.y) <= 1)
        {
            // 尝试进入医院
            bool success = targetHospital.TryEnter(unit);
            if (success)
            {
                return;
            }
            else
            {
                // 进入失败（可能医院突然不可用），清除目标，下一帧重新寻找
                targetHospital = null;
            }
        }

        // 计时重新寻路
        timer += Time.deltaTime;
        if (timer >= inspectionInterval)
        {
            timer = 0f;
            if (targetHospital != null)
            {
                RouteDecision.UniversalRouteDecision(unit, targetGrid);
            }
        }
    }

    public void Exit()
    {

    }

    /// <summary>
    /// 寻找一个新的可驻扎医院，并设置路径
    /// </summary>
    private void FindNewHospital()
    {
        targetHospital = null;
        if (FactionManager.FactionHospitals.TryGetValue(unit.Faction, out var hospitals))
        {
            foreach (var hospital in hospitals)
            {
                if (hospital != null && hospital.CanOccupy())
                {
                    targetHospital = hospital;
                    targetGrid = (Vector2Int)hospital.GridPos; // 假设 BuildingHospital 有 GridPos 属性
                    RouteDecision.UniversalRouteDecision(unit, targetGrid);
                    break;
                }
            }
        }

        if (targetHospital == null)
        {
            Debug.LogWarning($"{unit.name} 找不到可驻扎的医院");
        }
    }

    public void Reset()
    {
        FindNewHospital(); // 初始寻找医院
    }
}


