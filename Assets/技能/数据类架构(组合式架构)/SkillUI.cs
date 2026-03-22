using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{

    private static SkillUI instance;
    public static SkillUI Instance => instance;

    // 内部自动收集的按钮和文本
    private List<Button> skillButtons = new List<Button>();
    private List<Text> skillTexts = new List<Text>();
    private UnitSkillsManager currentUnitSkills;

    private void Update()
    {
        // 检测数字键 1-6
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < skillButtons.Count && skillButtons[i].gameObject.activeSelf)
                {
                    skillButtons[i].onClick.Invoke();
                }
                break; // 只处理一个按键
            }
        }
        // 更新冷却显示
        if (currentUnitSkills != null)
        {
            for (int i = 0; i < skillButtons.Count; i++)
            {
                float progress = currentUnitSkills.GetCooldownProgress(i);
                // 更新UI
            }
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 自动获取所有子物体中的Button组件（包括未激活的）
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            skillButtons.Add(btn);
            // 尝试获取按钮下的Text组件（Text可能作为子物体或挂在同一物体上）
            Text txt = btn.GetComponentInChildren<Text>();
            if (txt == null)
                txt = btn.GetComponent<Text>();
            skillTexts.Add(txt);
        }

        // 初始隐藏所有按钮
        foreach (var btn in skillButtons)
            btn.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 动态添加单个技能（如果还有空槽位）
    /// </summary>
    public void AddSkill()
    {

    }

    /// <summary>
    /// 清除所有技能显示（隐藏按钮，移除监听）
    /// </summary>
    public void ClearSkills()
    {
        foreach (var btn in skillButtons)
        {
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }
    }
    //=============================新加=================================
    public void SetUnit(UnitSkillsManager manager)
    {
        currentUnitSkills = manager;
        RefreshUI();
    }
    private void RefreshUI()
    {
        ClearSkills();
        if (currentUnitSkills == null) return;
        var skills = currentUnitSkills.GetSkillDataList();
        for (int i = 0; i < skills.Count; i++)
        {
            // 设置图标、名称等
            SkillData skill = skills[i];
            Button btn = skillButtons[i];
            Text txt = skillTexts[i];

            if (txt != null)
                txt.text = skill.skillName;
            int index = i;
            Debug.Log("绑定技能" + skill.skillName+"序号"+index);
            btn.onClick.AddListener(() => OnSkillButtonClick(index));
            btn.gameObject.SetActive(true);
        }
    }
    public void OnSkillButtonClick(int index)
    {
        currentUnitSkills?.TryActivateSkill(index);
    }
}