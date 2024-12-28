using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected DialogueTextSO dialogueTextSO;
    public int integrity;
    public int maxIntegrity;
    public int influence;
    public int maxInfluence;
    public bool interactable = false;

    public abstract void Interact();

    void Update()
    {
        if(InputManager.Instance.Submit.triggered && interactable)
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
