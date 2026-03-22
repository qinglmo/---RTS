using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public abstract void Execute(BaseUnit caster, Vector2Int targetCell);
}