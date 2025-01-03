using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private DialogueInfoSO defaultDialogueInfo;
    private DialogueInfoSO currentDialogueInfo;
    private Queue<string> dialogueTextQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    private Coroutine waitAndPlayNextDialogueCoroutine;
    public bool IsTyping {get; private set;} = false;
    public bool PlaySound {get; set;} = true;

    public bool IsDialoguePlaying {get; private set;} = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }



    public void PlayDialogue(DialogueInfoSO dialogueTextSO, bool playerInitiated = true)
    {
        if(playerInitiated && waitAndPlayNextDialogueCoroutine != null)
        {
            StopCoroutine(waitAndPlayNextDialogueCoroutine);
        }

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
            currentDialogueInfo = dialogueTextSO;
            StartDialogue();
        }
    }


    private void StartDialogue()
    {
        IsDialoguePlaying = true;
        dialoguePanel.SetActive(true);
        speakerNameText.text = currentDialogueInfo.speakerName;
        foreach(string dialogue in currentDialogueInfo.dialogueText)
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
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }        
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        IsTyping = false;
    }

    private IEnumerator WaitAndPlayNextDialogue()
    {
        yield return new WaitForSeconds(1f);
        NextDialogue();

    }



    public void EndDialogue()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        if(waitAndPlayNextDialogueCoroutine != null)
        {
            StopCoroutine(waitAndPlayNextDialogueCoroutine);
        }
        IsDialoguePlaying = false;
        speakerNameText.text = "";
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
    }

    IEnumerator TypeDialogueText()
    {
        IsTyping = true;
        dialogueText.maxVisibleCharacters = 0;
        int charCount = 0;
        int validCharCount = 0;
        foreach(char letter in dialogueText.text.ToCharArray())
        {
            if(!char.IsWhiteSpace(letter) && !char.IsSymbol(letter) && !char.IsPunctuation(letter))
            {
                if(PlaySound && currentDialogueInfo.audioClips.Length > 0)
                {
                    PlayDialogueTypingSound(validCharCount, dialogueText.text[charCount]);
                }
                charCount++;
                validCharCount++;
                dialogueText.maxVisibleCharacters = charCount;
                yield return new WaitForSeconds(currentDialogueInfo.speakSpeed);
            }
            else
            {
                charCount++;
                dialogueText.maxVisibleCharacters = charCount;
                if(letter == '.' )
                {
                    yield return new WaitForSeconds(currentDialogueInfo.speakSpeed);
                }
            }
        }
        IsTyping = false;
        waitAndPlayNextDialogueCoroutine = StartCoroutine(WaitAndPlayNextDialogue());
    }

    private void PlayDialogueTypingSound(int currentCharacterIndex, char currentCharacter)
    {
        AudioSource audioSource = SFXManager.Instance.SoundFXObjectPrefab;
        AudioClip[] audioClips = currentDialogueInfo.audioClips;
        float minPitch = currentDialogueInfo.MinPitch;
        float maxPitch = currentDialogueInfo.MaxPitch;
        int frequency = currentDialogueInfo.frequency;


        if(currentCharacterIndex % frequency == 0)
        {
            audioSource.Stop();

            int hashCode = (currentCharacter+10000)%11;
            int audioIndex = hashCode % audioClips.Length;

            int minPitchInt = (int)minPitch*100;
            int maxPitchInt = (int)maxPitch*100;
            int pitchRange = maxPitchInt - minPitchInt;
            if(pitchRange != 0)
            {
                int randomPitchInt = hashCode%pitchRange + minPitchInt;
                float randomPitch = randomPitchInt/100f;
                audioSource.pitch = randomPitch;
            }
            else
            {
                audioSource.pitch = minPitch;
            }

            SFXManager.Instance.PlaySoundFX(currentDialogueInfo.audioClips[audioIndex], transform);
        }
    }
}
