using UnityEngine;
public interface IGridEntityProvider
{
    public IFactionMember GetFactionMember(Vector2Int pos);
    public BaseUnit GetUnit(Vector2Int pos);
    public BaseBuilding GetBuilding(Vector2Int pos);
    public SurfaceResources GetResource(Vector2Int pos);
}
public class GridEntityProvider : MonoBehaviour, IGridEntityProvider
{
    public static GridEntityProvider Instance;
    private void Awake()
    {
        Instance = this;
    }
    public IFactionMember GetFactionMember(Vector2Int pos)
    {
        var entity = GridManager.Instance.GetOccupiedEnity(pos);
        if(entity is IFactionMember)
        {
            return entity as IFactionMember;
        }
        return null;
    }
    public BaseUnit GetUnit(Vector2Int pos)
    {
        var entity = GridManager.Instance.GetOccupiedEnity(pos);
        if(entity is BaseUnit)
        {
            return entity as BaseUnit;
        }
        return null;
    }
    public BaseBuilding GetBuilding(Vector2Int pos)
    {
        var entity = GridManager.Instance.GetOccupiedEnity(pos);
        if(entity is BaseBuilding)
        {
            return entity as BaseBuilding;
        }
        return null;
    }
    public SurfaceResources GetResource(Vector2Int pos)
    {
        var entity = GridManager.Instance.GetOccupiedEnity(pos);
        if(entity is SurfaceResources)
        {
            return entity as SurfaceResources;
        }
        return null;
    }
}