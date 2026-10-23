using UnityEngine;

public class FarmBuilding : BaseBuilding
{
    private float terrainBonus=1;//地形加成
    private int level=1;//等级
    public int Production=2;//产量

    protected override void Start()//初始化部分数据
    {
        base.Start();
        TimeManager.Instance.OnOneHourPulse += OneHour;
        var tile = GridManager.Instance.GetTerrainTile(GridPos);
        if (TileTypeManager.Instance.IsTargetTile(tile, TerrainTileType.FertileGrass))
        {
            terrainBonus=1.5f;
        }
    }

    protected override void OnDestroy()//销毁时移除事件
    {
        base.OnDestroy();
        TimeManager.Instance.OnOneHourPulse -= OneHour;
    }
    public float CountFinalProduction()//计算最终产量
    {
        float finalProduction=Production;
        switch (level)
        {
            case 1:
                finalProduction=OneLevel();
                break;
            case 2:
                finalProduction=TwoLevel();
                break;
            case 3:
                finalProduction=ThreeLevel();
                break;
            default:
                break;
        }
        finalProduction*=terrainBonus;
        return finalProduction;
    }
    public void OneHour()
    {
        float finalProduction=CountFinalProduction();
        ResourceSystem.AddFood((int)finalProduction);
    }

    public void Upgrade()
    {
        if (level>=3)
        {
            return;
        }
        level++;
    }

    private float OneLevel()//四领域,每相邻2农场,产量增加1
    {

        var neighbors = SpaceGet.GetNeighbors(GridPos);
        var buildings = BackgroundScan.GetBaseBuildings(neighbors);
        int farmCount=0;
        foreach (var building in buildings)
        {
            if (building is FarmBuilding)
            {
                farmCount++;
            }
        }
        int bonus=farmCount/2;
        return Production+bonus;
        
    }

    private float TwoLevel()//八领域,每相邻2农场,产量增加1
    {
        var neighbors = SpaceGet.GetNeighbors_8(GridPos);
        var buildings = BackgroundScan.GetBaseBuildings(neighbors);
        int farmCount=0;
        foreach (var building in buildings)
        {
            if (building is FarmBuilding)
            {
                farmCount++;
            }
        }
        int bonus=farmCount/2;
        return Production+bonus;
    }

    private float ThreeLevel()//八领域,每相邻1农场,产量增加1
    {

        var neighbors = SpaceGet.GetNeighbors_8(GridPos);
        var buildings = BackgroundScan.GetBaseBuildings(neighbors);
        int farmCount=0;
        foreach (var building in buildings)
        {
            if (building is FarmBuilding)
            {
                farmCount++;
            }
        }
        int bonus=farmCount;
        
        return Production+bonus;
    }
}