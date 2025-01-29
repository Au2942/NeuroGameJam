using System.Collections.Generic;
public interface ICombatUnit
{
    public float Health { get; set;}
    public float MaxHealth { get; set;}
    public float AttackDamage { get; set;}
    public float AttackRate { get; set;}
    public List<ICombatUnit> CombatTargets { get; set;}
    public void AddCombatTarget(ICombatUnit target);
    public void RemoveCombatTarget(ICombatUnit target);
    public void Attack();
    public bool IsCombatReady();
    public void DealDamage(ICombatUnit target);
    public void TakeDamage(float value, ICombatUnit attacker);
}