using UnityEngine;


public class MemoryEntityData : EntityData
{
    [SerializeField] protected float AttackDamage = 1f;
    [SerializeField] protected float AttackRate = 1f;
    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] public bool IsBeingMaintained = false;
    [SerializeField] public bool InFocus = false;
    [SerializeField] public bool dealAOEDamage = false;
    [SerializeField] GlitchOverlay glitchEffect;

    protected List<ICombatUnit> combatTargets = new List<ICombatUnit>();

    protected float shutupTimer = 0f;
}