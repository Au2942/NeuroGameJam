using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] public EntityData entityData;
    public RectTransform EntityBody;
    public RectTransform EntityCell;
    protected List<UIEventHandler> clickInteractDetectors = new();
    protected DialogueManager dialogueManager;
    protected List<DialogueSet> dialogueSets = new();
    public float Health = 100;
    public float MaxHealth = 100;
    public float Corruption = 100; //to use when repairing / resetting
    public float MaxCorruption = 100;
    public float CorruptionCooldown = 10f; //cooldown after glitching out
    public bool Interactable = true;
    protected bool talkInOrder = true;
    protected bool talkRepeatable = true;
    protected float talkRollInterval = 2f;
    protected float talkChance = 0.25f;
    protected float glitchRollThreshold = 0.7f; //start rolling at this integrity
    public List<AnimatorClipsPair> defaultAnimatorClips = new();
    public List<AnimatorClipsPair> idleAnimatorClips = new();
    public List<AnimatorClipsPair> dialoguePlayingAnimation = new(); //plays while dialogue is playing
    public List<AnimatorClipsPair> dialogueTypingAnimation = new(); //plays when playing a typing sound
    public List<AnimatorClipsPair> normalAnimatorClips = new();
    public List<AnimatorClipsPair> glitchAnimatorClips = new();

    [SerializeField] public AnimationState CurrentAnimationState = AnimationState.Default;
    public enum AnimationState
    {
        Default,
        Idle,
    }

    protected float talkRollTimer = 0f;
    public bool Glitched {get; set;} = false;
    public float CorruptionCooldownTimer = 0f;
    protected int dialogueSetIndex = 0;
    protected int talkCounter = 0;

    public event System.Action<float> OnHealthChangedEvent;
    public event System.Action<float> OnCorruptionChangedEvent;
    public event System.Action OnEnterGlitchEvent;
    public event System.Action OnExitGlitchEvent;
    private List<System.Action<PointerEventData>> OnClickInteractHandlers = new List<System.Action<PointerEventData>>();


    protected virtual void Awake()
    {
        
    }
    protected virtual void Start()
    {
        PlayDefaultAnimation();
        SetNormalAppearance();
        PlayIdleAnimation();
    }
    protected virtual void OnEnable()
    {
        OnClickInteractHandlers.Clear();
        for(int i = 0; i < clickInteractDetectors.Count; i++)
        {
            int index = i;
            OnClickInteractHandlers.Add((t) => ClickInteract(clickInteractDetectors[index].gameObject));
            clickInteractDetectors[index].OnLeftClickEvent += OnClickInteractHandlers[index];
        }
        GameManager.Instance.OnStartStream += OnStartStream;
        GameManager.Instance.OnEndStream += OnEndStream;
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
        OnHealthChanged(amount);
    }

    public void DamageHealth(float amount)
    {
        RestoreHealth(-amount);
    }

    public void RestoreCorruption(float amount)
    {
        Corruption += amount;
        if (Corruption > MaxCorruption)
        {
            Corruption = MaxCorruption;
        }
        if (Corruption < 0)
        {
            Corruption = 0;
        }
        OnCorruptionChanged(amount);
    }

    public void DamageCorruption(float amount)
    {
        RestoreCorruption(-amount);
    }

    protected virtual void OnHealthChanged(float amount)
    {
        if(!Glitched && amount < 0)
        {
            RollChanceToGlitch();
        }
        OnHealthChangedEvent?.Invoke(Health);
    }

    protected virtual void OnCorruptionChanged(float amount)
    {
        if(Glitched && Corruption <= 0)
        {
            ExitGlitchState();
        }
        OnCorruptionChangedEvent?.Invoke(Corruption);
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
        CorruptionCooldownTimer -= Time.deltaTime;
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
        MaxCorruption = MaxHealth - Health;
        Corruption = MaxCorruption;

        dialogueSetIndex = 1;
        ShutUp();

        SetGlitchAppearance();
        OnEnterGlitchEvent?.Invoke();
    }


    public virtual void ExitGlitchState()
    {
        Glitched = false;
        CorruptionCooldownTimer = CorruptionCooldown;
        dialogueSetIndex = 0;
        SetNormalAppearance();
        OnExitGlitchEvent?.Invoke();
    }

    public virtual void RollChanceToGlitch()
    {
        if (HealthPercentage() < glitchRollThreshold)
        {
            if (Random.Range(0f, 1f) < 1-HealthPercentage())
            {
                EnterGlitchState();
            }
        }
    }

    public float HealthPercentage()
    {
        return Health / MaxHealth;
    }

    public float CorruptionPercentage()
    {
        return Corruption / MaxCorruption;
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

    protected virtual void OnDisable()
    {
        for(int i = 0; i < clickInteractDetectors.Count; i++)
        {
            clickInteractDetectors[i].OnLeftClickEvent -= OnClickInteractHandlers[i];
        }
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnStartStream -= OnStartStream;
            GameManager.Instance.OnEndStream -= OnEndStream;
        }
    }
    protected virtual void OnDestroy()
    {


    }
}
