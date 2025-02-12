using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MemoryEntity : Entity, ICombatant
{
    [SerializeField] protected MemoryEntityData memoryEntityData;
    public MemoryBlock MemoryBlock {get => memoryEntityData.MemoryBlock; set => memoryEntityData.MemoryBlock = value;}
    protected GlitchOverlay GlitchEffect {get => memoryEntityData.GlitchEffect; set => memoryEntityData.GlitchEffect = value;}
    public Playback Playback {get => memoryEntityData.Playback; set => memoryEntityData.Playback = value;}
    public float AttackDamage {get => memoryEntityData.AttackDamage; set => memoryEntityData.AttackDamage = value;}
    public float AttackRate {get => memoryEntityData.AttackRate; set => memoryEntityData.AttackRate = value;}
    protected float TimeToShutup {get => memoryEntityData.TimeToShutup; set => memoryEntityData.TimeToShutup = value;}
    public bool DealAOEDamage {get => memoryEntityData.DealAOEDamage; set => memoryEntityData.DealAOEDamage = value;}
    public bool InFocus {get => memoryEntityData.InFocus; set => memoryEntityData.InFocus = value;}
    public bool IsBeingMaintained {get => memoryEntityData.IsBeingMaintained; set => memoryEntityData.IsBeingMaintained = value;}
    protected float ShutupTimer {get => memoryEntityData.shutupTimer; set => memoryEntityData.shutupTimer = value;}
    public List<ICombatant> CombatTargets {get => memoryEntityData.combatTargets; set => memoryEntityData.combatTargets = value;}
    float ICombatant.Health { get => ErrorIndex; set => ErrorIndex = value; }
    float ICombatant.MaxHealth { get => MaxErrorIndex; set => MaxErrorIndex = value; }


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
        if(IsBeingMaintained) return;
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
                if(!IsBeingMaintained && WorkerManager.Instance.TryDoMaintainWork(this)) //check isbeingmaintain first
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

    protected override void OnHealthChanged(float amount)
    {
        base.OnHealthChanged(amount);
        if(amount < 0)
        {
            AddCorruptSegment(-amount/MaxHealth);
        }
        else
        {
            RemoveCorruptSegment(Playback.CurrentPlaybackTime, amount);
        }

        if(GlitchEffect != null)
        {
            if(HealthPercentage() <= GlitchRollThreshold)
            {
                GlitchEffect.Show();
                GlitchEffect.SetBlockIntensity(1-HealthPercentage());
                GlitchEffect.SetBlockShuffleRate(1-HealthPercentage());
            }
            else GlitchEffect.Hide();
        }
    }

    protected virtual void AddCorruptSegment(float percentage)
    {
        Playback.AddRandomCorruptPart(percentage*Playback.PlaybackDuration);
    }

    protected virtual void RemoveCorruptSegment(float start, float duration)
    {
        Playback.RemoveCorruptPart(start, start + duration);
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

    public override void EnterGlitchState()
    {
        base.EnterGlitchState();
        GlitchEffect.Show();
        GlitchEffect.SetGlitchIntensity(1-HealthPercentage());
        if(InFocus)
        {
            GameManager.Instance.ScreenEffectController.Show();
        }
        StartCoroutine(Attacking());
    }

    public override void ExitGlitchState()
    {
        base.ExitGlitchState();
        GlitchEffect.SetGlitchIntensity(0);
        if(InFocus)
        {
            GameManager.Instance.ScreenEffectController.Hide();
        }
        //CombatManager.Instance.EndCombat(this);
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
        IncreaseErrorIndex(value);
        Debug.Log(name + " corruption is " + ErrorIndex);
        return true;
    }
}
