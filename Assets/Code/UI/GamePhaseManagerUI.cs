using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePhaseManagerUI : MonoBehaviour
{
    public GameObject TimePanelObj;
    public GameObject battleButtonObj;
    public Button battleButton;

    void Start()
    {
        battleButton.onClick.AddListener(GamePhaseManager.Instance.OnBattleButtonClick);
        GamePhaseManager.Instance.OnPhaseChanged+=PauseChange;
        battleButtonObj.SetActive(false);
    }
    void OnDestroy()
    {
        battleButton.onClick.RemoveListener(GamePhaseManager.Instance.OnBattleButtonClick);
        GamePhaseManager.Instance.OnPhaseChanged-=PauseChange;
    }
    private void PauseChange(GamePhaseManager.GamePhase phase)
    {
        if (phase == GamePhaseManager.GamePhase.Battle)
        {
            TimePanelObj.SetActive(true);
            battleButtonObj.SetActive(false);
        }
        if(phase == GamePhaseManager.GamePhase.Deployment)
        {
            TimePanelObj.SetActive(false);
            battleButtonObj.SetActive(true);
        }
    }
}
