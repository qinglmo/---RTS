using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseControl : MonoBehaviour
{
    private static MouseControl instance;
    public static MouseControl Instance => instance;

    private List<BaseUnit> selectedUnits = new List<BaseUnit>();

    [Header("射线检测层（可选）")]
    public LayerMask unitLayer;

    [Header("框选设置")]
    public float boxSelectThreshold = 10f; // 触发框选的最小拖拽像素距离

    // 框选状态
    private bool isDragging = false;
    private bool isBoxSelecting = false;
    private Vector2 dragStartScreenPos;
    private Vector2 dragCurrentScreenPos;

    public bool isSkillAiming=false;

    private float ignoreInputUntilTime;      // 屏蔽输入的时间点

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        // 如果当前时间在屏蔽窗口内，跳过所有输入处理
        if (Time.time < ignoreInputUntilTime)
            return;
        if (isSkillAiming)
            return;
        // --- 鼠标左键处理（点击 + 框选）---
        if (Input.GetMouseButtonDown(0))
        {
            
            // 按下左键，记录起始点
            isDragging = true;
            isBoxSelecting = false;
            dragStartScreenPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging && !isBoxSelecting)
        {
            // 如果移动距离超过阈值，进入框选模式
            if (Vector2.Distance(Input.mousePosition, dragStartScreenPos) > boxSelectThreshold)
            {
                isBoxSelecting = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isBoxSelecting)
            {
                // 框选结束：获取框选区域内的单位并触发事件
                dragCurrentScreenPos = Input.mousePosition;
                List<BaseUnit> boxedUnits = GetUnitsInBox(dragStartScreenPos, dragCurrentScreenPos);
                HandleBoxSelection(boxedUnits);
            }
            else
            {
                // 普通点击
                HandleLeftClick();

            }

            // 重置状态
            isDragging = false;
            isBoxSelecting = false;
        }

        // --- 右键处理（统一发布移动命令）---
        HandleRightClick();


    }
    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            foreach (var unit in selectedUnits)
            {
                if (unit != null)
                {
                    //清除其他命令
                    unit.UnitCommander.ClearAllCommands();
                }
            }
            // 1. 射线检测鼠标下的物体
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // 2. 如果点击到物体
            if (hit.collider != null)
            {
                GameObject clickedObject = hit.collider.gameObject;

                // 2.1 点击到敌人（标签为 "Enemy"）
                if (clickedObject.CompareTag("Enemy"))
                {
                    foreach (var unit in selectedUnits)
                    {
                        if (unit != null)
                        {
                            unit.commandAttackTarget=clickedObject.GetComponent<BaseUnit>();
                            unit.UnitCommander.AddCommand(new ChaseAttackCommand());
                        }
                    }
                    return; // 指令已下发，跳过默认移动
                }

                // 2.2 点击到可驻扎建筑
                IStationIn building = clickedObject.GetComponent<IStationIn>();
                if (building != null && building.CanOccupy())
                {
                    foreach (var unit in selectedUnits)
                    {
                        if (unit != null)
                        {
                            var pos = clickedObject.transform.position;
                            unit.commandMovePosition = new Vector2Int((int)pos.x, (int)pos.y);
                            unit.UnitCommander.AddCommand(new MoveOccupyCommand());
                        }
                    }
                    return; // 指令已下发，跳过默认移动
                }
            }
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 targetPos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // 统一向所有选中单位发布移动命令
            foreach (var unit in selectedUnits)
            {
                if (unit != null)
                {
                    MoveUnit(unit, targetPos);
                }
            }
        }
    }
    private void MoveUnit(BaseUnit unit, Vector2 target)
    {
        // 尝试转换为具体Unit执行移动
        Unit concreteUnit = unit as Unit;
        if (concreteUnit != null)
        {
            concreteUnit.MoveToGridPos(GridManager.Instance.WorldToCell(target));
        }
        // 其他BaseUnit子类可以在这里扩展
    }
    private void HandleBoxSelection(List<BaseUnit> boxedUnits)
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (!ctrlHeld)
        {
            // 不按 Ctrl：替换选中
            ClearSelection();
            ListPanel.Instance.SetUnitList(boxedUnits);
            foreach (var unit in boxedUnits)
            {
                AddToSelection(unit);
            }
                
        }
        else
        {
            // 按 Ctrl：对框内每个单位切换选中
            foreach (var unit in boxedUnits)
            {
                if (selectedUnits.Contains(unit))
                    RemoveFromSelection(unit);
                else
                    AddToSelection(unit);
            }
        }
    }
    private void HandleLeftClick()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return; // 如果点在 UI 上，忽略世界点击
        // 发射射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray); // 对于2D，使用Physics2D
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (hit.collider != null)
        {
            // 尝试获取 UnitMove 组件
            BaseUnit clickedUnit = hit.collider.GetComponent<BaseUnit>();

            if (clickedUnit != null)
            {
                if (shiftHeld)
                {
                    // Ctrl + 左键：切换选中状态
                    if (selectedUnits.Contains(clickedUnit))
                        RemoveFromSelection(clickedUnit);
                    else
                        AddToSelection(clickedUnit);
                }
                else
                {
                    // 普通左键：清除之前选中，只选中当前单位
                    ClearSelection();
                    AddToSelection(clickedUnit);
                    UnitInfoPanel.Instance.UpdateInfo(clickedUnit);
                }
                return;
            }
            BaseBuilding baseBuilding= hit.collider.GetComponent<BaseBuilding>();
            if (baseBuilding != null)
            {
                Debug.Log("检测到建筑");
                if(baseBuilding is BuildingHospital)
                {
                    var hospital= (BuildingHospital)baseBuilding;
                    ListPanel.Instance.SetUnitList(hospital.ReadUnitList());
                }
                if(baseBuilding is BuildingBarracks)
                {
                    var barracks= (BuildingBarracks)baseBuilding;
                    ListPanel.Instance.SetPrefabList_Index(barracks.ReadUnitList());
                    ListPanel.Instance.OnUnitPrefabSelected = barracks.GenerateUnit;
                }
            }

        }
        else
        {
            // 点击空白区域：如果没有按Ctrl，清除所有选中
            if (!shiftHeld)
                ClearSelection();
            ListPanel.Instance.HidePanel();
        }
    }
    private List<BaseUnit> GetUnitsInBox(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector3 startWorld = Camera.main.ScreenToWorldPoint(screenStart);
        Vector3 endWorld = Camera.main.ScreenToWorldPoint(screenEnd);

        float minX = Mathf.Min(startWorld.x, endWorld.x);
        float maxX = Mathf.Max(startWorld.x, endWorld.x);
        float minY = Mathf.Min(startWorld.y, endWorld.y);
        float maxY = Mathf.Max(startWorld.y, endWorld.y);

        Collider2D[] colliders = Physics2D.OverlapAreaAll(new Vector2(minX, minY), new Vector2(maxX, maxY), unitLayer);

        List<BaseUnit> unitsInBox = new List<BaseUnit>();
        foreach (var col in colliders)
        {
            BaseUnit unit = col.GetComponent<BaseUnit>();
            if (unit != null && !unitsInBox.Contains(unit))
                unitsInBox.Add(unit);
        }
        return unitsInBox;
    }
    private void AddToSelection(BaseUnit unit)
    {
        if (unit == null || selectedUnits.Contains(unit)) return;
        selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    private void RemoveFromSelection(BaseUnit unit)
    {
        if (unit == null || !selectedUnits.Contains(unit)) return;
        selectedUnits.Remove(unit);
        unit.SetSelected(false);
    }

    private void ClearSelection()
    {
        foreach (BaseUnit unit in selectedUnits)
        {
            if (unit != null)
                unit.SetSelected(false);
        }
        selectedUnits.Clear();
    }
    // 在技能释放后调用此方法，短暂屏蔽输入
    public void TemporarilyIgnoreInput(float duration = 0.1f)
    {
        ignoreInputUntilTime = Time.time + duration;
    }
    // 可选：绘制框选矩形（调试用）
    private void OnDrawGizmos()
    {
        if (isBoxSelecting)
        {
            Vector3 start = Camera.main.ScreenToWorldPoint(dragStartScreenPos);
            Vector3 current = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 min = new Vector3(Mathf.Min(start.x, current.x), Mathf.Min(start.y, current.y), 0);
            Vector3 max = new Vector3(Mathf.Max(start.x, current.x), Mathf.Max(start.y, current.y), 0);
            Vector3 size = max - min;
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawCube(min + size * 0.5f, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(min + size * 0.5f, size);
        }
        else
        {

        }
    }
}
