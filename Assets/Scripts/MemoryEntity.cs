using UnityEngine;

public abstract class MemoryEntity : MonoBehaviour
{
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueInfoSO dialogueTextSO;
    [SerializeField] private float timeToShutup = 5f;

    [SerializeField] protected GameObject npcVisual;

    private float elapsedTimeSinceInFocus = 0f;
    public int Integrity {get; set;}
    public int MaxIntegrity {get; set;}
    public int Influence {get; set;}
    public int MaxInfluence {get; set;}
    [SerializeField] public bool InFocus = false;
    public abstract void Interact();

    protected virtual void Update()
    {

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
        if (dialogueManager == null || dialogueTextSO == null)
        {
            return;
        }
        dialogueManager.PlayDialogue(dialogueTextSO);
    }

    public void ShutUp()
    {
        dialogueManager.EndDialogue();
    }
}
