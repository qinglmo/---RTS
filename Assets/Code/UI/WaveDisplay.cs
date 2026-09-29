using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveDisplay : MonoBehaviour
{
    public Text EnemyInfotext;

    void Update()
    {
        EnemyInfotext.text = GetEnemyInfo();
    }

    private string GetEnemyInfo()
    {
        var waves=WaveManager.Instance.GetCurrentWaves();
        if(waves.Count == 0)
        {
            return "当前无敌人";
        }
        string  enemyInfo = "";
        foreach(Wave wave in waves)
        {
            enemyInfo += wave.enemyPrefab.name + "*"+wave.count.ToString();
        }
        return enemyInfo;
    }
}
       
