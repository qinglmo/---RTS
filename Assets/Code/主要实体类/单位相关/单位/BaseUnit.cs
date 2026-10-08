using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BaseUnit : MonoBehaviour ,IFactionMember, IOccupyEnity
{
    [SerializeField] private Faction faction = null;
    public Faction Faction { get { return faction; } 
        set { 
            faction = value;
            gameObject.tag=faction.factionTag;
        } 
    }
    public Vector2 Position => transform.position;
    public Vector2Int GridPos => movement.stepMover.CurrentPos;
    [Header("基础组件")]
    public UnitMovement movement;
    [SerializeReference] public UnitAttributes attributes = new UnitAttributes();
    public IntentionDecision intentionDecision;//这个组件激活时依赖阵营属性，未来需要考虑顺序问题
    public UnitVisualEffect unitVisualEffect; 

    [Header("选择高亮")]
    public SpriteRenderer selectSprite;
    protected bool isSelected = false;
    [Header("状态高亮")]
    public SpriteRenderer StateSprite;

    private bool isInitialize=false;
    public virtual void Initialize(int hp, int ack,Faction faction)
    {
        attributes.Initialize(hp, hp, ack, 1);
        isInitialize = true;
        Faction = faction;
        GetComponentInChildren<SpriteRenderer>().color = Faction.factionColor;
        EventBus_Unit.InvokeActivated(this);
    }
    protected virtual void Awake()
    {
        // 自动获取或添加必要组件
        movement = GetComponent<UnitMovement>() ?? gameObject.AddComponent<UnitMovement>();
        if(attributes==null)
            attributes = new UnitAttributes();
        intentionDecision = GetComponent<IntentionDecision>() ?? gameObject.AddComponent<IntentionDecision>();
        unitVisualEffect = GetComponent<UnitVisualEffect>()??gameObject.AddComponent<UnitVisualEffect>();

    }
    protected void OnEnable()
    {
        if(faction!=null)
            EventBus_Unit.InvokeActivated(this);
    }
    protected void OnDisable()
    {
        EventBus_Unit.InvokeDeactivated(this);
    }
    protected virtual void Start()
    {
        
        movement.Initialize();
        transform.position = GridManager.Instance.CellToWorld(GridPos);
        attributes.OnDestroyEvent += OnDeathEvent;
    }
    public virtual void Update()
    {
        
    }
    
    public virtual void SetSelected(bool selected)
    {
        isSelected = selected;
        if(selectSprite != null)
            selectSprite.enabled = selected;
    }
    protected void OnDeathEvent()
    {
        attributes.OnDestroyEvent -= OnDeathEvent;
        Destroy(gameObject);
    }
    protected virtual void OnDestroy()
    {
        
    }
    public void MoveToOther()//让单位移动到其他位置
    {
        RouteDecision.MoveToOther(this);
    }

    public void UpdateAbility(float multiplier)//更新单位能力
    {
        attributes.MaxHealth = Mathf.RoundToInt(attributes.base_maxHealth*multiplier);
    }
}