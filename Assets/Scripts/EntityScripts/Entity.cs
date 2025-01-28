using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class Entity : MonoBehaviour
{

    [SerializeField] public GameObject Body;
    [SerializeField] protected UIEventHandler[] clickInteractHandlers;
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected List<DialogueSet> dialogueSets;
    [SerializeField] public float Health = 100;
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Stability = 100; //to use when repairing / resetting
    [SerializeField] public float MaxStability = 100; 
    [SerializeField] public bool Interactable = true;
    [SerializeField] protected bool talkInOrder = true;
    [SerializeField] protected bool talkRepeatable = true;
    [SerializeField] protected float talkRollInterval = 2f;
    [SerializeField] protected float talkChance = 0.25f;
    [SerializeField] protected float glitchRollThreshold = 0.7f; //start rolling at this integrity
    [SerializeField] protected float glitchCD = 30f;
    [SerializeField] public List<AnimatorClipsPair> defaultAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> idleAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> dialoguePlayingAnimation; //plays while dialogue is playing
    [SerializeField] public List<AnimatorClipsPair> dialogueTypingAnimation; //plays when playing a typing sound
    [SerializeField] public List<AnimatorClipsPair> normalAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> glitchAnimatorClips;

    public enum AnimationState
    {
        Default,
        Idle,
    }
    [SerializeField] public AnimationState CurrentAnimationState = AnimationState.Default;

    protected float talkRollTimer = 0f;
    protected float glitchCDTimer = 0f;
    public bool Glitched {get; set;} = false;
    protected int dialogueSetIndex = 0;

    protected int talkCounter = 0;




    protected virtual void Awake()
    {
        GameManager.Instance.OnStartStream += OnStartStream;
        GameManager.Instance.OnEndStream += OnEndStream;

        foreach(UIEventHandler clickInteract in clickInteractHandlers)
        {
            clickInteract.OnLeftClickEvent += (t) => ClickInteract(clickInteract.gameObject);
        }
    }
    protected virtual void Start()
    {
        PlayDefaultAnimation();
        SetNormalAppearance();
        PlayIdleAnimation();
    }
    protected virtual void OnStartStream()
    {

    }
    protected virtual void OnEndStream()
    {
        
    }
    protected virtual void ClickInteract(GameObject clickedObject)
    {

    }


    protected virtual void Update()
    {
        if(GameManager.Instance.isPause) return;

        if(Glitched)
        {
            GlitchBehavior();
        }
        else
        {
            NormalBehavior();
        }
    }

    public virtual void SetAnimationState(AnimationState state, bool force = false)
    {
        if (force || CurrentAnimationState != state)
        {
            CurrentAnimationState = state;
            PlayCurrentAnimation();
        }
    }
    public virtual void PlayCurrentAnimation()
    {
        switch(CurrentAnimationState)
        {
            case AnimationState.Default:
                PlayDefaultAnimation();
                break;
            case AnimationState.Idle:
                PlayIdleAnimation();
                break;
        }
    }

    public void AddHealth(float amount)
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
        OnHealthChanged();
    }

    public void AddStability(float amount)
    {
        Stability += amount;
        if (Stability > MaxStability)
        {
            Stability = MaxStability;
        }
        if (Stability < 0)
        {
            Stability = 0;
        }
        OnStabilityChanged();
    }

    protected virtual void OnHealthChanged()
    {
        RollChanceToGlitch();
    }

    protected virtual void OnStabilityChanged()
    {
        if(Glitched && Stability >= MaxStability)
        {
            ExitGlitchState();
        }
    }


    protected virtual void PlayDefaultAnimation()
    {
        for(int i = 0; i < defaultAnimatorClips.Count; i++)
        {
            if(defaultAnimatorClips[i].animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in defaultAnimatorClips[i].clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(defaultAnimatorClips[i].animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
        CurrentAnimationState = AnimationState.Default;
    }

    protected virtual void PlayIdleAnimation()
    {
        for(int i = 0; i < idleAnimatorClips.Count; i++)
        {
            if(idleAnimatorClips[i].animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in idleAnimatorClips[i].clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(idleAnimatorClips[i].animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
        CurrentAnimationState = AnimationState.Idle;
    }



    protected virtual void SetNormalAppearance()
    {
        foreach(AnimatorClipsPair animatorClipsPair in normalAnimatorClips)
        {
            if(animatorClipsPair.animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in animatorClipsPair.clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(animatorClipsPair.animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
    }

    protected virtual void SetGlitchAppearance()
    {
        foreach(AnimatorClipsPair animatorClipsPair in glitchAnimatorClips)
        {
            if(animatorClipsPair.animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in animatorClipsPair.clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(animatorClipsPair.animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
    }


    protected void PlayAnimation(Animator animator, AnimationClip clip, int layer = 0)
    {
        if (animator == null || clip == null) return;
        animator.CrossFade(clip.name, 0.2f, layer);
    }

    protected virtual void RollChanceToTalk()
    {
        if(!dialogueManager.IsDialoguePlaying)
        {
            if(talkRollTimer < Time.time)
            {
                if(Random.Range(0f, 1f) < talkChance)
                {
                    Speak();
                }
                talkRollTimer = Time.time + talkRollInterval;
            }
        }
    }

    protected virtual void NormalBehavior()
    {
        SharedBehavior();
    }

    protected virtual void GlitchBehavior()
    {
        SharedBehavior();
    }

    protected virtual void SharedBehavior()
    {

    }



    public virtual void EnterGlitchState()
    {
        Glitched = true;
        Stability = Health;
        MaxStability = MaxHealth;
        dialogueSetIndex = 1;
        ShutUp();
        SetGlitchAppearance();
        
    }


    public virtual void ExitGlitchState()
    {
        Glitched = false;
        dialogueSetIndex = 0;
        glitchCDTimer = Time.time + glitchCD;
        SetNormalAppearance();
    }

    public virtual void RollChanceToGlitch()
    {
        if(glitchCDTimer < Time.time) return;
        if (HealthPercentage() < glitchRollThreshold)
        {
            if (Random.Range(0f, 1f) >= HealthPercentage())
            {
                EnterGlitchState();
            }
        }
    }

    public float HealthPercentage()
    {
        return Health / MaxHealth;
    }

    public virtual void Converse()
    {
        Talk(dialogueSetIndex);
    }

    public virtual void Speak()
    {
        Talk(dialogueSetIndex, false);      
    }

    protected virtual void Talk(int dialogueSet = 0, bool playerInitiated = true)
    {
        if (dialogueManager == null || dialogueSets == null)
        {
            return;
        }

        List<DialogueInfoSO> dialogues = dialogueSets[dialogueSetIndex].dialogues;

       
        if (dialogues == null || dialogues.Count == 0)
        {
            return;
        }

        if(talkInOrder)
        {
            if(talkCounter >= dialogues.Count)
            {
                if(talkRepeatable)
                {
                    talkCounter = 0;
                }
                else
                {
                    return;
                }
            }
            dialogueManager.PlayDialogue(dialogues[talkCounter], playerInitiated);
            talkCounter++;
        }
        else
        {
            int randomIndex = Random.Range(0, dialogues.Count);
            dialogueManager.PlayDialogue(dialogues[randomIndex], playerInitiated);
        }
    }

    protected virtual void Talk(int dialogueSet, int dialogueIndex, bool playerInitiated = true)
    {
        if (dialogueManager == null || dialogueSets == null)
        {
            return;
        }

        DialogueInfoSO dialogue = dialogueSets[dialogueSetIndex].dialogues[dialogueIndex];

        if (dialogue == null)
        {
            return;
        }
        dialogueManager.PlayDialogue(dialogue, playerInitiated);
    }


    public void ShutUp()
    {
        dialogueManager.EndDialogue();
    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance.OnStartStream -= OnStartStream;
        GameManager.Instance.OnEndStream -= OnEndStream;
        foreach(UIEventHandler clickInteract in clickInteractHandlers)
        {
            clickInteract.OnLeftClickEvent -= (t) => ClickInteract(clickInteract.gameObject);
        }
    }
}
