using System.Collections.Generic;
using UnityEngine;

public abstract class MemoryEntity : Entity, ICombatUnit
{
    [SerializeField] protected float AttackDamage = 1f;
    [SerializeField] protected float AttackRate = 1f;
    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] public bool IsBeingMaintained = false;
    [SerializeField] public bool InFocus = false;
    [SerializeField] public bool dealAOEDamage = false;
    [SerializeField] GlitchOverlay glitchEffect;

    protected List<ICombatUnit> combatTargets = new List<ICombatUnit>();

    protected float shutupTimer = 0f;

    float ICombatUnit.Health { get => Corruption; set => Corruption = value; }
    float ICombatUnit.MaxHealth { get => MaxCorruption; set => MaxCorruption = value; }
    float ICombatUnit.AttackDamage { get => AttackDamage; set => AttackDamage = value; }
    float ICombatUnit.AttackRate { get => AttackRate; set => AttackRate = value; }
    List<ICombatUnit> ICombatUnit.CombatTargets { get => combatTargets; set => combatTargets = value; }

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
        dialogueManager.PlaySound = focus;
    }

    protected override void ClickInteract(GameObject clickedObject)
    {
        base.ClickInteract(clickedObject);
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.command )
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
        OnMaintainFail(worker);
        RollChanceToGlitch();
    }

    public virtual void OnMaintainFail(Worker worker)
    {

    }

    protected override void OnHealthChanged(float amount)
    {
        base.OnHealthChanged(amount);
        if(glitchEffect != null)
        {
            if(HealthPercentage() <= glitchRollThreshold)
            {
                glitchEffect.Show();
                glitchEffect.SetBlockIntensity(1-HealthPercentage());
                glitchEffect.SetBlockShuffleRate(1-HealthPercentage());
            }
            else glitchEffect.Hide();
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
        if (dialogueManager.IsDialoguePlaying)
        {
            if (shutupTimer >= timeToShutup)
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
        glitchEffect.Show();
        glitchEffect.SetGlitchIntensity(1-HealthPercentage());
        if(InFocus)
        {
            GameManager.Instance.ScreenEffectController.Show();
        }
        CombatManager.Instance.StartCombat(this);
    }

    public override void ExitGlitchState()
    {
        base.ExitGlitchState();
        glitchEffect.SetGlitchIntensity(0);
        if(InFocus)
        {
            GameManager.Instance.ScreenEffectController.Hide();
        }
        //CombatManager.Instance.EndCombat(this);
    }

    public virtual void Attack()
    {
        if(combatTargets.Count == 0) return;
        if(dealAOEDamage)
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
