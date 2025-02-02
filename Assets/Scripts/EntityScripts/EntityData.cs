using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityData : MonoBehaviour
{
    [SerializeField] public RectTransform EntityBody;
    [SerializeField] public RectTransform EntityCell;
    [SerializeField] public List<UIEventHandler> ClickInteractDetectors = new();
    
    [Header("Dialogue")]
    [SerializeField] public DialogueManager DialogueManager;
    [SerializeField] public List<DialogueSet> DialogueSets = new();
    [SerializeField] public int dialogueSetIndex = 0;
    [SerializeField] public bool TalkInOrder = true;
    [SerializeField] public bool TalkRepeatable = true;
    [SerializeField] public float TalkRollInterval = 2f;
    [SerializeField] public float TalkChance = 0.25f;
    
    [Header("Gameplay")]
    [SerializeField] public float Health = 100;
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Corruption = 100; //to use when repairing / resetting
    [SerializeField] public float MaxCorruption = 100;
    [SerializeField] public float CorruptionCooldown = 10f; //cooldown after glitching out
    [SerializeField] public bool Interactable = true;
    [SerializeField] public bool Glitched = false;
    [SerializeField] public float GlitchRollThreshold = 0.7f; //start rolling at this integrity

    [Header("Animation")]
    [SerializeField] public List<AnimatorClipsPair> DefaultAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> IdleAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> DialoguePlayingAnimation = new(); //plays while dialogue is playing
    [SerializeField] public List<AnimatorClipsPair> DialogueTypingAnimation = new(); //plays when playing a typing sound
    [SerializeField] public List<AnimatorClipsPair> NormalAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> GlitchAnimatorClips = new();
    [SerializeField] public AnimState CurrentAnimationState = AnimState.Default;
    public enum AnimState
    {
        Default,
        Idle,
    }
}