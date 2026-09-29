using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 波次管理器，按时间触发波次
/// </summary>
public class WaveManager : SingletonMono<WaveManager>
{
    [Header("波次列表")]
    public List<Wave> waves = new List<Wave>();   // 所有波次，可在面板中编辑

    public event Action<int> OnWaveTriggered;
    private int nextWaveNumber = 1;                // 下一个待触发的波次编号

    void Start()
    {
        TimeManager.Instance.OnOneDayPulse += TriggerNextWave;
    }
    void OnDestroy()
    {
        //TimeManager.Instance.OnOneDayPulse -= TriggerNextWave;
    }
    public void TriggerNextWave()
    {
        foreach(Wave wave in waves)
        {
            if(wave.waveNumber == nextWaveNumber)
            {
                wave.Spawn();
                OnWaveTriggered?.Invoke(nextWaveNumber);
                
            }
        }
        nextWaveNumber++;
    }

    public List<Wave> GetCurrentWaves()
    {   
        List<Wave> currentWaves = new List<Wave>();
        foreach(Wave wave in waves)
        {
            if(wave.waveNumber == nextWaveNumber)
            {
                currentWaves.Add(wave);
            }
        }
        return currentWaves;
    }

    public bool HasNextWave()
    {

        foreach(Wave wave in waves)
        {
            if(wave.waveNumber >=nextWaveNumber)
            {
                return true;
            }
        }
        return false;
    }
}