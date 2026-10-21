using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 单个波次的数据配置，可在 Inspector 中编辑
/// </summary>
[System.Serializable]
public class Wave
{
    [Header("波次设置")]
    public int waveNumber;                     // 触发波次
    public GameObject enemyPrefab;         // 生成的敌人预制体
    public int count = 1;                  // 生成数量
    public Vector2 spawnRangeMin;          // 生成矩形范围左下角
    public Vector2 spawnRangeMax;          // 生成矩形范围右上角

    /// <summary>
    /// 在指定矩形范围内生成指定数量的敌人
    /// </summary>
    public void Spawn()
    {
        int RetryCount = 0;
        for (int i = 0; i < count; i++)
        {
            // 在范围内随机位置
            float randomX = Random.Range(spawnRangeMin.x, spawnRangeMax.x);
            float randomY = Random.Range(spawnRangeMin.y, spawnRangeMax.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            //判断位置的合法性
            if(GridManager.Instance.IsOccupied(GridManager.Instance.WorldToCell(spawnPos)))
            {
                Debug.LogWarning($"波次{waveNumber}生成位置被占用，尝试重新生成");
                i--; // 重新尝试生成
                RetryCount++;
                if(RetryCount>10)
                {
                    Debug.LogError($"波次{waveNumber}生成位置连续失败超过10次，停止生成");
                    break;
                }
                continue;
            }
            // 实例化敌人
            if (enemyPrefab != null)
            {
                GameObject.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogError("波次中缺少敌人预制体！");
            }
        }
    }
}