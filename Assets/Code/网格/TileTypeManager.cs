using UnityEngine.Tilemaps;
using UnityEngine;

public enum TerrainTileType
{
    Grass,
    FertileGrass,
}
public class TileTypeManager : MonoBehaviour
{
    public static TileTypeManager Instance;
    private void Awake()
    {
        Instance=this;
    }
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase fertileGrassTile;

    public bool IsTargetTile(TileBase tile, TerrainTileType targetType)
    {
        switch (targetType)
        {
            case TerrainTileType.Grass:
                return tile == grassTile;
            case TerrainTileType.FertileGrass:
                return tile == fertileGrassTile;
            default:
                return false;
        }
    }
}