using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单位信息面板（单例），支持按属性独立更新。
/// 通过事件驱动，外部可订阅各属性的更新方法。
/// </summary>
public class FloatingPanel : MonoBehaviour
{
    public static FloatingPanel Instance { get; private set; }

    [Header("UI Text References")]
    public Text gridText;           // 
    public Text enityText;         // 
    
    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {

    }
    public void UpdateInfo(Vector2Int grid,IFactionMember enity)
    {
        gridText.text = $"({grid.x:F1}, {grid.y:F1})";

        MonoBehaviour enity2 = enity as MonoBehaviour;
        if (enity2 != null)
            enityText.text = enity2.name;
        else
            enityText.text = "无";
    }
}