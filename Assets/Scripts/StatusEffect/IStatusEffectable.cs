using System.Collections.Generic;
public interface IStatusEffectable
{
    List<StatusEffect> StatusEffects { get; set;}
    void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source);
    void RemoveStatusEffect(StatusEffect statusEffect);
}