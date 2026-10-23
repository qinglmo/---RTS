using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    public static UnitInfoPanel Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;

    private IOccupyEnity currentTarget;
    private readonly List<Text> valueTexts = new List<Text>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ListPanel.Instance.OnUnitSelected += UpdateInfo;
    }

    private void Update()
    {
        if (currentTarget == null)
            return;

        List<string> values =
            InfoDataTranslator.Instance.GetInfoValues(currentTarget);

        for (int i = 0; i < valueTexts.Count; i++)
        {
            if (i < values.Count)
                valueTexts[i].text = values[i];
        }
    }

    public void UpdateInfo(IOccupyEnity target)
    {
        currentTarget = target;

        ClearItems();


        List<string> labels =
            InfoDataTranslator.Instance.GetInfoLabels(currentTarget);

        foreach (string label in labels)
        {
            GameObject item = Instantiate(itemPrefab, content);

            Text[] texts = item.GetComponentsInChildren<Text>();

            if (texts.Length < 2)
            {
                Debug.LogError("信息条目预制体需要两个 Text 组件。");
                Destroy(item);
                continue;
            }

            texts[0].text = label;
            texts[1].text = "";

            valueTexts.Add(texts[1]);
        }
    }

    private void ClearItems()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        valueTexts.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (ListPanel.Instance != null)
            ListPanel.Instance.OnUnitSelected -= UpdateInfo;
    }
}