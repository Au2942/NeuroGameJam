using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class Entity : MonoBehaviour
{

    [SerializeField] protected GameObject Body;
    [SerializeField] protected UIEventHandler[] clickInteractHandlers;
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected List<DialogueSet> dialogueSets;
    [SerializeField] public float Integrity = 100;
    [SerializeField] public float MaxIntegrity = 100;
    [SerializeField] public float decayInterval = 3f;
    [SerializeField] public float InFocusDecayMultiplier = 0.5f;
    [SerializeField] public bool IntegrityDecay = true;
    [SerializeField] public int recoverIntegrityLimit = 50;
    [SerializeField] public bool InFocus = false;
    [SerializeField] public bool Interactable = true;
    [SerializeField] public bool IsBeingRepaired = false;
    [SerializeField] protected bool talkInOrder = true;
    [SerializeField] protected bool talkRepeatable = true;
    [SerializeField] protected float talkRollCD = 2f;
    [SerializeField] protected float talkChance = 0.25f;
    [SerializeField] protected float corruptRollCD = 10f;
    [SerializeField] protected float minCorruptRoll = 0.3f; //minroll
    [SerializeField] protected float maxCorruptRoll = 0.7f; //start rolling at this integrity
    [SerializeField] public List<AnimatorClipsPair> defaultAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> idleAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> dialoguePlayingAnimation; //plays while dialogue is playing
    [SerializeField] public List<AnimatorClipsPair> dialogueTypingAnimation; //plays when playing a typing sound
    [SerializeField] public List<AnimatorClipPair> normalAnimatorClips;
    [SerializeField] public List<AnimatorClipPair> corruptAnimatorClips;
    [SerializeField] public List<AnimatorClipsPair> extraAnimatorClipsPairs;

    public enum AnimationState
    {
        Default,
        Idle,
    }
    [SerializeField] public AnimationState CurrentAnimationState = AnimationState.Default;

    protected float rollTalkTimer = 0f;
    protected float rollCorruptTimer = 0f;
    public bool corrupted {get; set;} = false;
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
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair && !IsBeingRepaired)
        {
            PlayerManager.Instance.SetState(PlayerManager.PlayerState.normal);
            StartRepairing();
        }
    }
    protected virtual void SubmitInteract()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair)
        {
            return;
        }
        if(!Interactable) 
        {
            return;
        }
    }

    public virtual void StartRepairing()
    {
        IsBeingRepaired = true;
        Interactable = false;
        IntegrityDecay = false;
        StartCoroutine(Repairing());
    }

    protected virtual IEnumerator Repairing()
    {
        float repairTime = 3f;
        float elapsedTime = 0f;
        int repairAmount = 30;
        while(elapsedTime < repairTime)
        {
            elapsedTime += Time.deltaTime;
            AddIntegrity(repairAmount * Time.deltaTime / repairTime);
            yield return null;
        }
        FinishRepairing();
    }

    public virtual void FinishRepairing()
    {
        IsBeingRepaired = false;
        Interactable = true;
        IntegrityDecay = true;
    }

    protected virtual void Update()
    {
        if(!GameManager.Instance.isStreaming) return;

        if(IsBeingRepaired) return;

        if(InFocus)
        {
            InFocusBehavior();
        }
        else
        {
            OutOfFocusBehavior();
        }

        if(corrupted)
        {
            CorruptBehavior();
        }
        else
        {
            NormalBehavior();
            RollChanceToCorrupt();
        }
        
        Decay();

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
    protected virtual void InFocusBehavior()
    {
        if(InputManager.Instance.Submit.triggered)
        {
            SubmitInteract();
        }
    }

    protected virtual void OutOfFocusBehavior()
    {

    }

    public void SetInFocus(bool focus)
    {
        InFocus = focus;
        dialogueManager.PlaySound = focus;
    }

    public void AddIntegrity(float amount)
    {
        Integrity += amount;
        if (Integrity > MaxIntegrity)
        {
            Integrity = MaxIntegrity;
        }
        if (Integrity < 0)
        {
            Integrity = 0;
        }
    }

    protected virtual void Decay()
    {
        if(!IntegrityDecay || decayInterval <= 0) return;
        float decayMultiplier = InFocus ? InFocusDecayMultiplier : 1f;
        AddIntegrity(-1/decayInterval * Time.deltaTime * decayMultiplier);
        
    }

    protected virtual void PlayDefaultAnimation()
    {
        for(int i = 0; i < defaultAnimatorClips.Count; i++)
        {
            foreach(ClipLayerPair clipLayerPair in defaultAnimatorClips[i].clipLayerPairs)
            {
                if(clipLayerPair.clip != null)
                    defaultAnimatorClips[i].animator.CrossFade(clipLayerPair.clip.name, 0.2f, clipLayerPair.layer);
            }
        }
        CurrentAnimationState = AnimationState.Default;
    }

    protected virtual void PlayIdleAnimation()
    {
        for(int i = 0; i < idleAnimatorClips.Count; i++)
        {
            if(idleAnimatorClips[i].animator != null)
            {
                foreach(ClipLayerPair clipLayerPair in idleAnimatorClips[i].clipLayerPairs)
                {
                    if(clipLayerPair.clip != null)
                        idleAnimatorClips[i].animator.CrossFade(clipLayerPair.clip.name,0.2f, clipLayerPair.layer);
                }
            }
        }
        CurrentAnimationState = AnimationState.Idle;
    }

    protected virtual void SetAppearance(int index)
    {
        for (int i = 0; i < extraAnimatorClipsPairs.Count; i++)
        {
            if (extraAnimatorClipsPairs[i].animator != null )
            { 
                if(extraAnimatorClipsPairs[i].clipLayerPairs[index].clip != null)
                {
                    extraAnimatorClipsPairs[i].animator.CrossFade(extraAnimatorClipsPairs[i].clipLayerPairs[index].clip.name,0.2f, extraAnimatorClipsPairs[i].clipLayerPairs[index].layer);
                }
            }
        }
    }

    protected virtual void SetNormalAppearance()
    {
        foreach(AnimatorClipPair clipPair in normalAnimatorClips)
        {
            if(clipPair.animator != null) 
            {

                if(clipPair.clipLayerPair.clip != null)
                {
                    clipPair.animator.CrossFade(clipPair.clipLayerPair.clip.name, 0.2f, clipPair.clipLayerPair.layer);
                }

            }
        }
    }

    protected virtual void SetCorruptAppearance()
    {
        foreach(AnimatorClipPair clipPair in corruptAnimatorClips)
        {
            if(clipPair.animator != null)
            {
                if(clipPair.clipLayerPair.clip != null)
                {
                    clipPair.animator.CrossFade(clipPair.clipLayerPair.clip.name, 0.2f, clipPair.clipLayerPair.layer);
                }
            }
        }
    }

    protected void PlayAnimation(int animatorIndex = 0, int clipIndex = 0)
    {
        if (animatorIndex < 0 || animatorIndex >= extraAnimatorClipsPairs.Count) return;
        if (extraAnimatorClipsPairs[animatorIndex].clipLayerPairs[clipIndex].clip != null)
        {
            extraAnimatorClipsPairs[animatorIndex].animator.CrossFade(extraAnimatorClipsPairs[animatorIndex].clipLayerPairs[clipIndex].clip.name, 0.2f, extraAnimatorClipsPairs[animatorIndex].clipLayerPairs[clipIndex].layer);
        }
    }

    protected virtual void RollChanceToTalk()
    {
        if(!dialogueManager.IsDialoguePlaying)
        {
            if(rollTalkTimer >= talkRollCD)
            {
                if(Random.Range(0f, 1f) < talkChance)
                {
                    Speak();
                }
                rollTalkTimer = 0f;
            }
            else rollTalkTimer += Time.deltaTime;
        }
    }

    protected virtual void NormalBehavior()
    {
        SharedBehavior();
    }

    protected virtual void CorruptBehavior()
    {
        SharedBehavior();
    }

    protected virtual void SharedBehavior()
    {

    }

    public virtual void EnterCorruptState()
    {
        corrupted = true;
        dialogueSetIndex = 1;
        ShutUp();
        SetCorruptAppearance();
        
    }


    public virtual void ExitCorruptState()
    {
        corrupted = false;
        dialogueSetIndex = 0;
        rollCorruptTimer = 0f;
        SetNormalAppearance();
        Integrity = Mathf.Max(Integrity, recoverIntegrityLimit);
    }

    protected virtual void RollChanceToCorrupt()
    {
        if(rollCorruptTimer >= corruptRollCD)
        {
            float integrityRatio = Integrity / MaxIntegrity;
            if (integrityRatio <= minCorruptRoll)
            {
                EnterCorruptState();
            }
            else if (integrityRatio < maxCorruptRoll)
            {
                float t = (integrityRatio - maxCorruptRoll) / (minCorruptRoll-maxCorruptRoll);
                float probability = Mathf.Pow(t, 3); //cubic curve
                if (Random.Range(0f, 1f) < probability)
                {
                    EnterCorruptState();
                }
            }
            rollCorruptTimer = 0f;
        }
        else rollCorruptTimer += Time.deltaTime;        

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
