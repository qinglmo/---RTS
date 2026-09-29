using UnityEngine;
using UnityEngine.UI;

public class SceneSwitchButton : MonoBehaviour
{
    public string targetSceneName;  // 在 Inspector 里填要切换的场景名

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null && SceneLoader.Instance != null)
        {
            // 动态绑定：点击时调用管理器的 LoadScene 方法，并传递场景名
            btn.onClick.AddListener(() => SceneLoader.Instance.LoadSceneByName(targetSceneName));
        }
        else
        {
            Debug.LogError("按钮组件或 SceneLoader 实例不存在");
        }
    }
}