using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject GameSettlementPanel;
    void Start()
    {
        GameSettlementPanel.SetActive(false);
    }
    void Update()
    {
        if (!WaveManager.Instance.HasNextWave())
        {
            if(GameSettlementPanel.activeSelf == false)
            {
                if(FactionManager.Instance.GetUnits(FactionManager.Instance.EnemyFaction).Count == 0)
                {
                    GameSettlementPanel.SetActive(true);
                }
            }
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("主界面");
    }


}
