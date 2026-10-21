using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单位信息面板（单例），支持按属性独立更新。
/// 通过事件驱动，外部可订阅各属性的更新方法。
/// </summary>
public class UnitInfoPanel : MonoBehaviour
{
    public static UnitInfoPanel Instance { get; private set; }

    private IListPanel listPanel;
    [Header("UI Text References")]
    public Text nameText;           // 单位名称
    public Text healthText;         // 血量（当前/最大）
    public Text attackText;         // 攻击力
    public Text defenseText;        // 防御力
    public Text impactText;         // 冲击积累值
    public Text positionText;       // 坐标
    public Text statusText;         // 状态
    public Text statusText2;         // 子状态
    public Text occupyGridText;//占领情况
    public Text currentTargetText;//当前攻击对象

    private BaseUnit lastUnit;
    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        listPanel=ListPanel.Instance;
        listPanel.OnUnitSelected += UpdateInfo;

    }
    private void Subscribe(BaseUnit unit)
    {
        lastUnit = unit;
        unit.attributes.OnHealthChanged += UpdateHealth;
        unit.attributes.OnAttackChanged += UpdateAttack;
        unit.attributes.OnDefenseChanged += UpdateDefense;
        unit.attributes.OnimpactQuantityChanged += UpdateImpact;
        unit.movement.stepMover.OnStepCompleted += UpdatePosition;
        //unit.mainState.StateMachine.OnUnitStateChanged += UpdateStatus;
        //unit.mainState.OnSecondaryStateChanged += UpdateStatus2;
        unit.movement.stepMover.OnOccupyChanged += UpdateOccupy;
        unit.intentionDecision.OnTargetChanged += UpdateCurrentTarget;

    }
    private void Unsubscribe()
    {
        if(lastUnit == null)
        {
            return; 
        }
        lastUnit.attributes.OnHealthChanged -= UpdateHealth;
        lastUnit.attributes.OnAttackChanged -= UpdateAttack;
        lastUnit.attributes.OnDefenseChanged -= UpdateDefense;
        lastUnit.attributes.OnimpactQuantityChanged -= UpdateImpact;
        lastUnit.movement.stepMover.OnStepCompleted -= UpdatePosition;
        //lastUnit.unit.mainState.StateMachine.OnUnitStateChanged -= UpdateStatus;
        //lastunit.mainState.OnSecondaryStateChanged -= UpdateStatus2;
        lastUnit.movement.stepMover.OnOccupyChanged -= UpdateOccupy;
        lastUnit.intentionDecision.OnTargetChanged -= UpdateCurrentTarget;
    }
    #region 独立属性更新方法（用于事件订阅）

    /// <summary>更新单位名称</summary>
    public void UpdateName(string unitName)
    {
        if (nameText != null)
            nameText.text = unitName;
    }

    /// <summary>更新血量（当前/最大）</summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    /// <summary>更新攻击力</summary>
    public void UpdateAttack(int attack)
    {
        if (attackText != null)
            attackText.text = attack.ToString();
    }

    /// <summary>更新防御力</summary>
    public void UpdateDefense(int defense)
    {
        if (defenseText != null)
            defenseText.text = defense.ToString();
    }

    /// <summary>更新冲击积累值</summary>
    public void UpdateImpact(float impact)
    {
        if (impactText != null)
            impactText.text = impact.ToString();
    }

    /// <summary>更新坐标（网格位置）</summary>
    public void UpdatePosition(Vector2Int gridPos)
    {
        if (positionText != null)
            positionText.text = $"({gridPos.x:F1}, {gridPos.y:F1})";
    }

    /// <summary>更新状态</summary>
    public void UpdateStatus(BaseUnit unit , IntentionDecision IntentionState)
    {
        switch (IntentionState.decisionExecution.GetCurrentState())
            {
                
                case UnitState.Stunned:statusText.text = "麻痹";
                    break;
                default:
                {
                    switch(IntentionState.currentIntentStatus)
                    {
                        case IntentionDecision.IntentStatus.Action:
                            {
                                var target = IntentionState.CurrentTarget as MonoBehaviour;
                                if (target != null)
                                {
                                    statusText.text = $"攻击 {target.name}";
                                }
                                else
                                {
                                    statusText.text = "攻击";
                                }
                            }
                            break;
                        case IntentionDecision.IntentStatus.Chase:
                            {
                                var target = IntentionState.CurrentTarget as MonoBehaviour;
                                if (target != null)
                                {
                                    statusText.text = $"追逐 {target.name}";
                                }
                                else
                                {
                                    statusText.text = "追逐";
                                }
                            }
                            break;
                        case IntentionDecision.IntentStatus.Find:
                            {
                                var target = IntentionState.CurrentTarget as MonoBehaviour;
                                if (target != null)
                                {
                                    statusText.text = $"接近 {target.name}";
                                }
                                else
                                {
                                    statusText.text = "接近";
                                }
                            }
                            break;
                        case IntentionDecision.IntentStatus.Idle:statusText.text = "待机";
                            break;
                        default:statusText.text = "未知";
                            break;
                    }
                }
                    break;
            }
    }

    public void UpdateStatus2( UnitState status)//
    {
        if (statusText != null)
            statusText2.text = status.ToString();
    }
    public void UpdateOccupy(bool occupy)
    {
        if (occupy==false)
        {
            occupyGridText.text = "空";
        }
        else
        {
            occupyGridText.text = "占领";
        }   
    }
    public void UpdateCurrentTarget(IHasPosition target)
    {
        if(target as MonoBehaviour == null)
        {
            currentTargetText.text = "无";
        }
        else
        {
            var mono = target as MonoBehaviour;
            currentTargetText.text=mono.name;
        }
    }
    #endregion
    #region 一次性全量更新（可选）

    /// <summary>
    /// 一次性更新所有信息（内部调用独立方法）
    /// </summary>
    private void UpdateInfo(string unitName, int currentHealth, int maxHealth, int attack, int defense, float impact, Vector2Int gridPos, IntentionDecision IntentionState)
    {
        UpdateName(unitName);
        UpdateHealth(currentHealth, maxHealth);
        UpdateAttack(attack);
        UpdateDefense(defense);
        UpdateImpact(impact);
        UpdatePosition(gridPos);
        UpdateStatus(null, IntentionState);
    }

     public void UpdateInfo(BaseUnit unit) 
    {
        Unsubscribe();
        Subscribe(unit);
        UpdateInfo(
        unit.name, unit.attributes.CurrentHealth, unit.attributes.MaxHealth,
        unit.attributes.BaseAttack, unit.attributes.BaseDefense, unit.attributes.impactQuantity,
        unit.GridPos, unit.intentionDecision
         );
        UpdateOccupy(unit.movement.stepMover.IsOccupy);
        UpdateCurrentTarget(unit.intentionDecision.CurrentTarget);
    }

    #endregion
}