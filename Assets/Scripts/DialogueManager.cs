using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public struct DialogueSet
{
    public List<DialogueInfoSO> dialogues;

    public DialogueSet(List<DialogueInfoSO> dialogues)
    {
        this.dialogues = dialogues;
    }
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private UIEventHandler dialogueInteractDetector;
    [SerializeField] private DialogueInfoSO defaultDialogueInfo;
    [SerializeField] private ISpeaker speaker;
    [SerializeField] public bool Interactable = false;
    [SerializeField] private bool stayOnScreen = false;
    [SerializeField] public bool PlayAnimation = true;
    [SerializeField] public bool PlaySound = true;
    [SerializeField] public bool LocationBasedSound = true;
    [SerializeField] public bool UnscaledTime = false;

    private AudioSource audioSource;
    private DialogueInfoSO currentDialogueInfo;
    private Queue<string> dialogueTextQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    private Coroutine waitAndPlayNextDialogueCoroutine;
    private Coroutine typingAnimationCoroutine;
    private List<AnimatorStateInfosPair> storedPlayingAnimationAnimatorStateInfosPairs = new List<AnimatorStateInfosPair>();
    private List<AnimatorStateInfosPair> storedTypingAnimationAnimatorStateInfosPairs = new List<AnimatorStateInfosPair>();
    public bool IsTyping {get; private set;} = false;
    public bool IsDialoguePlaying {get; private set;} = false;

    private System.Action<PointerEventData> DialogueInteractedHandler;

    void Awake()
    {
        currentDialogueInfo = defaultDialogueInfo;
    }

    void OnEnable()
    {
        DialogueInteractedHandler = (eventData) => { if(Interactable) PlayDialogue(); };
        if(dialogueInteractDetector != null)
        {
            dialogueInteractDetector.OnLeftClickEvent += DialogueInteractedHandler;
        }
    }

    void Start()
    {
        UpdateLayoutElement();
        dialoguePanel.gameObject.SetActive(false);
    }
    

    private void UpdateLayoutElement()
    {
        layoutElement.enabled = dialogueText.preferredWidth > layoutElement.preferredWidth || speakerNameText.preferredWidth > layoutElement.preferredWidth;
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel);
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

    public void PlayDialogue()
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
    }


    private void StartDialogue()
    {   
        if(PlayAnimation) 
        {
            StoreInitialAnimationInfosAndPlay();
        }
        if(PlaySound)
        {
            audioSource = SFXManager.Instance.GetAudioSource();
            if(LocationBasedSound && audioSource != null)
            {
                audioSource.gameObject.transform.position = transform.position;
            } 
        }
        IsDialoguePlaying = true;
        dialoguePanel.gameObject.SetActive(true);
        if(speakerNameText != null)
        {
            speakerNameText.text = currentDialogueInfo.speakerName;
            if(string.IsNullOrEmpty(speakerNameText.text))
            {
                speakerNameText.gameObject.SetActive(false);
            }
        }
        foreach(string dialogue in currentDialogueInfo.dialogueText)
        {
            dialogueTextQueue.Enqueue(dialogue);
        }
        NextDialogue();
    }

    private void StoreInitialAnimationInfosAndPlay()
    {
        storedPlayingAnimationAnimatorStateInfosPairs.Clear();
        if(speaker != null)
        {
            foreach(AnimatorClipsPair animation in speaker.DialoguePlayingAnimation)
            {
                if(animation.animator != null)
                {
                    List<AnimatorStateInfo> animatorStateInfos = new List<AnimatorStateInfo>();
                    for (int i = 0; i < animation.animator.layerCount; i++)
                    {
                        animatorStateInfos.Add(animation.animator.GetCurrentAnimatorStateInfo(i));
                    }
                    storedPlayingAnimationAnimatorStateInfosPairs.Add(new AnimatorStateInfosPair(animation.animator, animatorStateInfos));
                    
                    for(int i = 0; i < animation.clipLayerPairs.Count; i++)
                    {
                        if(animation.clipLayerPairs[i].clip != null)
                        {
                            animation.animator.CrossFade(animation.clipLayerPairs[i].clip.name, 0.2f, animation.clipLayerPairs[i].layer);
                        }
                    }
                }
            }
        }

        storedTypingAnimationAnimatorStateInfosPairs.Clear();
        if(speaker != null && speaker.DialogueTypingAnimation.Count != 0)
        {
            foreach(AnimatorClipsPair animation in speaker.DialogueTypingAnimation)
            {
                if(animation.animator != null)
                {
                    List<AnimatorStateInfo> animatorStateInfos = new List<AnimatorStateInfo>();
                    for (int i = 0; i < animation.animator.layerCount; i++)
                    {
                        animatorStateInfos.Add(animation.animator.GetCurrentAnimatorStateInfo(i));
                    }
                    storedTypingAnimationAnimatorStateInfosPairs.Add(new AnimatorStateInfosPair(animation.animator, animatorStateInfos));
                }
            }
        }
        
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
        if(UnscaledTime)
        {
            yield return new WaitForSecondsRealtime(1f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        NextDialogue();

    }

    public void EndDialogue()
    {
        if(PlayAnimation)
        {
            foreach(AnimatorStateInfosPair pair in storedPlayingAnimationAnimatorStateInfosPairs)
            {
                if(pair.animator != null)
                {
                    for(int i = 0; i < pair.animator.layerCount; i++)
                    {
                        pair.animator.CrossFade(pair.stateInfos[i].fullPathHash,0.2f, i, pair.stateInfos[i].normalizedTime);
                    }
                }
            }
        }
        if(stayOnScreen)
        {
            return;
        }
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        if(waitAndPlayNextDialogueCoroutine != null)
        {
            StopCoroutine(waitAndPlayNextDialogueCoroutine);
        }
        if(audioSource != null)
        {
            SFXManager.Instance.ReturnAudioSource(audioSource);
            audioSource = null;
        }
        IsDialoguePlaying = false;
        if(speakerNameText != null) speakerNameText.text = string.Empty;
        {
            speakerNameText.text = "";
        }
        dialogueText.text = "";
        currentDialogueInfo = defaultDialogueInfo;
        dialoguePanel.gameObject.SetActive(false);
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
                if(currentDialogueInfo.audioClips.Length > 0)
                {
                    if(PlaySound)
                    {
                        PlayDialogueTypingSound(validCharCount, dialogueText.text[charCount]);
                    }
                    else if(PlayAnimation)
                    {
                        PlayDialogueTypingAnimation(validCharCount);
                    }
                }
                charCount++;
                validCharCount++;
                dialogueText.maxVisibleCharacters = charCount;
                UpdateLayoutElement();
                if(UnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(currentDialogueInfo.speakSpeed);
                }
                else
                {
                    yield return new WaitForSeconds(currentDialogueInfo.speakSpeed);
                }
            }
            else
            {
                charCount++;
                dialogueText.maxVisibleCharacters = charCount;
                UpdateLayoutElement();
                if(letter == '.' )
                {
                    if(UnscaledTime)
                    {
                        yield return new WaitForSecondsRealtime(currentDialogueInfo.speakSpeed);
                    }
                    else
                    {
                        yield return new WaitForSeconds(currentDialogueInfo.speakSpeed);
                    }
                }
            }
        }
        IsTyping = false;
        waitAndPlayNextDialogueCoroutine = StartCoroutine(WaitAndPlayNextDialogue());
    }

    private void PlayDialogueTypingAnimation(int currentCharacterIndex, float duration = -1)
    {
        if(speaker == null)
        {
            return;
        }

        if(duration < 0)
        {
            duration = currentDialogueInfo.speakSpeed;
        }

        int frequency = currentDialogueInfo.frequency;
        if(currentCharacterIndex % frequency == 0)
        {
            if(typingAnimationCoroutine != null)
            {
                StopCoroutine(typingAnimationCoroutine);
            }
            typingAnimationCoroutine = StartCoroutine(AnimateDialogueTypingAnimation(duration));
        }
    }

    private IEnumerator AnimateDialogueTypingAnimation(float duration = -1)
    {

        foreach(AnimatorClipsPair animation in speaker.DialogueTypingAnimation)
        {
            if(animation.animator != null)
            {
                for(int i = 0; i < animation.clipLayerPairs.Count; i++)
                {
                    if(animation.clipLayerPairs[i].clip != null)
                    {
                        animation.animator.CrossFade(animation.clipLayerPairs[i].clip.name, 0.2f, animation.clipLayerPairs[i].layer);
                    }
                }
            }
        }
        if(UnscaledTime)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        foreach (AnimatorStateInfosPair pair in storedTypingAnimationAnimatorStateInfosPairs)
        {
            if (pair.animator != null)
            {
                for (int i = 0; i < pair.animator.layerCount; i++)
                {
                    pair.animator.CrossFade(pair.stateInfos[i].fullPathHash,0.2f, i, pair.stateInfos[i].normalizedTime);
                }
            }
        }

    }

    private void PlayDialogueTypingSound(int currentCharacterIndex, char currentCharacter)
    {
        AudioClip[] audioClips = currentDialogueInfo.audioClips;
        float minPitch = currentDialogueInfo.MinPitch;
        float maxPitch = currentDialogueInfo.MaxPitch;
        int frequency = currentDialogueInfo.frequency;


        if(audioSource != null && currentCharacterIndex % frequency == 0)
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

            if(UnscaledTime)
            {
                audioSource.clip = audioClips[audioIndex];
                audioSource.Play();
            }
            else
            {
                audioSource.PlayOneShot(audioClips[audioIndex]);
            }
            if(PlayAnimation)
            {
                PlayDialogueTypingAnimation(currentCharacterIndex, audioClips[audioIndex].length);
            }
        }
    }

    void OnDisable()
    {
        dialogueInteractDetector.OnLeftClickEvent -= DialogueInteractedHandler;
    }


}