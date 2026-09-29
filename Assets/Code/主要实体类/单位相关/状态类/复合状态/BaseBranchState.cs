using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 2. 抽象基类：实现接口，复用代码（偷懒用）
public abstract class BaseBranchState : IBranchState
{
    protected BaseUnit unit;
    public StateMachine stateMachine;
    public StateMachine StateMachine { get { return stateMachine; } protected set { } }

    protected BaseBranchState(BaseUnit unit)
    {
        this.unit = unit;
    }   

    public abstract void Enter();

    public virtual void Exit()
    {

    }

    public virtual void Reset()
    {

    }

    public virtual void Update() => StateMachine?.Update();
    // ... 其他通用代码
}