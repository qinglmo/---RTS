using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ProductionQueuePanel : SingletonMono<ProductionQueuePanel>
{

    [Header("队列按钮预制体（需有 Text 组件，并且带一个 Image 子物体作为遮罩）")]
    public GameObject queueButtonPrefab;//子图片灰色透明遮罩Mask，通过控制高度来实现进度条效果

    [Header("生产建筑")]
    private BuildingProduction production;

    // 用来存放实例化出来的按钮
    private List<GameObject> buttonInstances = new List<GameObject>();
    // 第一个按钮的遮罩引用
    private Image firstMaskImage;
    private RectTransform firstMaskRect;
    private float fullMaskHeight;
    void Awake()
    {
        if(Instance == null)//惰性启动
        {
        }
    }

    void Start()
    {
        gameObject.SetActive(false);
    }
    void Update()
    {
        UpdateFirstMask();
    }

    public void SetProduction(BuildingProduction prod)
    {
        production = prod;
    }
    public void Show()
    {
        gameObject.SetActive(true);
        production.OnOneUnitComplete += RefreshQueue;//订阅单个生产完成事件
        production.OnAddNewUnit += RefreshQueue;//订阅加入新单位事件
        RefreshQueue();
    }

    public void Hide()
    {
        production.OnOneUnitComplete -= RefreshQueue;//取消订阅单个生产完成事件
        production.OnAddNewUnit -= RefreshQueue;//取消订阅加入新单位事件
        gameObject.SetActive(false);
    }
    void RefreshQueue()
    {
        // 清除旧的按钮
        foreach (var btn in buttonInstances)
        {
            if (btn != null)
                Destroy(btn);
        }
        buttonInstances.Clear();

        if (production == null)
            return;

        List<UnitSpawnData> queue = production.GetProductionQueue();
        if (queue == null || queue.Count == 0)
            return;

        // 逐个实例化按钮
        for (int i = 0; i < queue.Count; i++)
        {
            UnitSpawnData data = queue[i];
            GameObject btn = Instantiate(queueButtonPrefab, transform);

            // 设置按钮上的文本（这里假设 UnitSpawnData 有 unitName 字段，可根据实际修改）
            Text txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = data.unitName; // 按实际属性名调整
            }

            buttonInstances.Add(btn);

            // 找到预设好的遮罩子物体（假设名称为 "Mask"）
            Transform maskTrans = btn.transform.Find("Mask");
            if (maskTrans != null)
            {
                Image maskImg = maskTrans.GetComponent<Image>();
                if (maskImg != null)
                {
                    if (i == 0) // 第一个按钮需要实时更新遮罩
                    {
                        firstMaskImage = maskImg;
                        firstMaskRect = maskImg.rectTransform;
                        fullMaskHeight = firstMaskRect.sizeDelta.y;
                    }
                    else // 其它按钮的遮罩保持完全覆盖（或根据需求隐藏）
                    {
                        RectTransform otherRect = maskImg.rectTransform;
                        float h = fullMaskHeight > 0 ? fullMaskHeight : otherRect.sizeDelta.y;
                        otherRect.sizeDelta = new Vector2(otherRect.sizeDelta.x, h);
                    }
                }
            }
        }

        // 刷新后立即更新一次第一个遮罩
        UpdateFirstMask();

    }

    void UpdateFirstMask()
    {
       if (production == null || firstMaskImage == null||production.GetProductionQueue().Count == 0)//因为最后一个单位生产完时也会刷新一次，所以需要判断队列是否为空
            return;

        // 0~1，表示剩余冷却比例
        float progress = production.GetProductionTimeRate();
        SetMaskHeight(firstMaskImage, progress);
    }

    void SetMaskHeight(Image mask, float ratio)
    {
        if (mask == null)
            return;

        // 如果全高尚未记录，则从当前遮罩获取
        if (fullMaskHeight <= 0)
            fullMaskHeight = mask.rectTransform.sizeDelta.y;

        // 按比例设置高度（假设锚点位于底部，高度缩减时视觉上遮罩从顶部向下消失）
        float newHeight = fullMaskHeight * ratio;
        mask.rectTransform.sizeDelta = new Vector2(mask.rectTransform.sizeDelta.x, newHeight);
    }
}