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
public struct AnimatorClipsPair
{
    public Animator animator;

    public List<ClipLayerPair> clipLayerPairs;

    public AnimatorClipsPair(Animator animator, List<ClipLayerPair> clipLayerPairs)
    {
        this.animator = animator;
        this.clipLayerPairs = clipLayerPairs;
    }
}
[System.Serializable]
public struct ClipLayerPair
{
    public AnimationClip clip;
    public int layer;

    public ClipLayerPair(AnimationClip clip, int layer)
    {
        this.clip = clip;
        this.layer = layer;
    }
}
[System.Serializable]
public struct AnimatorClipPair
{
    public Animator animator;
    public ClipLayerPair clipLayerPair;

    public AnimatorClipPair(Animator animator, ClipLayerPair clipLayerPair)
    {
        this.animator = animator;
        this.clipLayerPair = clipLayerPair;
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
