using System.Collections.Generic;
public interface ICombatant
{
    float Health { get; set;}
    float MaxHealth { get; set;}
    float AttackDamage { get; set;}
    float AttackRate { get; set;}
    List<ICombatant> CombatTargets { get; set;}
    void AddCombatTarget(ICombatant target);
    void RemoveCombatTarget(ICombatant target);
    void Attack();
    bool IsCombatReady();
    void DealDamage(ICombatant target, DamageType damageType);
    bool TakeDamage(float value, DamageType damageType, ICombatant attacker);
}

public enum DamageType
{
    Normal,
    True
}