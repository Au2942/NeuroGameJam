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
    [SerializeField] protected GameObject[] appearancePhases;

    protected int phase = 0;
    private float decayTimerElapsed = 0f;
    private float elapsedTimeSinceInFocus = 0f;

    // public int Influence {get; set;}
    // public int MaxInfluence {get; set;}
    [SerializeField] public bool InFocus = false;

    protected abstract void PhaseOneBehaviour();
    protected abstract void PhaseTwoBehaviour();
    protected abstract void PhaseThreeBehaviour();
    protected virtual void Interact()
    {
        Talk();
    }

    protected virtual void Awake()
    {
        GameManager.Instance.OnDayEnd += OnDayEnd;
        GameManager.Instance.OnDayStart += OnDayStart;

    }

    protected virtual void OnDayEnd()
    {

    }
    protected virtual void OnDayStart()
    {

    }
    protected virtual void Update()
    {
        if(!GameManager.Instance.isStreaming) return;
        //Interactable = IsPlayerInRange();
        if(InputManager.Instance.Submit.triggered && InFocus)
        {
            Interact();
        }

        if (dialogueManager.IsDialoguePlaying && !InFocus)
        {
            dialogueManager.PlaySound = false;
            elapsedTimeSinceInFocus += Time.deltaTime;
            if (elapsedTimeSinceInFocus >= timeToShutup)
            {
                ShutUp();
                elapsedTimeSinceInFocus = 0f;
            }
        }
        Behave();
        Decay();
        UpdatePhase();
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
        if(InFocus) decayTimerElapsed += Time.deltaTime/2;
        else decayTimerElapsed += Time.deltaTime;
        if(decayTimerElapsed >= decayTimer)
        {
            Integrity--;
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

    public void Talk()
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
        dialogueManager.PlayDialogue(dialogues[randomIndex]);
    }

    public void ShutUp()
    {
        dialogueManager.EndDialogue();
    }
}
