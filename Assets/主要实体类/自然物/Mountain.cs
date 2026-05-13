using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour, IOccupyEnity
{
    public Vector2 Position{ get { return transform.position; } }

    private Vector2Int? currentGrid;
    public Vector2Int GridPos { get; set; }

    void Start()
    {
        if(currentGrid == null)
        {
            var grid = GridManager.Instance.WorldToCell(transform.position);
            if (GridManager.Instance.TryOccupy(grid, this))
            {
                currentGrid = grid;
                transform.position = new Vector3(currentGrid.Value.x, currentGrid.Value.y, 0);
            }

        }
        
    }
}
