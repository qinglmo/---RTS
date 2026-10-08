using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRuleGroup", menuName = "State Machine/Rule Group")]
public class RuleGroup: ScriptableObject
{
    public List<TransitionRule> rules = new List<TransitionRule>();
}
