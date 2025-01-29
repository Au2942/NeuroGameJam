using UnityEngine;
using System.Collections.Generic;

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
public struct AnimatorStateInfosPair
{
    public Animator animator;
    public List<AnimatorStateInfo> stateInfos;

    public AnimatorStateInfosPair(Animator animator, List<AnimatorStateInfo> stateInfos)
    {
        this.animator = animator;
        this.stateInfos = stateInfos;
    }
}
