using UnityEngine;


[System.Serializable]
public class MemoryEntityData : MonoBehaviour
{
    [SerializeField] public MemoryBlock MemoryBlock;
    [SerializeField] public GlitchOverlay GlitchEffect;
    
    [Header("Combat")]
    [SerializeField] public float AttackDamage = 1f;
    [SerializeField] public float AttackRate = 1f;
    [SerializeField] public float TimeToShutup = 5f;
    [SerializeField] public bool DealAOEDamage = false;


}