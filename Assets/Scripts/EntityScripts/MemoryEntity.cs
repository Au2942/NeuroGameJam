using System.Collections.Generic;
using UnityEngine;

public abstract class MemoryEntity : Entity, ICombatUnit
{
    [SerializeField] protected MemoryEntityData memoryEntityData;
    protected GlitchOverlay GlitchEffect {get => memoryEntityData.GlitchEffect; set => memoryEntityData.GlitchEffect = value;}
    protected float AttackDamage {get => memoryEntityData.AttackDamage; set => memoryEntityData.AttackDamage = value;}
    protected float AttackRate {get => memoryEntityData.AttackRate; set => memoryEntityData.AttackRate = value;}
    protected float TimeToShutup {get => memoryEntityData.TimeToShutup; set => memoryEntityData.TimeToShutup = value;}
    public bool IsBeingMaintained {get => memoryEntityData.IsBeingMaintained; set => memoryEntityData.IsBeingMaintained = value;}
    public bool InFocus {get => memoryEntityData.InFocus; set => memoryEntityData.InFocus = value;}
    public bool DealAOEDamage {get => memoryEntityData.DealAOEDamage; set => memoryEntityData.DealAOEDamage = value;}

    protected List<ICombatUnit> combatTargets = new List<ICombatUnit>();
    protected float shutupTimer = 0f;

    float ICombatUnit.Health { get => Corruption; set => Corruption = value; }
    float ICombatUnit.MaxHealth { get => MaxCorruption; set => MaxCorruption = value; }
    float ICombatUnit.AttackDamage { get => AttackDamage; set => AttackDamage = value; }
    float ICombatUnit.AttackRate { get => AttackRate; set => AttackRate = value; }
    List<ICombatUnit> ICombatUnit.CombatTargets { get => combatTargets; set => combatTargets = value; }

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

    public virtual void AddCombatTarget(ICombatUnit target)
    {
        combatTargets.Add(target);
    }

    public virtual void RemoveCombatTarget(ICombatUnit target)
    {
        combatTargets.Remove(target);
    }

    protected virtual void SubmitInteract()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.command)
        {
            return;
        }
        if(!Interactable) 
        {
            return;
        }
    }

    protected override void Update()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.sleep) return;
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
        DialogueManager.PlaySound = focus;
    }

    protected override void ClickInteract(GameObject clickedObject)
    {
        base.ClickInteract(clickedObject);
        if(InFocus && PlayerManager.Instance.state == PlayerManager.PlayerState.command)
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

    public virtual void MaintainSuccess(Worker worker)
    {
        RestoreHealth(worker.TotalStats.WorkAmount);
        RollChanceToRecall(worker);
    }
    public virtual void RollChanceToRecall(Worker worker)
    {
        if(Random.Range(0f, 1f) > 1-HealthPercentage())
        {
            Recall(worker);
        }
    }


    public virtual void Recall(Worker worker)
    {

    }

    public virtual void MaintainFail(Worker worker)
    {
        RestoreHealth(worker.TotalStats.WorkAmount/2);
        OnMaintainFail(worker);
        RollChanceToGlitch();
    }

    public virtual void OnMaintainFail(Worker worker)
    {
        DealDamage(worker);
    }

    protected override void OnHealthChanged(float amount)
    {
        base.OnHealthChanged(amount);
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
            if (shutupTimer >= TimeToShutup)
            {
                ShutUp();
                shutupTimer = 0f;
            }
            else shutupTimer += Time.deltaTime;
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
        CombatManager.Instance.StartCombat(this);
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

    public virtual void Attack()
    {
        if(combatTargets.Count == 0) return;
        if(DealAOEDamage)
        {
            foreach(ICombatUnit target in combatTargets)
            {
                DealDamage(target);
            }
        }
        else
        {
            DealDamage(combatTargets[0]);
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

    public virtual void DealDamage(ICombatUnit target)
    {
        target.TakeDamage(AttackDamage, this);
    }

    public virtual void TakeDamage(float value, ICombatUnit attacker)
    {
        DamageCorruption(value);
        Debug.Log(name + " corruption is " + Corruption);
    }
}
