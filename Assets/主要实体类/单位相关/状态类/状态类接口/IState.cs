using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Enter();      // 进入状态时调用
    void Update();     // 每帧更新
    void Exit();       // 退出状态时调用

    void Reset();//重置状态，状态复用
}

public interface IBranchState :IState
{
    StateMachine StateMachine { get; }
} 