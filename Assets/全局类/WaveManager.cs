using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 波次管理器，按时间触发波次
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("波次列表")]
    public List<Wave> waves = new List<Wave>();   // 所有波次，可在面板中编辑

    private int nextWaveIndex = 0;                // 下一个待触发的波次索引

    private void Update()
    {
        // 如果所有波次都已触发，则不再检查
        if (nextWaveIndex >= waves.Count) return;

        // 获取当前游戏时间（使用之前的 TimeManager）
        float currentTime = TimeManager.Instance.GameTime;

        // 如果当前时间 ≥ 下一个波次的触发时间，则生成该波次
        if (currentTime >= waves[nextWaveIndex].time)
        {
            waves[nextWaveIndex].Spawn();
            nextWaveIndex++;
        }
    }
}