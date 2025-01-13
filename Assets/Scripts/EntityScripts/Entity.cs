using UnityEngine;
using System.Collections.Generic;

public abstract class Entity : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueSet
    {
        public List<DialogueInfoSO> dialogues;
    }
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueSet[] dialogueSets;
    [SerializeField] protected GameObject[] appearances;
    [SerializeField] public int Integrity = 100;
    [SerializeField] public int MaxIntegrity = 100;
    [SerializeField] public float decayCD = 3f;
    [SerializeField] public float InFocusDecayMultiplier = 0.5f;
    [SerializeField] public bool IntegrityDecay = true;
    [SerializeField] protected bool inFocus = false;
    [SerializeField] public int recoverIntegrityLimit = 50;
    [SerializeField] protected float talkRollCD = 2f;

    [SerializeField] protected float talkChance = 0.25f; 
    [SerializeField] protected float glitchRollCD = 10f;

    [SerializeField] protected float minGlitchRoll = 0.3f; //minroll
    [SerializeField] protected float maxGlitchRoll = 0.7f; //start rolling at this integrity



    protected float rollTalkTimer = 0f;
    protected float rollGlitchTimer = 0f;
    protected float decayTimer = 0f;
    public bool glitched {get; set;} = false;
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
        SetAppearance(0);
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

        RollChanceToGlitch();

        if(glitched)
        {
            GlitchBehavior();
        }
        else
        {
            NormalBehavior();
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
        for (int i = 0; i < appearances.Length; i++)
        {
            if (appearances[i] != null)
            {
                appearances[i].SetActive(i == index);
            }
        }
        if(appearances.Length-1 < index)
        {
            appearances[0].SetActive(true);
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

    protected virtual void GlitchBehavior()
    {
        SharedBehavior();
    }

    protected virtual void SharedBehavior()
    {

    }

    public virtual void EnterGlitchState()
    {
        glitched = true;
        dialogueSetIndex = 1;
        ShutUp();
        SetAppearance(1);
    }


    public virtual void ExitGlitchState()
    {
        glitched = false;
        dialogueSetIndex = 0;
        rollGlitchTimer = 0f;
        SetAppearance(0);
        Integrity = Mathf.Max(Integrity, recoverIntegrityLimit);
    }

    protected virtual void RollChanceToGlitch()
    {
        if(rollGlitchTimer >= glitchRollCD)
        {
            float integrityRatio = (float)Integrity / MaxIntegrity;
            if (integrityRatio <= minGlitchRoll)
            {
                EnterGlitchState();
            }
            else if (integrityRatio < maxGlitchRoll)
            {
                float t = (integrityRatio - maxGlitchRoll) / (minGlitchRoll-maxGlitchRoll);
                float probability = Mathf.Pow(t, 3); //cubic curve
                if (Random.Range(0f, 1f) < probability)
                {
                    EnterGlitchState();
                }
            }
            rollGlitchTimer = 0f;
        }
        else rollGlitchTimer += Time.deltaTime;        

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
