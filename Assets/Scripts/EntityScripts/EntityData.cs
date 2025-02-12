using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityData : MonoBehaviour
{
    [Header("Core")]
    public RectTransform EntityBody;
    public RectTransform EntityCell;
    public List<UIEventHandler> ClickInteractDetectors = new();
    
    [Header("Dialogue")]
    public DialogueManager DialogueManager;
    public List<DialogueSet> DialogueSets = new();
    public int dialogueSetIndex = 0;
    public bool TalkInOrder = true;
    public bool TalkRepeatable = true;
    public float TalkRollInterval = 2f;
    public float TalkChance = 0.25f;
    public float talkRollTimer = 0f;
    public int talkCounter = 0;
    
    [Header("Gameplay")]
    public float Health = 100;
    public float MaxHealth = 100;
    public float ErrorIndex = 100; //to use when repairing / resetting
    public float MaxErrorIndex = 100;
    public float CorruptionCooldown = 10f; //cooldown after glitching out
    public float CorruptionCooldownTimer = 0f;
    public float GlitchRollThreshold = 0.7f; //start rolling at this integrity
    public List<StatusEffect> StatusEffects = new List<StatusEffect>(); 


    [Header("Animation")]
    public List<AnimatorClipsPair> DefaultAnimatorClips = new();
    public List<AnimatorClipsPair> IdleAnimatorClips = new();
    public List<AnimatorClipsPair> DialoguePlayingAnimation = new(); //plays while dialogue is playing
    public List<AnimatorClipsPair> DialogueTypingAnimation = new(); //plays when playing a typing sound
    public List<AnimatorClipsPair> NormalAnimatorClips = new();
    public List<AnimatorClipsPair> GlitchAnimatorClips = new();
    public Entity.AnimState CurrentAnimationState = Entity.AnimState.Default;


    public float HealthPercentage()
    {
        return Health / MaxHealth;
    }

    public float CorruptionPercentage()
    {
        return ErrorIndex / MaxErrorIndex;
    }

    public void RestoreHealth(float amount)
    {
        Health += amount;
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
        if (Health < 0)
        {
            Health = 0;
        }
    }

    public void DamageHealth(float amount)
    {
        RestoreHealth(-amount);
    }

    public void RestoreCorruption(float amount)
    {
        ErrorIndex += amount;
        if (ErrorIndex > MaxErrorIndex)
        {
            ErrorIndex = MaxErrorIndex;
        }
        if (ErrorIndex < 0)
        {
            ErrorIndex = 0;
        }
    }

    public void DamageCorruption(float amount)
    {
        RestoreCorruption(-amount);
    }


}