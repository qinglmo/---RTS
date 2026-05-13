using Unity.VisualScripting;
using UnityEngine;

//定义单位和维护资源的关系，目标主要指食物。
public class MaintenanceComponent : MonoBehaviour
{
    //不能让单位自动调整消耗食物，而是既可以玩家指定消耗，也有单位自身消耗的限制。
    //也不能让玩家无脑拉高收益，食物消耗收益要有边界效益。
    //培养单位时鼓励长期维护，但也要临时维护有效果。

    //意图理解，玩家配置项的意思是，玩家设置的供给等级，维持高供给，单位会逐渐增加食物摄入。

    //错误的经验，其实不应该用现实中的肌肉模拟单位能力变化。这个属于设计视角的错误。实际上当前类的意图是更多食物，所以单位更强的能力。
    //如果用现在中真实的肌肉增长模型，就会发现，食物成为维护单位能力的惩罚，而不是用于激励单位能力。
    //而实际上，我的设计意图是食物激励单位能力，而非食物锻炼单位能力，因为锻炼这件事有一个最佳消耗食物比，不能无脑增加食物，这会减少食物收益，而微调会破坏游戏体验。
    //如果我喜欢锻炼模型，那么我应该明确的写出锻炼需要消耗的食物量，让玩家自己操作，消耗一定量食物和时间升级单位，然后改变单位的食物消耗量即可。并且允许玩家减少供给，从而降级。
    //不过我也不能真的让玩家微操每个单位，所以可以设置一个锻炼供给的配置。
    //想了想，还是决定放弃锻炼模型，转向食物激励模型，因为食物激励模型可以提供更多的食物，玩家玩起来也合理点，而且实现也简单。
    public float foodCost=2.0f;//基础维护成本
    public float playerSetCost=1.0f;//玩家指定供给量,最大4.0，最小0.5。让玩家在食物紧缺时能够降低供给量，让单位自然降级。
    public float currentNeedCost=1.0f;//当前需要维护成本，影响单位能力变化。可以理解为肌肉量，当能量池见底时，会损耗肌肉，且只有能量池满时，才能增长肌肉。
    public float energy=1.0f;//当前能量池，影响单位能力变化.过低会下降小幅度战斗能力。最高1.0，最低0。少有惩罚，多没有收益。且能量池满时才会增加肌肉量。    
    public float MaxFoodCost=4;//长期最大维护成本
    public float MinFoodCost=0.5f;//长期最小维护成本，低维护费就是单位亏空状态，不能等价为正常状态，该状态下食物消耗减少同时能力下降。


    public float PlayerSetCost
    {
        get
        {
            return playerSetCost;
        }
        set
        {
            playerSetCost = Mathf.Clamp(value, MinFoodCost, 2.0f);
        }
    }
    private BaseUnit unit;

    void Awake()
    {
        unit = GetComponent<BaseUnit>();
    }
    public void Start()
    {
        TimeManager.OnOneDayPulse += DataUpdate;
    }
    public void DataUpdate()
    {
        
        float currentActualCost=0;//当前实际维护成本，影响单位能力变化
        if (PlayerSetCost >=currentNeedCost)//不允许过度激励，最高0.8f
        {
            currentActualCost=Mathf.Min(currentNeedCost+0.8f,PlayerSetCost);
        }
        else
        {
            currentActualCost=PlayerSetCost;//允许立即降低供给
        }
        var Cost = foodCost * currentActualCost; 
        var success= ResourceSystem.SpendFood(Mathf.FloorToInt(Cost));//这是暂时的，允许少消耗食物。
        if (success)
        {
            if (energy < 0.9f)
            {
                energy=1;
            }
            else
            {
                if(currentActualCost>=currentNeedCost)
                {
                    currentNeedCost=Mathf.Min(currentNeedCost+0.3f,currentActualCost);
                }
                else
                {
                    currentNeedCost=Mathf.Max(currentNeedCost-0.5f,currentActualCost);
                }
            }
            
        }
        else//饥饿，应该有惩罚。发生饥饿的场景是玩家没有任何食物，所以不需要再去缩小食物需求，算起来太麻烦。
        {
            if (energy >= 0.9f)
            {
                energy=0;
            }
            else
            {
                currentNeedCost-=1f;
                if(currentNeedCost<0.5f)
                {
                    //一旦低于该值，可以造成不可逆损伤或者死亡。具体效果之后扩展
                }
                currentNeedCost = Mathf.Clamp(currentNeedCost, MinFoodCost, MaxFoodCost);
                currentActualCost=currentNeedCost;
            }
        }
        var energyMultiplier=1f;
        if (energy == 0)
        {
            energyMultiplier=0.8f;
        }
        unit.UpdateAbility(Mathf.Sqrt(currentActualCost)*energyMultiplier);
    }
    public void OnDestroy()
    {
        TimeManager.OnOneDayPulse -= DataUpdate;
    }
}