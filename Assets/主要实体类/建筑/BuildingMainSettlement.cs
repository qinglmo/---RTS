using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingMainSettlement : BaseBuilding,IProduction
{
    public float scan = 20f;//资源扫描范围
    public List<WorkUnit> workUnits = new List<WorkUnit>();//为空闲单位派发任务
    public Dictionary<IResource,int> resources = new Dictionary<IResource,int>();//资源-工作人数
    public IResourceProvider resourceProvider = null;

    public BuildingProduction ProductionComponent_Building { get ; set ; }

    private void Awake()
    {
        resourceProvider= GetComponent<IResourceProvider>() ?? gameObject.AddComponent<BackgroundScan>();
        ProductionComponent_Building = GetComponent<BuildingProduction>() ?? gameObject.AddComponent<BuildingProduction>();

    }
    protected override void Start()
    {
        base.Start();
        FactionManager.Instance.mainSettlement = this;
        TimeHourUpdate();
        TimeManager.OnOneHourPulse += TimeHourUpdate;
    }
    private void OnDestroy()
    {
        TimeManager.OnOneHourPulse -= TimeHourUpdate;
    }
    private void TimeHourUpdate()
    {
        var targets=resourceProvider.GetAllResourceInCircle(GridPos,scan);
        foreach(var resource in targets)
        {
            if(!resources.ContainsKey(resource))
                resources.Add(resource,0);
        }
    }
    public IResource TaskApplication(WorkUnit unit)//先简化实现
    {
        if(resources.Count== 0)
        {
            return null;
        }
        foreach(var resource in resources)
        {
            if(resource.Key.LifeCycle != LifeCycle.Harvest)
                continue;
            if(resource.Value>=CanWorkPosNum(resource.Key))//工作人数大于等于最大工作人数
            {
                continue;
            }
            resources[resource.Key]++;
            return resource.Key;
        }
        return null;
    }

    public IResource SubmissionResource(ResourceType type,int num,IResource resource )
    {
        
        
        switch (type)
        {
            case ResourceType.Food:ResourceSystem.AddFood(num);
                break;
            case ResourceType.Wood:ResourceSystem.AddWood(num);
                break;
            case ResourceType.Stone:ResourceSystem.AddStone(num);
                break;
        }
        if(resource.LifeCycle==LifeCycle.Harvest)
        {
            return resource;
        }
        else
        {
            resources[resource]--;
            return null;
        }
    }
    private int CanWorkPosNum(IResource resource)
    {
        var neighbors=SpaceGet.GetNeighbors(resource.GridPos);
        int count=1;//自身位置算一个工作位置
        foreach(var neighbor in neighbors)
        {
            if(GridManager.Instance.GetResource(neighbor)!=null)//格子上是资源，不算工作位置
            {
                continue;
            }
            if (GridManager.Instance.IsOccupied_Static(neighbor))//格子被阻碍，不算工作位置
            {
                continue;
            }
            count++;
        }
        return count;
    }
}
