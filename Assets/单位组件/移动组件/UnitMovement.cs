using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责分发路径，当路径执行失败时停止且报错。只接收完整路径(不包括开始格子)，一格到任意格，不参与寻路。
/// 实现以下细节：每次接收新路径时，第一步应该直接下发，无论底层是否正在移动，让底层处理新路径转向问题。
/// 应该支持可以连续接收路径，可以正确处理路径的更新，当新路径来到时，应该立即抛弃旧路径的任何发布和等待。
/// </summary>
public class UnitMovement : MonoBehaviour
{
    private Vector2Int? PreOccupyPos; // 预占领格子，防友军不防敌军
    private GameObject PreOcuupyImage;
    private GameObject preOccupyImageInstance;

    public event System.Action OnMoveStarted;
    public event System.Action OnMoveCompleted; // 到达目标或被中断

    public BaseUnit unit;
    public StepMover stepMover;

    // 当前正在执行的路径和索引
    private List<Vector2Int> _currentPath;
    private int _currentStepIndex;


    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        stepMover = GetComponent<StepMover>() ?? gameObject.AddComponent<StepMover>();
        stepMover.OnStepCompleted += OnStepCompletedHandler;
        PreOcuupyImage= Resources.Load<GameObject>("预占图标");
    }

    private void OnDisable()
    {
        // 释放预占
        if (PreOccupyPos.HasValue)
        {
            FactionManager.FactionOccupyCells[unit.Faction].Remove(PreOccupyPos.Value);
            PreOccupyPos = null;
            Destroy(preOccupyImageInstance);
        }
    }
    private void OnDestroy()
    {
        if (stepMover != null)
            stepMover.OnStepCompleted -= OnStepCompletedHandler;
    }

    /// <summary>
    /// 初始化占用（通常在 Start 中调用）
    /// </summary>
    public void Initialize()
    {
        stepMover.InitializeOccupancy(unit);
    }

    /// <summary>
    /// 移动到目标网格（自动处理终点占用和移动阻塞重试）
    /// </summary>
    public void MoveToGridWithPaths(Vector2Int finalGrid, List<Vector2Int> paths)
    {
        // 取消之前的路径
        CancelCurrentPath();

        // 预占最终格子
        FactionManager.FactionOccupyCells[unit.Faction][finalGrid] = unit;
        PreOccupyPos = finalGrid;
        preOccupyImageInstance = Instantiate(PreOcuupyImage,(Vector2)PreOccupyPos.Value, Quaternion.identity);
        // 复制路径，防止外部修改
        _currentPath = new List<Vector2Int>(paths);
        if (_currentPath.Count == 0)
        {
            Debug.LogError("路径为空，无法移动");
            return;
        }

        // 第一步立即下发
        stepMover.NextStepDerection(_currentPath[0]);
        _currentStepIndex = 1;

        OnMoveStarted?.Invoke();
    }

    /// <summary>
    /// 取消当前移动
    /// </summary>
    public void CancelMove()
    {
        CancelCurrentPath();
        stepMover.CancelMove();
        // 不触发 OnMoveCompleted，按注释不触发
    }

    private void CancelCurrentPath()
    {

        _currentPath = null;
        _currentStepIndex = 0;

        // 释放预占
        if (PreOccupyPos.HasValue)
        {
            FactionManager.FactionOccupyCells[unit.Faction].Remove(PreOccupyPos.Value);
            PreOccupyPos = null;
            Destroy(preOccupyImageInstance);
        }
    }

    private void OnStepCompletedHandler(Vector2Int arrivedGrid)
    {

        // 检查到达的格子是否是路径中预期的下一个
        if (_currentPath == null || _currentStepIndex - 1 >= _currentPath.Count || arrivedGrid != _currentPath[_currentStepIndex - 1])
        {
            // 路径异常，取消移动
            CancelMove();
            return;
        }

        // 如果所有步骤完成
        if (_currentStepIndex >= _currentPath.Count)
        {
            _currentPath = null;
            _currentStepIndex = 0;
            OnMoveCompleted?.Invoke();

            // 释放预占
            if (PreOccupyPos.HasValue)
            {
                FactionManager.FactionOccupyCells[unit.Faction].Remove(PreOccupyPos.Value);
                PreOccupyPos = null;
                Destroy(preOccupyImageInstance);
            }
            return;
        }

        // 还有下一步，下发
        Vector2Int nextTarget = _currentPath[_currentStepIndex];
        stepMover.NextStepDerection(nextTarget);
        _currentStepIndex++;
    }
}