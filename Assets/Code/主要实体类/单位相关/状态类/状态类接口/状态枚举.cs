public enum UnitState
{
    MoveToPos,//移动到指定位置
    ActionToTarget,//执行对目标的操作
    Stunned , // 控制性眩晕，可以思考但不能执行操作
    Idle,         // 空闲
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