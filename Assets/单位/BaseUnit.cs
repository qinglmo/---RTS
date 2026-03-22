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
    public TargetDetector detector;//这个组件激活时依赖阵营属性，未来需要考虑顺序问题
    public UnitVisualEffect unitVisualEffect; 
    public UnitCommander UnitCommander;
    public MainState mainState;//这个有大量引用，最好放在最后面初始化
    [Header("选择高亮")]
    public SpriteRenderer selectSprite;
    protected bool isSelected = false;
    [Header("状态高亮")]
    public SpriteRenderer StateSprite;
    
    //移动用
    public Vector2Int? commandMovePosition;
    public BaseUnit commandAttackTarget;
    private bool isInitialize=false;
    public virtual void Initialize(int hp, int ack,Faction faction)
    {
        attributes.Initialize(hp, hp, ack, 1);
        isInitialize = true;
        Faction = faction;
        GetComponentInChildren<SpriteRenderer>().color = Faction.factionColor;
    }
    protected virtual void Awake()
    {
        // 自动获取或添加必要组件
        movement = GetComponent<UnitMovement>() ?? gameObject.AddComponent<UnitMovement>();
        if(attributes==null)
            attributes = new UnitAttributes();
        detector = GetComponent<TargetDetector>() ?? gameObject.AddComponent<TargetDetector>();
        UnitCommander = GetComponent<UnitCommander>() ?? gameObject.AddComponent<UnitCommander>();
        unitVisualEffect = GetComponent<UnitVisualEffect>()??gameObject.AddComponent<UnitVisualEffect>();
        mainState = GetComponent<MainState>() ?? gameObject.AddComponent<MainState>();

    }
    protected virtual void Start()
    {
        movement.Initialize();
        transform.position = (Vector2)movement.stepMover.CurrentPos;
        attributes.OnDestroyEvent += OnDeathEvent;
        mainState.Initialize(this);
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
}
