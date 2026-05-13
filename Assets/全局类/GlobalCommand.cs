using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public  class GlobalCommand :SingletonMono<GlobalCommand>
{
    public Faction Faction;
    [SerializeField] private GameObject cursorPrefab; // 命令图标预制体，需在Inspector中赋值
    public Vector2Int? TotalAttackTarget;
    public Vector2Int? TotalDefenseTarget;
    private List<string> commands = new List<string>();
    private GameObject cursorObject;
    private enum CommandMode
    {
        None,
        Attack,
        Defense
    }
    private void Awake()
    {
        commands.Add("攻击");
        commands.Add("防守");
        commands.Add("清空");
    }
    private CommandMode currentMode = CommandMode.None;
    private void Update()
    {
        if (currentMode == CommandMode.None) return;

        // 跟随鼠标，吸附到网格中心
        Vector3 worldPos = GetGridCenterFromMouse();
        if (cursorObject != null)
            cursorObject.transform.position = worldPos;

        // 鼠标左键点击触发
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int gridPos = GetGridPositionFromMouse();
            switch (currentMode)
            {
                case CommandMode.Attack:
                    SetAttackTarget(gridPos);
                    break;
                case CommandMode.Defense:
                    SetDefenseTarget(gridPos);
                    break;
            }
            CancelCommandMode();
        }

        // 右键或ESC取消命令
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCommandMode();
        }
    }
    public void SetAttackTarget(Vector2Int pos) 
    {
        TotalDefenseTarget = null;
        TotalAttackTarget = pos;
        foreach (var unit in FactionManager.Instance.FactionUnits[Faction])
        {
            unit.AttackTarget = pos;
        }
    }
    public void SetDefenseTarget(Vector2Int pos)
    {
        TotalAttackTarget = null;
        TotalDefenseTarget = pos;
        foreach (var unit in FactionManager.Instance.FactionUnits[Faction])
        {
            unit.DefenseTarget = pos;
        }
    }
    public void ClearAllTarget()
    {
        TotalAttackTarget=null;
        TotalDefenseTarget=null;
    }

    public void OpenCommandMenu()
    {
        ListPanel.Instance.ShowPanel();
        ListPanel.Instance.SetButtonList(commands);
        ListPanel.Instance.OnButtonSelected += ButtonTrriger;
    }
    private void ButtonTrriger(int index)
    {
        switch(index)
        {
            case 0:
                StartCommandMode(CommandMode.Attack);
                break;
            case 1:
                StartCommandMode(CommandMode.Defense);
                break;
            case 2:ClearAllTarget();
                break;
            default:
                break;
        }
    }
    private void StartCommandMode(CommandMode mode)
    {
        CancelCommandMode(); // 清除之前可能残留的命令模式
        currentMode = mode;

        if (cursorPrefab != null)
        {
            cursorObject = Instantiate(cursorPrefab);
        }
        else
        {
            // 如果没有预制体，动态创建一个简单的Sprite用于调试（可选）
            cursorObject = new GameObject("CommandCursor");
            var sr = cursorObject.AddComponent<SpriteRenderer>();
            sr.color = mode == CommandMode.Attack ? Color.red : Color.blue;
            // 需要添加一个默认sprite，可在编辑器中设置或动态创建
        }
    }

    private void CancelCommandMode()
    {
        currentMode = CommandMode.None;
        if (cursorObject != null)
            Destroy(cursorObject);
    }
    // 获取鼠标所在位置的网格中心点（世界坐标）
    private Vector3 GetGridCenterFromMouse()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0; // 假设是2D平面，根据需要调整
        return (Vector2)GridManager.Instance.WorldToCell(worldPos);
    }

    // 获取鼠标所在位置的网格坐标（整数索引）
    private Vector2Int GetGridPositionFromMouse()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return GridManager.Instance.WorldToCell(worldPos);
    }
}
