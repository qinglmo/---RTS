using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BehaviorTree
{
    public class Blackboard
    {
        
        public BaseUnit unit;                     // 引用单位
        public IFocusTarget focusTarget;//注意力接口，用于行为树决策
        public ITargetFiltering targetFiltering;

        public IHasPosition CurrentTarget { get { return unit.detector.CurrentTarget; } }      // 当前目标
        public int attackRange;             // 攻击范围
        public Vector2Int? lastPos;//敌人的最后位置
        public Blackboard(BaseUnit unit, IFocusTarget focusTarget,ITargetFiltering targetFiltering, int attackRange)
        {
            this.unit = unit;
            this.focusTarget = focusTarget;
            this.targetFiltering = targetFiltering;
            this.attackRange = attackRange;
        }
        public void TrrigerEvent_AllTier(StateEvent stateEvent)
        {
            //将事件传递给主状态机和次状态机，让其自身处理转换。
            unit.mainState.StateMachine.HandleEvent(stateEvent);
            IBranchState combat = unit?.mainState?.StateMachine?.currentState as IBranchState;
            if (combat != null )
            {
                combat.StateMachine.HandleEvent(stateEvent);
            }
                
        }
        public void TrrigerEvent_MainState(StateEvent stateEvent)
        {
            //将事件传递给主状态机，让其自身处理转换。
            unit.mainState.StateMachine.HandleEvent(stateEvent);                
        }
        public void TriggerEvent_BranchState(StateEvent stateEvent)
        {
            //将事件传递给次状态机，让其自身处理转换。
            IBranchState combat = unit?.mainState?.StateMachine?.currentState as IBranchState;
            if (combat != null )
            {
                combat.StateMachine.HandleEvent(stateEvent);
            }
                
        }
        
    }
}
