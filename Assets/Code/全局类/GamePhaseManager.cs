using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GamePhaseManager : SingletonMono<GamePhaseManager>
{
    public enum GamePhase
    {
        Operation,       // 经营阶段（强调基地/资源运作）
        Deployment,      // 站前准备阶段（强调部队部署）
        Battle           // 战斗阶段
    }
    private GamePhase currentPhase=GamePhase.Operation;
    public event Action<GamePhase> OnPhaseChanged;
    void Awake()
    {
        WaveManager.Instance.OnWaveTriggered+=SetDeploymentPhase;
    }
    void Update()
    {
        if (currentPhase == GamePhase.Battle)
        {
            if(FactionManager.Instance.FactionUnits[FactionManager.Instance.EnemyFaction].Count==0)
            {
                Debug.Log("敌人已灭");
                SetOperationPhase();
            }
        }
    }
    void OnDestroy()
    {
        //WaveManager.Instance.OnWaveTriggered-=SetDeploymentPhase;
    }
    public void SetOperationPhase()
    {
        currentPhase=GamePhase.Operation;

        TimeManager.Instance.ResumeWorldTime();//恢复世界时间流速

        DragUnitHandler.Instance.SetEnableDrag(false);
        OnPhaseChanged?.Invoke(currentPhase);
    }
    public void SetDeploymentPhase(int waveIndex)
    {
        currentPhase=GamePhase.Deployment;
        DragUnitHandler.Instance.SetEnableDrag(true);
        
        TimeManager.Instance.PauseGame();//暂停游戏
        OnPhaseChanged?.Invoke(currentPhase);
    }
    public void SetBattlePhase()
    {
        currentPhase=GamePhase.Battle;
        DragUnitHandler.Instance.SetEnableDrag(false);

        TimeManager.Instance.SlowWorldTime();//慢放世界时间流速
        TimeManager.Instance.SetTimeScale(1f);//设置时间缩放为1，恢复正常时间流

        OnPhaseChanged?.Invoke(currentPhase);
    }

    public void OnBattleButtonClick()
    {
        SetBattlePhase();
    }
}
