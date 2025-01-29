using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatHandler
{
    public ICombatUnit Unit;
    private float nextAttackTimer;
    private bool IsInCombat = true;

    public CombatHandler(ICombatUnit unit)
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

        List<ICombatUnit> removeTargets = new List<ICombatUnit>();
        foreach (ICombatUnit target in Unit.CombatTargets)
        {
            if (target == null || !target.IsCombatReady())
            {
                removeTargets.Add(target);
            }
        }
        foreach (ICombatUnit target in removeTargets)
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