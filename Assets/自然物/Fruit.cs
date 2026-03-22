using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResource
{
    public int TotalResources { get; }
    public int GatherNum { get; }
    ResourceType ResourceType { get; }
    public (ResourceType, int) GatherResource();
}
public enum ResourceType
{
    Wood,
    Food,
    Stone
}
public class Fruit: MonoBehaviour, IHasPosition, IResource
{
    public Vector2 Position { get { return transform.position; } }

    private Vector2Int? currentGrid;
    public Vector2Int GridPos { get { return currentGrid.Value; } set { } }
    [SerializeField] private int totalResources;
    public int TotalResources { get {return totalResources;}
        set 
        {
            if(value<=0)
            {
                totalResources = 0;
                
            }
            else
            {
                totalResources = value;
            }
                
        }
    }
    [SerializeField] private int gatherNum;
    public int GatherNum { get { return gatherNum; } }
    [SerializeField] private ResourceType resourceType;
    public ResourceType ResourceType { get { return resourceType; } }
    public IRegisterEntity register;
    void Start()
    {
        if (currentGrid == null)
        {
            register = GridManager.Instance;
            currentGrid = GridManager.Instance.WorldToCell(transform.position);
            transform.position=(Vector3Int)currentGrid.Value;
            if (!register.TryRegisterResource(currentGrid.Value, this))
            {
                Debug.LogError("放置错误的资源");
            }
        }
    }

    public (ResourceType,int) GatherResource()
    {
        int gathered = Mathf.Min(GatherNum, TotalResources);
        TotalResources -= gathered;
        return (ResourceType, gathered);
    }
}
