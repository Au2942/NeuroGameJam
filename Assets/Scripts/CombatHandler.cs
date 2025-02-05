using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatHandler
{
    public ICombatant Unit;
    private float nextAttackTimer;
    private bool IsInCombat = true;

    public CombatHandler(ICombatant unit)
    {
        Unit = unit;
        nextAttackTimer = 0;
    }

    public void UpdateCombat()
    {
        if (!CheckIsInCombat())
        {
            CombatManager.Instance.EndCombat(this);
            return;
        }

        List<ICombatant> removeTargets = new List<ICombatant>();
        foreach (ICombatant target in Unit.CombatTargets)
        {
            if (target == null || !target.IsCombatReady())
            {
                removeTargets.Add(target);
            }
        }
        foreach (ICombatant target in removeTargets)
        {
            Unit.RemoveCombatTarget(target);
        }

        nextAttackTimer += Time.deltaTime;

        if (nextAttackTimer >= Unit.AttackRate)
        {
            Unit.Attack();
            nextAttackTimer = 0;
        }
    }

    public void EndCombat()
    {
        IsInCombat = false;
        // Any cleanup logic if needed
    }

    public bool CheckIsInCombat()
    {
        IsInCombat = Unit != null && Unit.IsCombatReady();
        return IsInCombat;
    }
}