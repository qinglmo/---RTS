using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ResourceType
{
    Woods,
    Forest,
    Stone,
    Mountain,
    IronMountain,
}

public class SurfaceResources : MonoBehaviour, IOccupyEnity
{
    public Vector2 Position{ get { return transform.position; } }

    private Vector2Int? currentGrid;
    public Vector2Int GridPos { get; set; }
    public ResourceType resourceType ;

    void Start()
    {
        if(currentGrid == null)
        {
            var grid = GridManager.Instance.WorldToCell(transform.position);
            if (GridManager.Instance.TryOccupy(grid, this))
            {
                currentGrid = grid;
                transform.position = GridManager.Instance.CellToWorld(grid); //其实已经不需要了
            }
        }
    }
}
