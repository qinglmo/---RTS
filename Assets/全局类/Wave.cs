using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 单个波次的数据配置，可在 Inspector 中编辑
/// </summary>
[System.Serializable]
public class Wave
{
    [Header("波次设置")]
    public float time;                     // 触发时间（游戏内秒数）
    public GameObject enemyPrefab;         // 生成的敌人预制体
    public int count = 1;                  // 生成数量
    public Vector2 spawnRangeMin;          // 生成矩形范围左下角
    public Vector2 spawnRangeMax;          // 生成矩形范围右上角

    /// <summary>
    /// 在指定矩形范围内生成指定数量的敌人
    /// </summary>
    public void Spawn()
    {
        for (int i = 0; i < count; i++)
        {
            // 在范围内随机位置
            float randomX = Random.Range(spawnRangeMin.x, spawnRangeMax.x);
            float randomY = Random.Range(spawnRangeMin.y, spawnRangeMax.y);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

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