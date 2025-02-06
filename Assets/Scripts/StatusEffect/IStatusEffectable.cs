using System.Collections.Generic;
public interface IStatusEffectable
{
    List<StatusEffect> StatusEffects { get; set;}
    void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source);
    void ChangeStatusEffectStack(StatusEffect statusEffect, int stack);
    void RemoveStatusEffect(StatusEffect statusEffect);
}