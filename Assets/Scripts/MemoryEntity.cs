using UnityEngine;

public abstract class MemoryEntity : MonoBehaviour
{
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueTextSO dialogueTextSO;
    [SerializeField] private float timeToShutup = 5f;

    [SerializeField] protected GameObject npcVisual;

    private float elapsedTimeSinceUninteractable = 0f;
    public int Integrity {get; set;}
    public int MaxIntegrity {get; set;}
    public int Influence {get; set;}
    public int MaxInfluence {get; set;}
    public bool Interactable {get; set;} = false;
    public abstract void Interact();

    protected virtual void Update()
    {

        //Interactable = IsPlayerInRange();
        if(InputManager.Instance.Submit.triggered && Interactable)
        {
            Interact();
        }

        if (dialogueManager.IsDialoguePlaying && !Interactable)
        {
            elapsedTimeSinceUninteractable += Time.deltaTime;
            if (elapsedTimeSinceUninteractable >= timeToShutup)
            {
                ShutUp();
                elapsedTimeSinceUninteractable = 0f;
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
