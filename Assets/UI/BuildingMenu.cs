using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string buildingName;
    public GameObject prefab;      // 建筑预制体
    public int foodCost;
    public int woodCost;
    public int stoneCost;
    public float buildTime;        // 建造耗时（可选）
    public Sprite icon;            // UI图标（可选）
}
public class BuildingMenu :MonoBehaviour
{
    public static BuildingMenu Instance { get; private set; }
    public List<GameObject> buildingPrefabs;
    public List<BuildingData> availableBuildings;  // 在Inspector里配置

    private void Awake()
    {
        Instance = this;
    }

    public void OnBuildingMenuOpen()
    {
        ListPanel.Instance.ShowPanel();
        ListPanel.Instance.SetBuildingPrefabsList(availableBuildings);
        ListPanel.Instance.OnBuildingPrefabSelected = BuildingSystem.Instance.StartBuilding;
    }
}

