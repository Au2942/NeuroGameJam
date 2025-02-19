using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public abstract class Entity : MonoBehaviour, IStatusEffectable, IStatusEffectSource, ISpeaker
{
    #region Fields
    [SerializeField] protected EntityData entityData;
    public RectTransform EntityBody { get => entityData.EntityBody; set => entityData.EntityBody = value; }
    public RectTransform EntityCell { get => entityData.EntityCell; set => entityData.EntityCell = value; }
    protected List<UIEventHandler> ClickInteractDetectors { get => entityData.ClickInteractDetectors; set => entityData.ClickInteractDetectors = value; }
    protected DialogueManager DialogueManager { get => entityData.DialogueManager; set => entityData.DialogueManager = value; }
    protected List<DialogueSet> DialogueSets { get => entityData.DialogueSets; set => entityData.DialogueSets = value; }
    public int DialogueSetIndex { get => entityData.dialogueSetIndex; set => entityData.dialogueSetIndex = value; }
    protected bool TalkInOrder { get => entityData.TalkInOrder; set => entityData.TalkInOrder = value; }
    protected bool TalkRepeatable { get => entityData.TalkRepeatable; set => entityData.TalkRepeatable = value; }
    protected float TalkRollInterval { get => entityData.TalkRollInterval; set => entityData.TalkRollInterval = value; }
    protected float TalkChance { get => entityData.TalkChance; set => entityData.TalkChance = value; }
    protected float talkRollTimer { get => entityData.talkRollTimer; set => entityData.talkRollTimer = value; }
    protected int talkCounter { get => entityData.talkCounter; set => entityData.talkCounter = value; }
    public float Health { get => entityData.Health; set => entityData.Health = value; }
    public float MaxHealth { get => entityData.MaxHealth; set => entityData.MaxHealth = value; }
    public float ErrorIndex { get => entityData.ErrorIndex; set => entityData.ErrorIndex = value; }
    public float CorruptionCooldown { get => entityData.CorruptionCooldown; set => entityData.CorruptionCooldown = value; }
    public float CorruptionCooldownTimer { get => entityData.CorruptionCooldownTimer; set => entityData.CorruptionCooldownTimer = value; }
    public bool Interactable = true;
    public bool Glitched = false;
    protected float GlitchRollThreshold { get => entityData.GlitchRollThreshold; set => entityData.GlitchRollThreshold = value; }
    public List<AnimatorClipsPair> DefaultAnimatorClips { get => entityData.DefaultAnimatorClips; set => entityData.DefaultAnimatorClips = value; }
    public List<AnimatorClipsPair> IdleAnimatorClips { get => entityData.IdleAnimatorClips; set => entityData.IdleAnimatorClips = value; }
    public List<AnimatorClipsPair> DialoguePlayingAnimation { get => entityData.DialoguePlayingAnimation; set => entityData.DialoguePlayingAnimation = value; }
    public List<AnimatorClipsPair> DialogueTypingAnimation { get => entityData.DialogueTypingAnimation; set => entityData.DialogueTypingAnimation = value; }
    public List<AnimatorClipsPair> NormalAnimatorClips { get => entityData.NormalAnimatorClips; set => entityData.NormalAnimatorClips = value; }
    public List<AnimatorClipsPair> GlitchAnimatorClips { get => entityData.GlitchAnimatorClips; set => entityData.GlitchAnimatorClips = value; }
    public AnimState CurrentAnimationState { get => entityData.CurrentAnimationState; set => entityData.CurrentAnimationState = value; }
    public List<StatusEffect> StatusEffects { get => entityData.StatusEffects; set => entityData.StatusEffects = value; }
    public enum AnimState
    {
        Default,
        Idle,
    }
    public event System.Action<float> OnHealthChangedEvent;
    public event System.Action<float> OnErrorIndexChangedEvent;
    public event System.Action OnEnterGlitchEvent;
    public event System.Action OnExitGlitchEvent;
    protected List<System.Action<PointerEventData>> OnClickInteractDelegates = new();
    #endregion

    protected virtual void Awake()
    {
        if(entityData == null)
        {
            entityData = GetComponent<EntityData>();
            if(entityData == null)
            {
                entityData = gameObject.AddComponent<EntityData>();
            }
        }
    }

    protected virtual void Start()
    {
        PlayDefaultAnimation();
        SetNormalAppearance();
        PlayIdleAnimation();
    }
    protected virtual void OnEnable()
    {
        for(int i = 0; i < ClickInteractDetectors.Count; i++)
        {
            int index = i;
            OnClickInteractDelegates.Add((t) => ClickInteract(ClickInteractDetectors[index].gameObject));
            ClickInteractDetectors[index].OnLeftClickEvent += OnClickInteractDelegates[index];
        }
        GameManager.Instance.OnStartStream += OnStartStream;
        GameManager.Instance.OnEndStream += OnEndStream;
    }

    protected virtual void OnStartStream() {}
    protected virtual void OnEndStream() {}
    protected virtual void ClickInteract(GameObject clickedObject) {}

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

#region Health & ErrorIndex
    public void RestoreHealth(float amount)
    {
        entityData.RestoreHealth(amount);
        OnHealthChanged(amount);
    }

    public void DamageHealth(float amount)
    {
        RestoreHealth(-amount);
    }

    public void IncreaseErrorIndex(float amount)
    {
        entityData.IncreaseErrorIndex(amount);
        OnErrorIndexChanged(amount);
    }

    public void ReduceErrorIndex(float amount)
    {
        IncreaseErrorIndex(-amount);
    }

    protected virtual void OnHealthChanged(float amount)
    {
        // if(!Glitched && amount < 0)
        // {
        //     RollChanceToGlitch();
        // }
        OnHealthChangedEvent?.Invoke(Health);
    }

    protected virtual void OnErrorIndexChanged(float amount)
    {
        if(Glitched && ErrorIndex <= 0)
        {
            ExitGlitchState();
        }
        OnErrorIndexChangedEvent?.Invoke(ErrorIndex);
    }

    public float HealthPercentage()
    {
        return entityData.HealthPercentage();
    }

    public float ErrorIndexPercentage()
    {
        return entityData.ErrorIndexPercentage();
    }
#endregion

#region Animation
    public virtual void SetAnimationState(AnimState state, bool force = false)
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
            case AnimState.Default:
                PlayDefaultAnimation();
                break;
            case AnimState.Idle:
                PlayIdleAnimation();
                break;
        }
    }
    protected virtual void PlayDefaultAnimation()
    {
        for(int i = 0; i < DefaultAnimatorClips.Count; i++)
        {
            if(DefaultAnimatorClips[i].animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in DefaultAnimatorClips[i].clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(DefaultAnimatorClips[i].animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
        CurrentAnimationState = AnimState.Default;
    }

    protected virtual void PlayIdleAnimation()
    {
        for(int i = 0; i < IdleAnimatorClips.Count; i++)
        {
            if(IdleAnimatorClips[i].animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in IdleAnimatorClips[i].clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(IdleAnimatorClips[i].animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
        CurrentAnimationState = AnimState.Idle;
    }

    protected virtual void SetNormalAppearance()
    {
        foreach(AnimatorClipsPair animatorClipsPair in NormalAnimatorClips)
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
        foreach(AnimatorClipsPair animatorClipsPair in GlitchAnimatorClips)
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
#endregion

    protected virtual void NormalBehavior()
    {
        SharedBehavior();
        CorruptionCooldownTimer -= Time.deltaTime;
    }

    protected virtual void GlitchBehavior()
    {
        SharedBehavior();
    }

    protected virtual void SharedBehavior() {}

    public virtual void EnterGlitchState()
    {
        Glitched = true;
        SetGlitchAppearance();
        OnEnterGlitchEvent?.Invoke();
    }


    public virtual void ExitGlitchState()
    {
        Glitched = false;
        CorruptionCooldownTimer = CorruptionCooldown;
        SetNormalAppearance();
        OnExitGlitchEvent?.Invoke();
    }

    public virtual bool RollChanceToGlitch()
    {
        if (HealthPercentage() < GlitchRollThreshold)
        {
            if (Random.Range(0f, 1f) < 1-HealthPercentage())
            {
                EnterGlitchState();
                return true;
            }
        }
        return false;
    }

    public virtual void Converse()
    {
        Talk(DialogueSetIndex);
    }

    public virtual void Speak()
    {
        Talk(DialogueSetIndex, false);      
    }

    /// <summary>
    /// Talk using the dialogue set at the specified index
    /// </summary>
    protected virtual void Talk(int dialogueSet = 0, bool playerInitiated = true)
    {
        if (DialogueManager == null || DialogueSetIndex >= DialogueSets.Count)
        {
            return;
        }

        List<DialogueInfoSO> dialogues = DialogueSets[DialogueSetIndex].dialogues;

       
        if (dialogues == null || dialogues.Count == 0)
        {
            return;
        }

        if(TalkInOrder)
        {
            if(talkCounter >= dialogues.Count)
            {
                if(TalkRepeatable)
                {
                    talkCounter = 0;
                }
                else
                {
                    return;
                }
            }
            DialogueManager.PlayDialogue(dialogues[talkCounter], playerInitiated);
            talkCounter++;
        }
        else
        {
            int randomIndex = Random.Range(0, dialogues.Count);
            DialogueManager.PlayDialogue(dialogues[randomIndex], playerInitiated);
        }
    }

    /// <summary>
    /// Say a specific dialogue from a specific dialogue set
    /// </summary>
    protected virtual void Say(int dialogueSet, int dialogueIndex, bool playerInitiated = true)
    {
        if (DialogueManager == null || DialogueSets == null)
        {
            return;
        }

        DialogueInfoSO dialogue = DialogueSets[DialogueSetIndex].dialogues[dialogueIndex];

        if (dialogue == null)
        {
            return;
        }
        DialogueManager.PlayDialogue(dialogue, playerInitiated);
    }

    public void ShutUp()
    {
        DialogueManager.EndDialogue();
    }

        protected virtual void RollChanceToTalk()
    {
        if(!DialogueManager.IsDialoguePlaying)
        {
            if(talkRollTimer < Time.time)
            {
                if(Random.Range(0f, 1f) < TalkChance)
                {
                    Speak();
                }
                talkRollTimer = Time.time + TalkRollInterval;
            }
        }
    }

    protected virtual void OnDisable()
    {
        for(int i = 0; i < ClickInteractDetectors.Count; i++)
        {
            ClickInteractDetectors[i].OnLeftClickEvent -= OnClickInteractDelegates[i];
        }
        OnClickInteractDelegates.Clear();
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnStartStream -= OnStartStream;
            GameManager.Instance.OnEndStream -= OnEndStream;
        }

    }
    protected virtual void OnDestroy() {}

    public void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source = null) {}

    public void RemoveStatusEffect(StatusEffect statusEffect) {}

    public void ChangeStatusEffectStack(StatusEffect statusEffect, int stack) {}

}
