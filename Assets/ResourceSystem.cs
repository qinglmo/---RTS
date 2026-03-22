using System;

public static class ResourceSystem
{
    // 资源数据
    private static int _food=100;
    private static int _wood=100;
    private static int _stone = 100;

    // 公开只读属性
    public static int Food => _food;
    public static int Wood => _wood;
    public static int Stone => _stone;

    // 增加资源
    public static void AddFood(int amount) { _food += amount; OnFoodChanged?.Invoke(_food); }
    public static void AddWood(int amount) { _wood += amount; OnWoodChanged?.Invoke(_wood); }
    public static void AddStone(int amount) { _stone += amount; OnStoneChanged?.Invoke(_stone); }

    // 消耗资源（返回是否成功）
    public static bool SpendFood(int amount)
    {
        if (_food < amount) return false;
        _food -= amount;
        OnFoodChanged?.Invoke(_food);
        return true;
    }

    public static bool SpendWood(int amount)
    {
        if (_wood < amount) return false;
        _wood -= amount;
        OnWoodChanged?.Invoke(_wood);
        return true;
    }

    public static bool SpendStone(int amount)
    {
        if (_stone < amount) return false;
        _stone -= amount;
        OnStoneChanged?.Invoke(_stone);
        return true;
    }
    public static bool TryBuild(int foodCost, int woodCost, int stoneCost)
    {
        if (Food < foodCost || Wood < woodCost || Stone < stoneCost) return false;
        SpendFood(foodCost);
        SpendWood(woodCost);
        SpendStone(stoneCost);
        return true;
    }

    // 资源变化事件
    public static event Action<int> OnFoodChanged;
    public static event Action<int> OnWoodChanged;
    public static event Action<int> OnStoneChanged;
}