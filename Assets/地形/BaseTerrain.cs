using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTerrain : MonoBehaviour
{
    protected Vector2Int? currentGrid;
    public float speedMultiplier;//该地形上面行走的速度倍率。
    protected virtual void OnEnable()
    {
        RegisterAtCurrentPosition();
    }

    protected virtual void OnDisable()
    {
        UnregisterCurrentPosition();
    }

    protected virtual void OnDestroy()
    {
        UnregisterCurrentPosition();
    }

    private void RegisterAtCurrentPosition()
    {
        if (currentGrid == null)
        {
            Vector2Int currPos = GridManager.Instance.WorldToCell(transform.position);
            if (!GridManager.CellsOfTerrain.ContainsKey(currPos))
            {
                GridManager.CellsOfTerrain[currPos] = this;
                currentGrid = currPos;
            }
        }
    }
    private void UnregisterCurrentPosition()
    {
        if (currentGrid.HasValue && GridManager.CellsOfTerrain.ContainsKey(currentGrid.Value))
        {
            // 仅当字典中确实是本对象时才移除（防止被其他对象覆盖后误删）
            if (GridManager.CellsOfTerrain[currentGrid.Value] == this)
                GridManager.CellsOfTerrain.Remove(currentGrid.Value);
        }
        currentGrid = null;
    }
}
