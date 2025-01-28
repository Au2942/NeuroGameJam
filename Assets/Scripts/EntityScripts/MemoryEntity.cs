using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] public bool IsBeingWorkedOn = false;
    [SerializeField] public bool InFocus = false;
    [SerializeField] GlitchOverlay glitchEffect;
    protected float shutupTimer = 0f;

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
        dialogueManager.PlaySound = focus;
    }

    protected override void ClickInteract(GameObject clickedObject)
    {
        base.ClickInteract(clickedObject);
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.command && !IsBeingWorkedOn )
        {
            if(!Glitched)
            {
                if(WorkerManager.Instance.TryDoMaintainWork(this))
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

    protected override void SharedBehavior()
    {
        base.SharedBehavior();
        
    }

/*     public virtual void OnRepairSuccess(Worker worker)
    {
        AddHealth(worker.Stats.RepairAmount);
        RollChanceToRecall();
    }

    public virtual void OnRepairFail(Worker worker)
    {
        RollChanceToGlitch();
    } */

    public virtual void MaintainSuccess(Worker worker)
    {
        RollChanceToRecall(worker);
    }
    public virtual void RollChanceToRecall(Worker worker)
    {
        if(Random.Range(0, 100) < HealthPercentage())
        {
            Recall(worker);
        }
    }


    public virtual void Recall(Worker worker)
    {

    }

    public virtual void MaintainFail(Worker worker)
    {
        OnRepairFailEffect(worker);
        RollChanceToGlitch();
    }

    public virtual void OnRepairFailEffect(Worker worker)
    {

    }

    protected override void OnHealthChanged()
    {
        base.OnHealthChanged();
        if(glitchEffect != null)
        {
            if(HealthPercentage() <= glitchRollThreshold)
            {
                glitchEffect.Show();
                float t = 1 - (Health / MaxHealth);
                float easedT = t*t*t*t;
                glitchEffect.SetGlitchIntensity(easedT);
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



    protected override void OnEndStream()
    {
        base.OnEndStream();
        if(Glitched) ExitGlitchState();
    }

    public override void EnterGlitchState()
    {
        base.EnterGlitchState();
    }

    public override void ExitGlitchState()
    {
        base.ExitGlitchState();

    }

}
