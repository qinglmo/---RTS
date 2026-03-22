using System.Collections.Generic;
using UnityEngine;

public class UnitSkillsManager : MonoBehaviour
{
    [Header("默认高亮预制体（全局）")]
    [SerializeField] private GameObject defaultHighlightPrefab;

    [Header("技能数据列表")]
    [SerializeField] private List<SkillData> skillDataList; // 在Inspector中配置

    // 运行时技能状态
    private List<SkillInstance> skillInstances = new List<SkillInstance>();

    private SkillInstance currentAimingSkill;

    private List<Vector2Int> highlightedCells = new List<Vector2Int>();
    private List<GameObject> activeIndicators = new List<GameObject>();
    private Camera mainCamera;
    private BaseUnit hero;

    private static GridManager gridManager;
    private static GridManager GridManager
    {
        get
        {
            if (gridManager == null)
                gridManager = GridManager.Instance;
            return gridManager;
        }
    }
    private class SkillInstance
    {
        public SkillData data;
        public float cooldownTimer; // 冷却剩余时间
        public bool IsOnCooldown => cooldownTimer > 0;

        public SkillInstance(SkillData data)
        {
            this.data = data;
            cooldownTimer = 0;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (cooldownTimer > 0) cooldownTimer -= deltaTime;
        }
    }

    private void Awake()
    {
        hero = GetComponent<BaseUnit>();
        mainCamera = Camera.main;

        // 创建技能实例
        foreach (var data in skillDataList)
        {
            skillInstances.Add(new SkillInstance(data));
        }
    }

    private void Update()
    {
        // 更新冷却
        foreach (var inst in skillInstances)
        {
            inst.UpdateCooldown(Time.deltaTime);
        }

        if (currentAimingSkill != null)
        {
            HandleAimingInput();
        }
    }

    /// <summary>
    /// 由UI调用，尝试激活指定索引的技能
    /// </summary>
    public bool TryActivateSkill(int index)
    {
        MouseControl.Instance.TemporarilyIgnoreInput();
        if (index < 0 || index >= skillInstances.Count) return false;
        var inst = skillInstances[index];
        if (inst.IsOnCooldown) return false;

        // 如果有其他技能在瞄准，强制取消
        if (currentAimingSkill != null)
        {
            CancelAiming();
        }

        currentAimingSkill = inst;
        MouseControl.Instance.isSkillAiming = true;

        Debug.Log("触发技能" + currentAimingSkill.data.skillName);
        ShowTargetsForSkill(inst.data);
        return true;
    }

    private void ShowTargetsForSkill(SkillData data)
    {
        ClearHighlights();

        Vector2Int heroPos = hero.movement.stepMover.CurrentPos;

        if (data.rangeType == SkillRangeType.Circle)
        {
            ShowCircleTargets(heroPos, data.rangeValue);
        }
        else if (data.rangeType == SkillRangeType.Line)
        {
            ShowLineTargets(heroPos, Mathf.RoundToInt(data.rangeValue));
        }
    }

    private void ShowCircleTargets(Vector2Int heroPos, float radius)
    {
        Vector3 heroWorld = GridManager.CellToWorld(heroPos);
        int rangeInCells = Mathf.CeilToInt(radius);
        for (int x = heroPos.x - rangeInCells; x <= heroPos.x + rangeInCells; x++)
        {
            for (int y = heroPos.y - rangeInCells; y <= heroPos.y + rangeInCells; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector3 cellWorld = GridManager.CellToWorld(cell);
                if (Vector3.Distance(cellWorld, heroWorld) <= radius)
                {
                    HighlightCell(cell);
                }
            }
        }
    }

    private void ShowLineTargets(Vector2Int heroPos, int length)
    {
        // 四个方向
        for (int i = 1; i <= length; i++)
        {
            HighlightCell(new Vector2Int(heroPos.x, heroPos.y + i));
            HighlightCell(new Vector2Int(heroPos.x, heroPos.y - i));
            HighlightCell(new Vector2Int(heroPos.x + i, heroPos.y));
            HighlightCell(new Vector2Int(heroPos.x - i, heroPos.y));
        }
    }

    private void HighlightCell(Vector2Int cell)
    {
        GameObject prefab=null;
        if (currentAimingSkill.data.highlightPrefab != null)
        {
            prefab=currentAimingSkill.data.highlightPrefab;
        }
        else if(defaultHighlightPrefab != null)
        {
            prefab = defaultHighlightPrefab;
        }
        
        if (prefab == null) return;
        Vector3 worldPos = GridManager.CellToWorld(cell);
        GameObject indicator = Instantiate(prefab, worldPos, Quaternion.identity);
        activeIndicators.Add(indicator);
        highlightedCells.Add(cell);
    }

    private void ClearHighlights()
    {
        foreach (var ind in activeIndicators)
            Destroy(ind);
        activeIndicators.Clear();
        highlightedCells.Clear();
    }

    private void HandleAimingInput()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int? targetGrid = GridManager.WorldToCell(mouseWorld);

        if (targetGrid.HasValue && highlightedCells.Contains(targetGrid.Value))
        {
            // 鼠标悬浮可加特效

            if (Input.GetMouseButtonDown(0))
            {
                ExecuteSkill(targetGrid.Value);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelAiming();
        }
    }

    private void ExecuteSkill(Vector2Int targetCell)
    {
        if (currentAimingSkill == null) return;

        // 执行效果
        currentAimingSkill.data.effect.Execute(hero, targetCell);

        // 进入冷却
        currentAimingSkill.cooldownTimer = currentAimingSkill.data.cooldown;

        // 退出瞄准
        CancelAiming();
    }

    private void CancelAiming()
    {
        currentAimingSkill = null;
        MouseControl.Instance.isSkillAiming = false;
        MouseControl.Instance.TemporarilyIgnoreInput();
        ClearHighlights();
    }

    // UI 获取技能信息的方法
    public IReadOnlyList<SkillData> GetSkillDataList() => skillDataList;
    public float GetCooldownProgress(int index)
    {
        if (index < 0 || index >= skillInstances.Count) return 1f;
        var inst = skillInstances[index];
        return inst.IsOnCooldown ? (inst.cooldownTimer / inst.data.cooldown) : 0f;
    }
}