
using System.Collections;
using UnityEngine;

public class BaseBuilding : MonoBehaviour, IFactionMember, IOccupyEnity
{
    [SerializeField] private Faction faction = null;
    public Faction Faction { get { return faction; } set { faction = value; } }
    public Vector2 Position => transform.position;

    protected Vector2Int? currentGrid;          // 当前建筑占用的网格坐标
    public Vector2Int GridPos { get { return currentGrid.Value; } }
    public int maxHp = 100;
    public int hp = 100;                       // 建筑生命值
    private bool isInitialized=false;

    private static GridManager gridManager;
    protected static GridManager GridManager
    {
        get
        {
            if (gridManager == null)
                gridManager = GridManager.Instance;
            return gridManager;
        }
    }
    public void Initialize()//零初始化
    {
        // 初始化逻辑
        isInitialized = true;
    }
    public void Initialize(Faction faction)
    {
        isInitialized = true;
        Faction = faction;
        // 初始化时尝试占用当前位置的网格
        if (currentGrid == null)
        {
            var grid = GridManager.WorldToCell(transform.position);
            if (GridManager.TryOccupy(grid, this))
            {
                currentGrid = grid;
                transform.position = new Vector3(currentGrid.Value.x, currentGrid.Value.y, 0);
            }
        }
        GetComponent<SpriteRenderer>().color = faction.factionColor;
        gameObject.tag = faction.factionTag;
    }
    protected virtual void Start()
    {
        if (isInitialized) return;
        // 初始化时尝试占用当前位置的网格
        if (currentGrid == null)
        {
            var grid=GridManager.WorldToCell(transform.position);
            if (GridManager.TryOccupy(grid,this))
            {
                currentGrid = grid;
                transform.position = new Vector3(currentGrid.Value.x, currentGrid.Value.y, 0);
            }
        }
        GetComponent<SpriteRenderer>().color=faction.factionColor;
        gameObject.tag=faction.factionTag;
    }

    private void OnDestroy()
    {
        if(currentGrid != null)
            GridManager.Release((Vector2Int)currentGrid,this);
    }
    public virtual void TakeDamage(int damage)
    {
        hp=hp-damage;
        if(hp < 0)
        {
            Destroy(gameObject);
        }
    }

}