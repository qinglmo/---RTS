using System.Collections.Generic;
using UnityEngine;

public class ChaseAttackCommand : Command
{
    BaseUnit unit;
    /// <summary> 开始执行命令，设置单位的目标字段 </summary>
    public override void Execute(BaseUnit unit) {
        unit.detector.SetCommandTarget(unit.commandAttackTarget);
        this.unit = unit;
    }

    /// <summary> 每帧更新，检查完成条件或处理阶段性逻辑 </summary>
    public override void Update(BaseUnit unit) {
        if (unit.commandAttackTarget == null) {
            IsFinished = true;
        }
    }

    /// <summary> 取消命令，清理单位上的目标字段 </summary>
    public override void Cancel(BaseUnit unit) { 
        unit.detector.isCommand=false;
    }
}
