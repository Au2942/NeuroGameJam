using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueTextSO dialogueTextSO;
    public int Integrity {get; set;}
    public int MaxIntegrity {get; set;}
    public int Influence {get; set;}
    public int MaxInfluence {get; set;}
    public bool Interactable {get; set;} = false;

    public abstract void Interact();

    void Update()
    {
        if(InputManager.Instance.Submit.triggered && Interactable)
        {
            Interact();
        }
    }

    public void Talk()
    {
        if(dialogueManager == null || dialogueTextSO == null)
        {
            return;
        }

        dialogueManager.PlayDialogue(dialogueTextSO);
    }
}
