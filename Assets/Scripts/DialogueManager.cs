using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private Queue<string> dialogueTextQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    public bool IsTyping {get; private set;} = false;

    public bool IsDialoguePlaying {get; private set;} = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void PlayDialogue(DialogueTextSO dialogueTextSO)
    {
        if(IsDialoguePlaying)
        {
            if(IsTyping && typingCoroutine != null)
            {
                FinishTyping();
            }
            else 
            {
                NextDialogue();
            }
        } 
        else 
        {
            StartDialogue(dialogueTextSO);
        }
    }


    private void StartDialogue(DialogueTextSO dialogueTextSO)
    {
        IsDialoguePlaying = true;
        dialoguePanel.SetActive(true);
        speakerNameText.text = dialogueTextSO.speakerName;
        foreach(string dialogue in dialogueTextSO.dialogueText)
        {
            dialogueTextQueue.Enqueue(dialogue);
        }
        NextDialogue();
    }
    public bool FinishedDialogue()
    {
        return dialogueTextQueue.Count == 0;
    }
    private void NextDialogue()
    {
        if(dialogueTextQueue.Count > 0)
        {
            dialogueText.text = dialogueTextQueue.Dequeue();
            typingCoroutine = StartCoroutine(TypeDialogueText());
        }
        else
        {
            EndDialogue();
        }
    }

    private void FinishTyping()
    {
        StopCoroutine(typingCoroutine);
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        IsTyping = false;
    }



    public void EndDialogue()
    {
        IsDialoguePlaying = false;
        speakerNameText.text = "";
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
    }

    IEnumerator TypeDialogueText()
    {
        IsTyping = true;
        dialogueText.maxVisibleCharacters = 0;
        int counter = 0;
        foreach(char letter in dialogueText.text.ToCharArray())
        {
            counter++;
            dialogueText.maxVisibleCharacters = counter;
            yield return new WaitForSeconds(0.01f);
        }
        IsTyping = false;
    }
}
