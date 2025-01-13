using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DialogueSet
{
    public List<DialogueInfoSO> dialogues;

    public DialogueSet(List<DialogueInfoSO> dialogues)
    {
        this.dialogues = dialogues;
    }
}
[System.Serializable]
public struct Clips
{
    public AnimationClip[] clips;

    public Clips(AnimationClip[] clips)
    {
        this.clips = clips;
    }
}

[System.Serializable]
public struct AnimatorClipsPair
{
    public Animator animator;

    public Clips[] clipsByLayer;

    public AnimatorClipsPair(Animator animator, Clips[] clipsByLayer)
    {
        this.animator = animator;
        this.clipsByLayer = clipsByLayer;
    }
}
[System.Serializable]
public struct AnimatorClipPair
{
    public Animator animator;
    public AnimationClip[] clipByLayer;

    public AnimatorClipPair(Animator animator, AnimationClip[] clipByLayer)
    {
        this.animator = animator;
        this.clipByLayer = clipByLayer;
    }
}
[System.Serializable]
public struct AnimatorStateInfosPair
{
    public Animator animator;
    public List<AnimatorStateInfo> stateInfos;

    public AnimatorStateInfosPair(Animator animator, List<AnimatorStateInfo> stateInfo)
    {
        this.animator = animator;
        this.stateInfos = stateInfo;
    }
}