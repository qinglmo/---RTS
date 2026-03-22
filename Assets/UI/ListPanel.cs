using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

interface IListPanel
{
    Action<BaseUnit> OnUnitSelected { get; set; }
}
/// <summary>
/// 单位列表面板管理器（单例）
/// 挂载在包含自动排列组件（如 Vertical/Horizontal/Grid Layout Group）的 GameObject 上
/// </summary>
public class ListPanel : MonoBehaviour,IListPanel
{
    // 单例实例
    public static ListPanel Instance { get; private set; }

    [Header("UI 组件")]
    [Tooltip("按钮预制体，需包含 Button 组件及用于显示名字的 Text 子物体")]
    [SerializeField] private GameObject ButtonPrefab;

    [Tooltip("按钮生成位置的父物体（通常就是脚本所在对象，可留空自动使用 transform）")]
    [SerializeField] private Transform contentParent;

    // 可选：当单位按钮被点击时触发的事件（参数为单位名字）
    public System.Action<int> OnUnitPrefabSelected;
    //当实例单位被点击时触发的事件
    public System.Action<BaseUnit> OnUnitSelected { get; set; }
    public  Action<GameObject> OnPrefabSelected;
    public Action<BuildingData> OnBuildingPrefabSelected;

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 如果未指定父物体，则默认使用当前 transform
        if (contentParent == null)
            contentParent = transform;

        // 初始时隐藏面板
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 接收单位名称列表并刷新 UI
    /// </summary>
    /// <param name="unitNames">单位名称列表</param>
    public void SetUnitList(List<BaseUnit> Units)
    {
        ShowPanel();
        // 清除当前所有生成的按钮
        ClearButtons();

        // 如果没有列表或列表为空，直接返回
        if (Units == null || Units.Count == 0)
            return;

        // 遍历名称列表，生成对应按钮
        foreach (var unit in Units)
        {
            // 避免闭包陷阱，保存当前循环的值
            BaseUnit currunit = unit;

            // 实例化按钮并设置父物体
            GameObject newButton = Instantiate(ButtonPrefab, contentParent);

            // 设置按钮上的文本
            Text textComp = newButton.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = currunit.name;
            else
                Debug.LogWarning("按钮预制体缺少 Text 组件，无法显示单位名称。");

            // 添加点击监听
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnUnitButtonClicked(currunit));
            }
        }
    }
    /// <summary>
    /// 接收单位名称列表并刷新 UI
    /// </summary>
    /// <param name="unitNames">单位名称列表</param>
    public void SetPrefabList_Index(List<GameObject> Units)
    {
        ShowPanel();
        // 清除当前所有生成的按钮
        ClearButtons();

        // 如果没有列表或列表为空，直接返回
        if (Units == null || Units.Count == 0)
            return;
        int index = 0;
        // 遍历名称列表，生成对应按钮
        foreach (var unit in Units)
        {

            // 避免闭包陷阱，保存当前循环的值
            int currentIndex = index;

            // 实例化按钮并设置父物体
            GameObject newButton = Instantiate(ButtonPrefab, contentParent);

            // 设置按钮上的文本
            Text textComp = newButton.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = unit.name;
            else
                Debug.LogWarning("按钮预制体缺少 Text 组件，无法显示单位名称。");

            // 添加点击监听
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnUnitButtonClicked(currentIndex));
            }
            index++;
        }
    }
    /// <summary>
    /// 接收建筑列表并刷新 UI
    /// </summary>
    /// <param name="buildings">建筑列表</param>
    public void SetBuildingList(List<BaseBuilding> buildings)
    {
        // 清除当前所有生成的按钮
        ClearButtons();

        // 如果列表为空，直接返回
        if (buildings == null || buildings.Count == 0)
            return;

        // 遍历建筑列表，生成对应按钮
        foreach (var building in buildings)
        {
            // 保存当前建筑名称（避免闭包陷阱）
            string currentName = building.name;

            // 实例化按钮并设置父物体
            GameObject newButton = Instantiate(ButtonPrefab, contentParent);

            // 设置按钮上的文本
            Text textComp = newButton.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = currentName;
            else
                Debug.LogWarning("按钮预制体缺少 Text 组件，无法显示建筑名称。");

            // 添加点击监听（复用单位按钮的点击处理方法）
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                //btn.onClick.AddListener(() => OnUnitButtonClicked(currentName));
            }
        }
    }
    public void SetPrefabsList(List<GameObject> prefabs)
    {
        // 清除当前所有生成的按钮
        ClearButtons();

        // 如果列表为空，直接返回
        if (prefabs == null || prefabs.Count == 0)
            return;

        // 遍历建筑列表，生成对应按钮
        foreach (var prefab in prefabs)
        {
            // 保存当前建筑（避免闭包陷阱）
            GameObject current = prefab;

            // 实例化按钮并设置父物体
            GameObject newButton = Instantiate(ButtonPrefab, contentParent);

            // 设置按钮上的文本
            Text textComp = newButton.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = current.name;
            else
                Debug.LogWarning("按钮预制体缺少 Text 组件，无法显示建筑名称。");

            // 添加点击监听（复用单位按钮的点击处理方法）
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnPrefabButtonClicked(current));
            }
        }
    }
    public void SetBuildingPrefabsList(List<BuildingData> prefabs)
    {
        // 清除当前所有生成的按钮
        ClearButtons();

        // 如果列表为空，直接返回
        if (prefabs == null || prefabs.Count == 0)
            return;

        // 遍历建筑列表，生成对应按钮
        foreach (var prefab in prefabs)
        {
            // 保存当前建筑（避免闭包陷阱）
            BuildingData current = prefab;

            // 实例化按钮并设置父物体
            GameObject newButton = Instantiate(ButtonPrefab, contentParent);

            // 设置按钮上的文本
            Text textComp = newButton.GetComponentInChildren<Text>();
            if (textComp != null)
                textComp.text = current.buildingName;
            else
                Debug.LogWarning("按钮预制体缺少 Text 组件，无法显示建筑名称。");

            // 添加点击监听（复用单位按钮的点击处理方法）
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnBuildingPrefabButtonClicked(current));
            }
        }
    }

    /// <summary>
    /// 清除所有已生成的按钮
    /// </summary>
    private void ClearButtons()
    {
        // 遍历父物体下的所有子对象并销毁
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        //清空所有事件
        OnPrefabSelected = null;
        OnUnitPrefabSelected = null;
        OnUnitSelected = null;
        OnBuildingPrefabSelected=null;
    }

    /// <summary>
    /// 按钮点击时的内部处理方法
    /// </summary>
    /// <param name="unitName">被点击的单位名称</param>
    private void OnUnitButtonClicked(int index)
    {
        // 触发外部注册的事件
        OnUnitPrefabSelected?.Invoke(index);

        // 可选的默认行为（例如打印日志）
        Debug.Log($"单位 [{index}] 被选中");
    }
    private void OnUnitButtonClicked(BaseUnit unit)
    {
        // 触发外部注册的事件
        OnUnitSelected?.Invoke(unit);

        // 可选的默认行为（例如打印日志）
        Debug.Log($"单位 [{unit}] 被选中");
    }

    private void OnPrefabButtonClicked(GameObject prefab)
    {
        OnPrefabSelected?.Invoke(prefab);
    }
    private void OnBuildingPrefabButtonClicked(BuildingData prefab)
    {
        OnBuildingPrefabSelected?.Invoke(prefab);
    }
}