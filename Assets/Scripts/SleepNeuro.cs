using UnityEngine;
using System.Collections.Generic;

public class SleepNeuro : MonoBehaviour, ISpeaker
{
    [SerializeField] private List<AnimatorClipsPair> awakeClips;
    [SerializeField] private List<AnimatorClipsPair> sleepClips;
    [SerializeField] private List<AnimatorClipsPair> dialoguePlayingClips;
    [SerializeField] private List<AnimatorClipsPair> dialogueTypingClips;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSet DialogueSets;
    [SerializeField] private float dialogueInterval = 10f;
    private float dialogueTimer = 0f;
    private bool isAwake = true;
    public List<AnimatorClipsPair> DialoguePlayingAnimation { get => dialoguePlayingClips; set => dialoguePlayingClips = value; }
    public List<AnimatorClipsPair> DialogueTypingAnimation { get => dialogueTypingClips; set => dialogueTypingClips = value; }


    private void Start()
    {
        SetAwakeState(isAwake);
    }
    
    private void Update()
    {
        if (isAwake)
        {
            dialogueTimer += Time.unscaledDeltaTime;
            if (dialogueTimer >= dialogueInterval)
            {
                dialogueTimer = 0f;
                SayDialogue();
            }
        }
    }

    public void SetAwakeState(bool isAwake)
    {
        this.isAwake = isAwake;
        if (this.isAwake)
        {
            PlayAwakeClips();
        }
        else
        {
            PlaySleepClips();
            ShutUp();
        }
    }
    

    private void PlayAwakeClips()
    {
        foreach(AnimatorClipsPair animatorClipsPair in awakeClips)
        {
            if(animatorClipsPair.animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in animatorClipsPair.clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(animatorClipsPair.animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
    }

    private void PlaySleepClips()
    {
        foreach(AnimatorClipsPair animatorClipsPair in sleepClips)
        {
            if(animatorClipsPair.animator == null) {continue;}
            foreach(ClipLayerPair clipLayerPair in animatorClipsPair.clipLayerPairs)
            {
                if(clipLayerPair.clip == null) {continue;}
                PlayAnimation(animatorClipsPair.animator, clipLayerPair.clip, clipLayerPair.layer);
            }
        }
    }
    
    void PlayAnimation(Animator animator, AnimationClip clip, int layer = 0)
    {
        if (animator == null || clip == null) return;
        animator.CrossFade(clip.name, 0.2f, layer);
    }

    private void SayDialogue()
    {
        int randomIndex = Random.Range(0, DialogueSets.dialogues.Count);
        dialogueManager.PlayDialogue(DialogueSets.dialogues[randomIndex]);
    }

    private void ShutUp()
    {
        dialogueManager.EndDialogue();
    }


}