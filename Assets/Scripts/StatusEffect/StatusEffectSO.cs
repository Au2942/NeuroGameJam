using UnityEngine;

public abstract class StatusEffectSO: ScriptableObject
{
    public abstract StatusEffect CreateStatusEffect();
}