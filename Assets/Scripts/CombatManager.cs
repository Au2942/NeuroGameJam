/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] public List<CombatHandler> activeCombats = new List<CombatHandler>();
    private List<CombatHandler> endedCombats = new List<CombatHandler>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CombatHandler StartCombat(ICombatant unit)
    {
        CombatHandler combatHandler = new CombatHandler(unit);
        activeCombats.Add(combatHandler);
        return combatHandler;
    }

    public CombatHandler EngageInCombat(ICombatant engager, ICombatant target)
    {
        if (!target.IsCombatReady())
        {
            return null;
        }

        engager.AddCombatTarget(target);
        target.AddCombatTarget(engager);

        CombatHandler combatHandler1 = new CombatHandler(engager);
        activeCombats.Add(combatHandler1);

        return combatHandler1;
    }

    private void Update()
    {
        if (GameManager.Instance.isPause || activeCombats.Count == 0)
        {
            return;
        }
        UpdateCombatProgress();
    }

    private void UpdateCombatProgress()
    {
        foreach(CombatHandler combatHandler in activeCombats)
        {
            combatHandler.UpdateCombat();
        }
        foreach(CombatHandler combatHandler in endedCombats)
        {
            activeCombats.Remove(combatHandler);
        }
    }

    public void EndCombat(CombatHandler combatHandler)
    {
        combatHandler.EndCombat();
        endedCombats.Add(combatHandler);
    }
} */