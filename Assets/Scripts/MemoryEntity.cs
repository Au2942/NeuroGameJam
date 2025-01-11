using UnityEngine;

public abstract class MemoryEntity : MonoBehaviour
{
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueInfoSO[] pOneDialogueTextSOs; //phase 1 dialogues
    [SerializeField] protected DialogueInfoSO[] pTwoDialogueTextSOs; //phase 2 dialogues
    [SerializeField] protected DialogueInfoSO[] pThreeDialogueTextSOs; //phase 3 dialogues
    [SerializeField] public int Integrity = 100;
    [SerializeField] public int MaxIntegrity = 100;
    [SerializeField] public float decayTimer = 3f;
    [SerializeField] private float timeToShutup = 5f;
    [SerializeField] private float attemptTalkCD = 2f;
    [SerializeField] private int talkPerDamage = 3;
    [SerializeField] private float talkIntegrityRecoverCD = 20f;
    [SerializeField] protected GameObject[] appearancePhases;

    public int phase {get; private set;} = 0;
    private float decayTimerElapsed = 0f;
    private float elapsedTimeSinceLeftFocus = 0f;
    private float attemptTalkTimer = 0f;
    private int talkCounter = 0;
    private float talkIntegrityRecoverTimer = 100f;

    // public int Influence {get; set;}
    // public int MaxInfluence {get; set;}
    [SerializeField] private bool inFocus = false;

    protected abstract void PhaseOneBehaviour();
    protected abstract void PhaseTwoBehaviour();
    protected abstract void PhaseThreeBehaviour();
    protected virtual void Interact()
    {
        Converse();
    }

    protected virtual void Awake()
    {
        GameManager.Instance.OnDayEnd += OnDayEnd;
        GameManager.Instance.OnDayStart += OnDayStart;

    }
    protected virtual void Start()
    {

    }

    protected virtual void OnDayEnd()
    {
        dialogueManager.EndDialogue();
    }
    protected virtual void OnDayStart()
    {

    }
    protected virtual void Update()
    {
        if(!GameManager.Instance.isStreaming) return;
        //Interactable = IsPlayerInRange();

        talkIntegrityRecoverTimer += Time.deltaTime;   

        if(inFocus)
        {
            if(InputManager.Instance.Submit.triggered)
            {
                if(talkIntegrityRecoverTimer >= talkIntegrityRecoverCD)
                {
                    AddIntegrity(3);
                    talkIntegrityRecoverTimer = 0f;
                }
                Interact();
            }
            //random chance to talk
            if(!dialogueManager.IsDialoguePlaying)
            {
                attemptTalkTimer += Time.deltaTime;
                if(attemptTalkTimer >= attemptTalkCD)
                {
                    if(Random.Range(0, 100) < 25)
                    {
                        Speak();
                    }
                    attemptTalkTimer = 0f;
                }
            }
        }
        else if (dialogueManager.IsDialoguePlaying)
        {
            elapsedTimeSinceLeftFocus += Time.deltaTime;
            if (elapsedTimeSinceLeftFocus >= timeToShutup)
            {
                ShutUp();
                elapsedTimeSinceLeftFocus = 0f;
            }
        }
        Behave();
        Decay();
        UpdatePhase();
    }

    public void SetInFocus(bool focus)
    {
        inFocus = focus;
        dialogueManager.PlaySound = focus;
        //Debug.Log(name + " is In Focus: " + inFocus);
        if(inFocus)
        {
            elapsedTimeSinceLeftFocus = 0f;
        }
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

    protected virtual void UpdatePhase()
    {
        if (Integrity > 66)
        {
            phase = 0;
            PreparePhaseOne();
        }
        else if (Integrity > 33)
        {
            phase = 1;
            PreparePhaseTwo();
        }
        else
        {
            phase = 2;
            PreparePhaseThree();
        }
    }

    protected virtual void PreparePhaseOne()
    {
        AppearanceChange();
    }

    protected virtual void PreparePhaseTwo()
    {
        AppearanceChange();
    }

    protected virtual void PreparePhaseThree()
    {
        AppearanceChange();
    }

    protected virtual void AppearanceChange()
    {
        for (int i = 0; i < appearancePhases.Length; i++)
        {
            if (appearancePhases[i] != null)
            {
                appearancePhases[i].SetActive(i == phase);
            }
        }

        if (phase < appearancePhases.Length && appearancePhases[phase] == null)
        {
            for (int i = 0; i < appearancePhases.Length; i++)
            {
                if (appearancePhases[i] != null)
                {
                    appearancePhases[i].SetActive(true);
                    break;
                }
            }
        }
    }

    protected virtual void Decay()
    {
        if(inFocus) decayTimerElapsed += Time.deltaTime/4;
        else decayTimerElapsed += Time.deltaTime;
        if(decayTimerElapsed >= decayTimer)
        {
            AddIntegrity(-1);
            decayTimerElapsed = 0f;
        }

    }

    private void Behave()
    {
        if(phase == 0)
        {
            PhaseOneBehaviour();
        }
        else if(phase == 1)
        {
            PhaseTwoBehaviour();
        }
        else if(phase == 2)
        {
            PhaseThreeBehaviour();
        }
    }

    // protected bool IsPlayerInRange()
    // {
    //     RectTransform rectTransform = GetComponent<RectTransform>();

    //     Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         rectTransform,
    //         screenCenter,
    //         null, // Camera is null because it's Screen Space - Overlay
    //         out Vector2 localPoint
    //     );

    //     return rectTransform.rect.Contains(localPoint);
    // }

    public void Converse()
    {
        Talk();
    }

    public void Speak()
    {
        Talk(false);      
    }

    private void Talk(bool playerInitiated = true)
    {
        if (dialogueManager == null || pOneDialogueTextSOs == null)
        {
            return;
        }

        DialogueInfoSO[] dialogues = null;

        switch (phase)
        {
            case 0:
                dialogues = pOneDialogueTextSOs;
                break;
            case 1:
                dialogues = (pTwoDialogueTextSOs != null && pTwoDialogueTextSOs.Length > 0) ? pTwoDialogueTextSOs : pOneDialogueTextSOs;
                break;
            case 2:
                dialogues = (pThreeDialogueTextSOs != null && pThreeDialogueTextSOs.Length > 0) ? pThreeDialogueTextSOs : 
                    (pTwoDialogueTextSOs != null && pTwoDialogueTextSOs.Length > 0) ? pTwoDialogueTextSOs : pOneDialogueTextSOs;
                break;
        }
        if (dialogues == null || dialogues.Length == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, dialogues.Length);
        dialogueManager.PlayDialogue(dialogues[randomIndex], playerInitiated);
        


        talkCounter++;
        if(talkCounter >= talkPerDamage)
        {
            PlayerManager.Instance.TakeDamage(1);
            talkCounter = 0;
        }
    }


    public void ShutUp()
    {
        dialogueManager.EndDialogue();
    }
}
