public enum UnitState
{
    Top_Attack,//战斗状态
    Top_Heal,//恢复状态，残血从战斗状态进入恢复状态，目前是逃跑状态。需要屏蔽发现敌人，直到满血才退出该状态。
    Top_Work,//工作状态
    Top_Stun,//眩晕状态
    Top_Advance,//推进状态，朝着目标前进，直到可以在一个设定距离内看到目标即可。期间可以短暂追击，执行攻击，也可以逃跑。
                //推进路线可以自主计算，也可以从全局类拉取。会定义一个回到路线和执行路线的新状态。
    Top_Defense,//防守状态，守备目标点，可以距离目标点一个设定范围内自由移动，可以略微超出部分范围追击，一旦超出最大范围，便强行执行回防逻辑，无视敌方攻击。


    Chaseing,   // 追击
    Attacking,   // 攻击
    Finding,//寻找敌人最后消失的位置
    Supporting,//支援友军
    TargetAdvancement,//目标推进，单位没有具体战斗细节时触发，目标通常由AI或者玩家派发。
    //战斗状态也持有空闲状态，由玩家切换是否参与战斗，让空闲状态可以平滑切换到追击或者攻击或者寻敌状态。
    ReturnRoute,

    Escaping,//逃跑状态

    Wandering,   // 游荡
    Nothing,     //空闲状态。
    Working,//工作-采集
    ApprochResource,//接近资源
    ReturnToBase,//返回基地
    Moving,      // 

    Stunned   // 新增眩晕状态
}
public enum StateEvent//用于控制转换
{
    Top_FreeAttack,//自由战斗
    Top_Advance,//推进战斗
    AttackTargetFound,   // 发现可攻击目标
    ChaseTargetFound,    // 发现可追击目标
    FindTarget,     // 追击目标丢失
    EscapeTriggered,     // 触发逃跑（如血量过低）
    StunBegin,           // 眩晕开始
    LifeFullyRestored,//生命完全恢复
    SupportFriendly,//支援友军
    TargetAdvancement,//路线推进
    OverRange,//超出范围，路线返回
    Idle,         // 空闲超时

    CloseToResources,//接近资源
    CollectResources,//采集资源
    ReturnToBase,//返回基地

    // 你可以根据需要继续添加
}