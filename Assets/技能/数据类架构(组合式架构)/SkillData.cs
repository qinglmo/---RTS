using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public float cooldown;      // 冷却时间
    public int manaCost;         // 法力消耗
    [TextArea] public string description;
    // 其他通用字段
    [Header("范围设置")]
    public SkillRangeType rangeType; // Circle, Line
    public float rangeValue;         // 半径或距离

    [Header("视觉")]
    public GameObject highlightPrefab; // 可选，覆盖默认

    [Header("效果")]
    public SkillEffect effect;        // 注意：ScriptableObject不能直接序列化接口，需要改用具体类或使用UnityEngine.Object引用
}