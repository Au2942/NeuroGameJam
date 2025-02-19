using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MemoryEntityData : MonoBehaviour
{
    public MemoryBlock MemoryBlock;
    public float TimeToShutup = 5f;
    public float shutupTimer = 0f;
    public bool InFocus = false;
    public bool IsBeingMaintained = false;
    public bool IsBeingRead = false;
    public float ReadCorruptAmount = 1f;
    public float ReadCorruptInterval = 5f;
    
    [Header("Combat")]
    public float AttackDamage = 1f;
    public float AttackRate = 10f;
    public bool DealAOEDamage = false;
    public List<ICombatant> combatTargets = new List<ICombatant>();

}
