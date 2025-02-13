using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MemoryEntityData : MonoBehaviour
{
    public MemoryBlock MemoryBlock;
    public float shutupTimer = 0f;
    public bool InFocus = false;
    public bool IsBeingMaintained = false;
    
    [Header("Combat")]
    public float AttackDamage = 1f;
    public float AttackRate = 10f;
    public float TimeToShutup = 5f;
    public bool DealAOEDamage = false;
    public List<ICombatant> combatTargets = new List<ICombatant>();

}
