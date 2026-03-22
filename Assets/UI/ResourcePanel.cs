using UnityEngine;
using UnityEngine.UI;  // 使用Text需要这个命名空间

public class ResourcePanel : MonoBehaviour
{
    [Header("UI Text References")]
    public Text foodText;   // 食物
    public Text woodText;   // 木材
    public Text stoneText;  // 石头

    private void Awake()
    {
        // 订阅资源变化事件
        ResourceSystem.OnFoodChanged += UpdateFoodUI;
        ResourceSystem.OnWoodChanged += UpdateWoodUI;
        ResourceSystem.OnStoneChanged += UpdateStoneUI;

        // 初始化显示（确保一开始就是正确的数值）
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        ResourceSystem.OnFoodChanged -= UpdateFoodUI;
        ResourceSystem.OnWoodChanged -= UpdateWoodUI;
        ResourceSystem.OnStoneChanged -= UpdateStoneUI;
    }

    private void UpdateAllUI()
    {
        UpdateFoodUI(ResourceSystem.Food);
        UpdateWoodUI(ResourceSystem.Wood);
        UpdateStoneUI(ResourceSystem.Stone);
    }

    private void UpdateFoodUI(int newValue)
    {
        if (foodText != null)
            foodText.text = newValue.ToString();
    }

    private void UpdateWoodUI(int newValue)
    {
        if (woodText != null)
            woodText.text = newValue.ToString();
    }

    private void UpdateStoneUI(int newValue)
    {
        if (stoneText != null)
            stoneText.text = newValue.ToString();
    }
}