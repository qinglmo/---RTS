using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorkUnit : BaseUnit
{
    public Fruit currentResourceTarget;
    public int gatherNum;
    public int GatherNum { get { return gatherNum; }
        set {
            if (value > gatherMax)
            {
                gatherNum = gatherMax;
            }
            else
                gatherNum=value;
        }
    }
    public int gatherMax=100;
    public ResourceType resourceType;
    private void CancelMove()
    {
        movement.CancelMove();
    }

    public override void Update()
    {
        if (currentResourceTarget == null)//申请任务
        {
            currentResourceTarget = FactionManager.Instance.mainSettlement?.TaskApplication(this) as Fruit;
            if (mainState.StateMachine.currentState is IBranchState branchState)
            {
                branchState.StateMachine.HandleEvent(StateEvent.Idle);
            }
        }
        else
        {
            if(gatherNum==gatherMax||currentResourceTarget.LifeCycle!=LifeCycle.Harvest)//收获完成或资源类型不是采集资源
            {
                if (DistanceCalculate.Heuristic(FactionManager.Instance.mainSettlement.GridPos, GridPos) <= 1)//到达基地
                {
                    currentResourceTarget = FactionManager.Instance.mainSettlement.SubmissionResource(resourceType, gatherNum,currentResourceTarget) as Fruit;
                    gatherNum =0;
                }
                else if (mainState.StateMachine.currentState is IBranchState branchState)//返回基地
                {
                    branchState.StateMachine.HandleEvent(StateEvent.ReturnToBase);
                }
                return;//资源有可能清空，所以需要判断是否需要重新申请任务
            }

            if (DistanceCalculate.Heuristic(currentResourceTarget.GridPos, GridPos) <= 1)//到达资源
            {
                if (mainState.StateMachine.currentState is IBranchState branchState)
                {
                    branchState.StateMachine.HandleEvent(StateEvent.CollectResources);
                }
            }
            else//接近资源
            {
                if (mainState.StateMachine.currentState is IBranchState branchState)
                {

                    branchState.StateMachine.HandleEvent(StateEvent.CloseToResources);
                }
            }
        }
    }
    public override BehaviorTree.Node CreateTree(out BehaviorTree.Blackboard blackboard)
    {
        var focusTarget = GetComponent<IFocusTarget>() ?? gameObject.AddComponent<FocusedPerception>();
        var targetFiltering = GetComponent<ITargetFiltering>() ?? gameObject.AddComponent<ViewFiltering>();
        blackboard = new BehaviorTree.Blackboard(this, focusTarget, targetFiltering, 1);
        var selector = new BehaviorTree.Selector(blackboard);
        //selector.AddChild(new BehaviorTree.LiveJudgNode(blackboard, "隐藏检查"));
        return selector;
    }
}
