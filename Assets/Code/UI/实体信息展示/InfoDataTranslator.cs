using System.Collections.Generic;
using UnityEngine;

public interface IInfoDataProvider
{
    List<string> GetInfoLabels(IOccupyEnity target);
    List<string> GetInfoValues(IOccupyEnity target);
}

public class InfoDataTranslator : IInfoDataProvider
{
    private static InfoDataTranslator instance;
    public static InfoDataTranslator Instance
    {
        get
        {
            if (instance == null)
                instance = new InfoDataTranslator();
            return instance;
        }
    }

    public List<string> GetInfoLabels(IOccupyEnity target)
    {
        var labels=new List<string>();
        labels.Add("名字");
        // 根据对象类型组织标签及其顺序
        if(target is BaseUnit)
        {
            
            labels.Add("生命值");
            labels.Add("攻击值");
            labels.Add("防御值");
            labels.Add("眩晕积累");
            labels.Add("坐标");
            labels.Add("当前意图");
            labels.Add("当前动作");
            labels.Add("当前目标");
        }
        else if(target is BaseBuilding)
        {
            labels.Add("生命值");
            if(target is FarmBuilding)
            {
                labels.Add("生产类型");
                labels.Add("生产数量");
            }
        }
        return labels;
    }

    public List<string> GetInfoValues(IOccupyEnity target)
    {
        // 根据对象类型读取对应数值
        var values = new List<string>();
        values.Add((target as MonoBehaviour).name);
        if(target is BaseUnit unit)
        {
            values.Add(unit.attributes.CurrentHealth.ToString()+"/"+unit.attributes.MaxHealth.ToString());
            values.Add(unit.attributes.BaseAttack.ToString());
            values.Add(unit.attributes.BaseDefense.ToString());
            values.Add(unit.attributes.Toughness.ToString());
            values.Add("(" +unit.GridPos.x+","+unit.GridPos.y+")");
            values.Add(unit.intentionDecision.currentIntentType.ToString());
            values.Add(unit.intentionDecision.currentIntentStatus.ToString());
            values.Add((unit.intentionDecision.CurrentTarget as MonoBehaviour)?.name??"无");
        }
        else if(target is BaseBuilding building)
        {
            values.Add(building.hp.ToString()+"/"+building.maxHp.ToString());
            if(building is FarmBuilding farmBuilding)
            {
                values.Add("食物");
                values.Add(farmBuilding.CountFinalProduction().ToString());
            }
        }
        return values;
    }
}