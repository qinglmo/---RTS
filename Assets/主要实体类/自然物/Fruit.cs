using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResource : IHasPosition
{
    public int TotalResources { get; }
    public int GatherNum { get; }
    ResourceType ResourceType { get; }
    LifeCycle LifeCycle { get; }
    public (ResourceType, int) GatherResource();
}
public enum ResourceType
{
    Wood,
    Food,
    Stone
}
public enum LifeCycle
{
    Growth ,//生长
    Harvest,//收获
    Regrowth ,//再生
    Dead//死亡
}
public class Fruit: MonoBehaviour, IResource
{
    
    public LifeCycle lifeCycle= LifeCycle.Harvest;
    public LifeCycle LifeCycle { get { return lifeCycle; } }
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
                lifeCycle = LifeCycle.Regrowth;
                
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
        TimeManager.OnOneHourPulse += TimeUpdate;
    }
    private void OnDestroy()
    {
        TimeManager.OnOneHourPulse -= TimeUpdate;
    }
    public void TimeUpdate()
    {
        if(lifeCycle == LifeCycle.Regrowth)
        {
            TotalResources+=GatherNum;
            if(TotalResources>=500)
            {
                lifeCycle = LifeCycle.Harvest;
            }
        }
    }
    public (ResourceType,int) GatherResource()
    {
        if(lifeCycle != LifeCycle.Harvest)
        {
            return (ResourceType,0);
        }
        int gathered = Mathf.Min(GatherNum, TotalResources);
        TotalResources -= gathered;
        return (ResourceType, gathered);
    }
}
