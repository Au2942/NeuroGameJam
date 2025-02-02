using UnityEngine;
using System.Collections.Generic;

public class StreamEntity : Entity
{
    [SerializeField] protected StreamEntityData streamEntityData;
    protected List<AnimatorClipsPair> SleepAnimatorClips {get => streamEntityData.SleepAnimatorClips; set => streamEntityData.SleepAnimatorClips = value;}

    protected override void Awake()
    {
        base.Awake();
        if(streamEntityData == null)
        {
            streamEntityData = GetComponent<StreamEntityData>();
            if(streamEntityData == null)
            {
                streamEntityData = gameObject.AddComponent<StreamEntityData>();
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        Health = PlayerManager.Instance.Health;
        MaxHealth = PlayerManager.Instance.MaxHealth;
        PlayerManager.Instance.OnTakeDamageEvent += DamageHealth;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Update()
    {  
        Health = PlayerManager.Instance.Health;
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.sleep) return;
        base.Update();
    }

    public override void EnterGlitchState()
    {
        base.EnterGlitchState();
        Interactable = false;
    }
    public override void ExitGlitchState()
    {
        base.ExitGlitchState();
        Interactable = true;
    }

    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();

    }
    public void EnterSleepState()
    {
        foreach(AnimatorClipsPair sleepAnimatorClip in SleepAnimatorClips)
        {
            if(sleepAnimatorClip.animator == null) continue;
            foreach(ClipLayerPair clipInfo in sleepAnimatorClip.clipLayerPairs)
            {
                if(clipInfo.clip == null) continue;
                PlayAnimation(sleepAnimatorClip.animator, clipInfo.clip, clipInfo.layer);
            }
        }
        
    }
    public void ExitSleepState()
    {
        SetNormalAppearance();
    }

    protected override void OnDestroy()
    {
        base.OnDisable();
        PlayerManager.Instance.OnTakeDamageEvent -= DamageHealth;
    }

}