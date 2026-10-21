using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingMainSettlement : BaseBuilding,IProduction
{


    public BuildingProduction ProductionComponent_Building { get ; set ; }

    private void Awake()
    {
        ProductionComponent_Building = GetComponent<BuildingProduction>() ?? gameObject.AddComponent<BuildingProduction>();

    }
    protected override void Start()
    {
        base.Start();
        FactionManager.Instance.mainSettlement = this;
        TimeHourUpdate();
        TimeManager.Instance.OnOneHourPulse += TimeHourUpdate;
    }
    private void OnDestroy()
    {
        TimeManager.Instance.OnOneHourPulse -= TimeHourUpdate;
    }
    private void TimeHourUpdate()
    {

    }

}
