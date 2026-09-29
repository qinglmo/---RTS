using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingBarracks : BaseBuilding, IProduction
{
    public BuildingProduction ProductionComponent_Building { get; set; }

    private void Awake()
    {
        ProductionComponent_Building = GetComponent<BuildingProduction>() ?? gameObject.AddComponent<BuildingProduction>();
    }
}