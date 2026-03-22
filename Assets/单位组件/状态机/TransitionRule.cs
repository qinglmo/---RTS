using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransitionRule
{
    [SerializeField] private UnitState fromState;
    [SerializeField] private bool fromStateHasValue=true;
    [SerializeField] private StateEvent triggerEvent;
    [SerializeField] private UnitState toState;
    [SerializeField] private int priority=0;
    public UnitState? FromState {
        get 
        { 
            if (fromStateHasValue)
                return fromState; 
            else
                return null;
        }
        set
        {
            if (value == null)
            {
                fromStateHasValue = false;
            }
            else
            {
                fromState = value.Value;
                fromStateHasValue = true; // 确保标记有值
            }
        }
    }          // 源状态（可为 null 表示任意状态）
    public StateEvent TriggerEvent { get { return triggerEvent; } set { triggerEvent = value; } }      // 触发事件
    public UnitState ToState { get { return toState; } set { toState = value; } }            // 目标状态
    public Func<bool> Condition { get; set; }         // 附加条件（可选）
    public int Priority { get { return priority; } set { priority = value; } }          // 优先级，数值越高越优先
}