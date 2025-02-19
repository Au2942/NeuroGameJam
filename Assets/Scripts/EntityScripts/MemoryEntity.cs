using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MemoryEntity : Entity, ICombatant
{
    [SerializeField] protected MemoryEntityData memoryEntityData;
    [Header("Memory Block")]
    public MemoryBlock MemoryBlock {get => memoryEntityData.MemoryBlock; set => memoryEntityData.MemoryBlock = value;}
    protected GlitchEffect GlitchEffect {get => MemoryBlock.GlitchEffect; set => MemoryBlock.GlitchEffect = value;}
    [Header("Playback Timeline")]
    public PlaybackTimeline PlaybackTL {get => MemoryBlock.PlaybackTimeline; set => MemoryBlock.PlaybackTimeline = value;}
    public Playback BasePlayback => PlaybackTL.BasePlayback;
    public Playback ErrorPlayback => PlaybackTL.ErrorPlayback;
    public float MemoryDuration => BasePlayback.PlaybackDuration;
    public bool IsBeingRead {get => memoryEntityData.IsBeingRead; set => memoryEntityData.IsBeingRead = value;}
    public float ReadCorruptAmount {get => memoryEntityData.ReadCorruptAmount; set => memoryEntityData.ReadCorruptAmount = value;}
    public float ReadCorruptInterval {get => memoryEntityData.ReadCorruptInterval; set => memoryEntityData.ReadCorruptInterval = value;}
    [Header("Behaviour")]
    protected float TimeToShutup {get => memoryEntityData.TimeToShutup; set => memoryEntityData.TimeToShutup = value;}
    protected float ShutupTimer {get => memoryEntityData.shutupTimer; set => memoryEntityData.shutupTimer = value;}
    public bool InFocus {get => memoryEntityData.InFocus; set => memoryEntityData.InFocus = value;}
    public bool IsBeingWorkedOn {get => memoryEntityData.IsBeingMaintained; set => memoryEntityData.IsBeingMaintained = value;}
    [Header("Combat")]
    public float AttackDamage {get => memoryEntityData.AttackDamage; set => memoryEntityData.AttackDamage = value;}
    public float AttackRate {get => memoryEntityData.AttackRate; set => memoryEntityData.AttackRate = value;}
    public bool DealAOEDamage {get => memoryEntityData.DealAOEDamage; set => memoryEntityData.DealAOEDamage = value;}
    public List<ICombatant> CombatTargets {get => memoryEntityData.combatTargets; set => memoryEntityData.combatTargets = value;}
    float ICombatant.Health { get => ErrorIndex; set => ErrorIndex = value; }
    float ICombatant.MaxHealth { get => MaxHealth; set => MaxHealth = value; }

    private Coroutine ReadCorruptCoroutine;
    System.Action<float, float> onCorruptPartChangeDelegate;

    protected override void Awake()
    {
        base.Awake();
        if(memoryEntityData == null)
        {
            memoryEntityData = GetComponent<MemoryEntityData>();
            if(memoryEntityData == null)
            {
                memoryEntityData = gameObject.AddComponent<MemoryEntityData>();
            }
        }
        onCorruptPartChangeDelegate = (s,e) => SyncHealthWithPlayback();
        if(PlaybackTL != null)
        {
            PlaybackTL.BasePlayback.OnCorruptPartAdd += onCorruptPartChangeDelegate;
            PlaybackTL.BasePlayback.OnCorruptPartRemove += onCorruptPartChangeDelegate;
            PlaybackTL.ErrorPlayback.OnCorruptPartRemove += onCorruptPartChangeDelegate;
        } 
        
    }

    public virtual void AddCombatTarget(ICombatant target)
    {
        CombatTargets.Add(target);
    }

    public virtual void RemoveCombatTarget(ICombatant target)
    {
        CombatTargets.Remove(target);
    }

    protected virtual void SubmitInteract()
    {
        if(PlayerManager.Instance.State == PlayerManager.PlayerState.command || !Interactable)
        {
            return;
        }
        if(DialogueManager.IsDialoguePlaying)
        {
            DialogueManager.PlayDialogue();
        }
    }

    protected override void Update()
    {
        if(PlayerManager.Instance.State == PlayerManager.PlayerState.sleep) return;
        if(IsBeingWorkedOn) return;
        base.Update();
        if(InFocus)
        {
            InFocusBehavior();
        }
        else
        {
            OutOfFocusBehavior();
        }
    }

    public virtual void SetInFocus(bool focus)
    {
        InFocus = focus;
        DialogueManager.Interactable = focus;
        DialogueManager.PlaySound = focus;
        MemoryBlock.EnableBlockClickDetector(!focus);
    }

    protected override void ClickInteract(GameObject clickedObject)
    {
        base.ClickInteract(clickedObject);
        if(InFocus && PlayerManager.Instance.State == PlayerManager.PlayerState.command)
        {
            if(!Glitched)
            {
                if(!IsBeingWorkedOn && WorkerManager.Instance.TryDoMaintainWork(this)) //check isbeingmaintain first
                {
                    PlayerManager.Instance.SetState(PlayerManager.PlayerState.normal);
                }
            }
            else
            {
                if(WorkerManager.Instance.TryDoRepairWork(this))
                {
                    PlayerManager.Instance.SetState(PlayerManager.PlayerState.normal);
                }
            }
        }
    }

    public virtual void MaintainSuccess(Worker worker) {}

    public virtual void WorkSuccess(Worker worker)
    {
        RollChanceToRecall(worker);
    }

    public virtual void RollChanceToRecall(Worker worker)
    {
        if(Random.Range(0f, 1f) > 1-HealthPercentage())
        {
            Recall(worker);
        }
    }

    public virtual void Recall(Worker worker) {}

    public virtual void MaintainFail(Worker worker)
    {
        DealDamage(worker);
    }

    public virtual void WorkFail(Worker worker)
    {
        RollChanceToGlitch();
    }

    public virtual void SyncHealthWithPlayback()
    {
        float healthDiff = Health - MaxHealth*( 1 - BasePlayback.GetCorruptedPercentage());
        float errorDiff = ErrorIndex - MaxHealth * ErrorPlayback.GetCorruptedPercentage();
        if(healthDiff != 0)
        {
            DamageHealth(healthDiff);
        }
        if(errorDiff != 0)
        {
            ReduceErrorIndex(errorDiff);
        }
    }

    public virtual void RestorePlayback(float amount)
    {
        PlaybackTL.RemoveCorruptedSegment(amount);
    }

    protected override void OnHealthChanged(float amount)
    {
        base.OnHealthChanged(amount);
    }

    protected virtual void AddCorruptSegment(float amount)
    {
        float point = PlaybackTL.BasePlayback.CurrentPlaybackTimePercentage;
        PlaybackSegment overlappedSegment = PlaybackTL.CheckOverlappedSegment(point);
        if(overlappedSegment != null)
        {
            if(RollChanceToGlitch())
            {
                PlaybackTL.AddErrorSegmentOvertime(amount);
            }
        }
        else
        {
            PlaybackTL.AddCorruptedSegmentOvertime(amount);
        }
    }

    public virtual void Read(bool read)
    {
        IsBeingRead = read;
        if(read)
        {
            ReadCorruptCoroutine = StartCoroutine(ReadCorruptMemory());
        }
        else
        {
            if(ReadCorruptCoroutine != null)
            {
                StopCoroutine(ReadCorruptCoroutine);
            }
        }
    }

    public IEnumerator ReadCorruptMemory()
    {
        while(IsBeingRead)
        {
            float elapsedTime = 0;
            float normalizedInterval = PlaybackTL.NormalizeTime(ReadCorruptInterval);
            while(elapsedTime < normalizedInterval)
            {
                elapsedTime += PlaybackTL.NormalizedDeltaTime;
                yield return null;
            }
            AddCorruptSegment(ReadCorruptAmount);

            if(IsBeingWorkedOn)
            {
                PlaybackTL.StopAddCorruptedSegmentRoutine();
                while(IsBeingWorkedOn)
                {
                    yield return null;
                }
            }
            
            yield return null;
        }
    }

    protected virtual void InFocusBehavior()
    {
        if(InputManager.Instance.Submit.triggered)
        {
            SubmitInteract();
        }
        RollChanceToTalk();
    }

    protected virtual void OutOfFocusBehavior()
    {
        if (DialogueManager.IsDialoguePlaying)
        {
            if (ShutupTimer >= TimeToShutup)
            {
                ShutUp();
                ShutupTimer = 0f;
            }
            else ShutupTimer += Time.deltaTime;
        }
    }
    


    protected override void SharedBehavior()
    {
        base.SharedBehavior();

        if(GlitchEffect != null)
        {
            if(PlaybackTL.BasePlayback.InCorruptedPart())
            {
                GlitchEffect.Show();
                GlitchEffect.SetBlockIntensity(1-HealthPercentage());
                GlitchEffect.SetBlockShuffleRate(1-HealthPercentage());
            }
            else GlitchEffect.Hide();
        }
    }

    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();
        if(PlaybackTL.ErrorPlayback.InCorruptedPart())
        {
            GlitchEffect.SetGlitchIntensity(1-HealthPercentage());
        }
        else
        {
            GlitchEffect.SetGlitchIntensity(0);
        }
    }

    public override void EnterGlitchState()
    {
        base.EnterGlitchState();
        StartCoroutine(Attacking());
    }

    public override void ExitGlitchState()
    {
        base.ExitGlitchState();        
        GlitchEffect.SetGlitchIntensity(0);
    }

    protected virtual IEnumerator Attacking()
    {
        while(IsCombatReady())
        {
            Attack();
            yield return new WaitForSeconds(AttackRate);
        }
    }
    public virtual void Attack()
    {
        if(CombatTargets.Count == 0) return;
        List<ICombatant> removeTargets = new List<ICombatant>();
        foreach(ICombatant target in CombatTargets)
        {
            if(target == null || target.Health <= 0)
            {
                removeTargets.Add(target);
            }
        }
        foreach(ICombatant target in removeTargets)
        {
            CombatTargets.Remove(target);
        }

        if(CombatTargets.Count == 0) return;
        if(DealAOEDamage)
        {
            foreach(ICombatant target in CombatTargets)
            {
                DealDamage(target);
            }
        }
        else
        {
            DealDamage(CombatTargets[0]);
        }
    }

    public virtual bool IsCombatReady()
    {   
        if(Glitched)
        {
            return true;
        }
        return false;
    }

    public virtual void DealDamage(ICombatant target, DamageType damageType = DamageType.Normal)
    {
        target.TakeDamage(AttackDamage, damageType, this);
    }

    public virtual bool TakeDamage(float value, DamageType damageType = DamageType.Normal, ICombatant attacker = null)
    {
        ReduceErrorIndex(value);
        Debug.Log(name + " corruption is " + ErrorIndex);
        return true;
    }
}
