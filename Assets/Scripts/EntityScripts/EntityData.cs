using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityData : MonoBehaviour
{
    [SerializeField] public RectTransform EntityBody;
    [SerializeField] public RectTransform EntityCell;
    [SerializeField] public List<UIEventHandler> clickInteractDetectors = new();
    [SerializeField] public DialogueManager dialogueManager;
    [SerializeField] protected List<DialogueSet> dialogueSets = new();
    [SerializeField] public float Health = 100;
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Corruption = 100; //to use when repairing / resetting
    [SerializeField] public float MaxCorruption = 100;
    [SerializeField] public float CorruptionCooldown = 10f; //cooldown after glitching out
    [SerializeField] public bool Interactable = true;
    [SerializeField] public bool talkInOrder = true;
    [SerializeField] public bool talkRepeatable = true;
    [SerializeField] public float talkRollInterval = 2f;
    [SerializeField] public float talkChance = 0.25f;
    [SerializeField] public float glitchRollThreshold = 0.7f; //start rolling at this integrity
    [SerializeField] public List<AnimatorClipsPair> defaultAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> idleAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> dialoguePlayingAnimation = new(); //plays while dialogue is playing
    [SerializeField] public List<AnimatorClipsPair> dialogueTypingAnimation = new(); //plays when playing a typing sound
    [SerializeField] public List<AnimatorClipsPair> normalAnimatorClips = new();
    [SerializeField] public List<AnimatorClipsPair> glitchAnimatorClips = new();

    [SerializeField] public AnimationState CurrentAnimationState = AnimationState.Default;
    public enum AnimationState
    {
        Default,
        Idle,
    }
}