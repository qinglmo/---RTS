using System.Collections.Generic;
using UnityEngine;
public abstract class Command
{
    public bool IsFinished { get; protected set; }

    /// <summary> 开始执行命令，设置单位的目标字段 </summary>
    public virtual void Execute(BaseUnit unit) { }

    /// <summary> 每帧更新，检查完成条件或处理阶段性逻辑 </summary>
    public virtual void Update(BaseUnit unit) { }

    /// <summary> 取消命令，清理单位上的目标字段 </summary>
    public virtual void Cancel(BaseUnit unit) { }
}
public class UnitCommander : MonoBehaviour
{
    private Queue<Command> commandQueue = new Queue<Command>();
    private Command currentCommand;
    private BaseUnit unit;

    private void Awake()
    {
        unit = GetComponent<BaseUnit>();
        if (unit == null)
        {
            Debug.LogError("UnitCommander requires a BaseUnit component on the same GameObject.");
        }
    }

    private void Update()
    {
        // 如果当前没有命令，尝试从队列中取下一个
        if (currentCommand == null || currentCommand.IsFinished)
        {
            if (commandQueue.Count > 0)
            {
                StartNextCommand();
            }
            else
            {
                // 无命令时，确保清除所有命令目标字段（状态机会回到自动行为）
                unit.commandAttackTarget = null;
                unit.commandMovePosition = null;
            }
        }

        // 更新当前命令
        currentCommand?.Update(unit);
    }

    private void StartNextCommand()
    {
        // 如果有正在执行的命令，先取消
        currentCommand?.Cancel(unit);

        // 取出下一个命令
        currentCommand = commandQueue.Dequeue();
        currentCommand?.Execute(unit);
    }

    /// <summary> 添加新命令到队列末尾 </summary>
    public void AddCommand(Command cmd)
    {
        commandQueue.Enqueue(cmd);
        // 如果当前没有执行中的命令，立即开始
        if (currentCommand == null || currentCommand.IsFinished)
        {
            StartNextCommand();
        }
    }

    /// <summary> 清除所有未执行的命令，并取消当前命令 </summary>
    public void ClearAllCommands()
    {
        commandQueue.Clear();
        CancelCurrentCommand();
    }

    /// <summary> 取消当前命令（不移除队列中的其他命令） </summary>
    public void CancelCurrentCommand()
    {
        currentCommand?.Cancel(unit);
        currentCommand = null;
        // 注意：不清除队列，下一个命令会在下一帧开始
    }

    /// <summary> 跳过当前命令，执行下一个（用于某些打断逻辑） </summary>
    public void SkipCurrentCommand()
    {
        CancelCurrentCommand();
        // 下一帧 Update 会从队列取下一个
    }
}