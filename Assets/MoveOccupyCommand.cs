
using UnityEngine;

public class MoveOccupyCommand : Command
{
    /// <summary> 开始执行命令，设置单位的目标字段 </summary>
    public override void Execute(BaseUnit unit)
    {

    }

    /// <summary> 每帧更新，检查完成条件或处理阶段性逻辑 </summary>
    public override void Update(BaseUnit unit)
    {
        //if (unit.StateMachine.CurrentStateEnum == UnitState.Nothing)
        //{
        //    BaseBuilding building = GridManager.Instance.GetBuilding((Vector2Int)unit.commandMovePosition);
        //    if(building != null && building is BuildingTower)
        //    {
        //        BuildingTower buildingTower = (BuildingTower)building;
        //        buildingTower.TryEnter(unit);
        //    }
        //    IsFinished = true;
        //}
    }

    /// <summary> 取消命令，清理单位上的目标字段 </summary>
    public override void Cancel(BaseUnit unit)
    {

    }

}