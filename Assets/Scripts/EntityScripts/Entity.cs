using UnityEngine;
using System.Collections.Generic;

public abstract class Entity : MonoBehaviour
{

    [SerializeField] protected GameObject Body;
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueSet[] dialogueSets;
    [SerializeField] protected AnimatorClipPair[] normalAnimatorClips;
    [SerializeField] protected AnimatorClipPair[] corruptAnimatorClips;
    [SerializeField] protected AnimatorClipsPair[] animatorClipsPairs;
    [SerializeField] public int Integrity = 100;
    [SerializeField] public int MaxIntegrity = 100;
    [SerializeField] public float decayCD = 3f;
    [SerializeField] public float InFocusDecayMultiplier = 0.5f;
    [SerializeField] public bool IntegrityDecay = true;
    [SerializeField] protected bool inFocus = false;
    [SerializeField] public int recoverIntegrityLimit = 50;
    [SerializeField] protected float talkRollCD = 2f;
    [SerializeField] protected float talkChance = 0.25f;
    [SerializeField] protected float corruptRollCD = 10f;
    [SerializeField] protected float minCorruptRoll = 0.3f; //minroll
    [SerializeField] protected float maxCorruptRoll = 0.7f; //start rolling at this integrity

    protected float rollTalkTimer = 0f;
    protected float rollCorruptTimer = 0f;
    protected float decayTimer = 0f;
    public bool corrupted {get; set;} = false;
    protected int dialogueSetIndex = 0;

    protected int talkCounter = 0;


    protected virtual void Interact()
    {
        
    }

    protected virtual void Awake()
    {
        GameManager.Instance.OnStartStream += OnStartStream;
        GameManager.Instance.OnEndStream += OnEndStream;
    }
    protected virtual void Start()
    {
        SetNormalAppearance();
    }
    protected virtual void OnStartStream()
    {

    }
    protected virtual void OnEndStream()
    {
        
    }

    protected virtual void Update()
    {
        if(!GameManager.Instance.isStreaming) return;
        //Interactable = IsPlayerInRange();

        if(inFocus)
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

    protected virtual void InFocusBehavior()
    {
        if(InputManager.Instance.Submit.triggered)
        {
            Interact();
        }
    }

    protected virtual void OutOfFocusBehavior()
    {

    }

    public void SetInFocus(bool focus)
    {
        inFocus = focus;
        dialogueManager.PlaySound = focus;
    }

    public void AddIntegrity(int amount)
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
        if(!IntegrityDecay) return;

        float elapsedTime = Time.deltaTime;
        if(inFocus) elapsedTime *= InFocusDecayMultiplier;
        
        if(decayTimer >= decayCD)
        {
            AddIntegrity(-1);
            decayTimer = 0f;
        }
        else decayTimer += elapsedTime;
        
    }
    protected virtual void SetAppearance(int index)
    {
        for (int i = 0; i < animatorClipsPairs.Length; i++)
        {
            if (animatorClipsPairs[i].animator != null )
            { 
                for(int j = 0; j < animatorClipsPairs[i].clipsByLayer.Length; j++)
                {
                    if(animatorClipsPairs[i].clipsByLayer[j].clips[index] != null)
                    {
                        animatorClipsPairs[i].animator.CrossFade(animatorClipsPairs[i].clipsByLayer[j].clips[index].name, 0.2f, j);
                    }
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
                for(int i = 0; i < clipPair.clipByLayer.Length; i++)
                {
                    if(clipPair.clipByLayer[i] != null)
                    {
                        clipPair.animator.CrossFade(clipPair.clipByLayer[i].name, 0.2f, i);
                    }
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
                for(int i = 0; i < clipPair.clipByLayer.Length; i++)
                {
                    if(clipPair.clipByLayer[i] != null)
                    {
                        clipPair.animator.CrossFade(clipPair.clipByLayer[i].name, 0.2f, i);
                    }
                }
            }
        }
    }

    protected void PlayAnimation(int animatorIndex, int layer, int clipIndex)
    {
        if (animatorIndex < 0 || animatorIndex >= animatorClipsPairs.Length) return;
        if (layer < 0 || layer >= animatorClipsPairs[animatorIndex].clipsByLayer.Length) return;
        if (clipIndex < 0 || clipIndex >= animatorClipsPairs[animatorIndex].clipsByLayer[layer].clips.Length) return;
        if (animatorClipsPairs[animatorIndex].animator != null)
        {
            AnimationClip clip = animatorClipsPairs[animatorIndex].clipsByLayer[layer].clips[clipIndex];
            if (clip != null)
            {
                animatorClipsPairs[animatorIndex].animator.CrossFade(clip.name, 0.2f, layer);
            }
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
            float integrityRatio = (float)Integrity / MaxIntegrity;
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
        int randomIndex = Random.Range(0, dialogues.Count);
        dialogueManager.PlayDialogue(dialogues[randomIndex], playerInitiated);
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
}
