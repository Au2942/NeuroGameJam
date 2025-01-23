using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] GlitchOverlay glitchEffect;
    protected float shutupTimer = 0f;

    protected override void Update()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.sleep) return;
        if(IsBeingRepaired) return;
        base.Update();
    }

    protected override void ClickInteract(GameObject clickedObject)
    {
        base.ClickInteract(clickedObject);
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair && !IsBeingRepaired && !Glitched)
        {
            if(WorkerManager.Instance.TryUseRepairWorker(this))
            {
                PlayerManager.Instance.SetState(PlayerManager.PlayerState.normal);
            }
        }
    }

    protected override void SharedBehavior()
    {
        base.SharedBehavior();
        
    }

    public virtual void Recall()
    {
        //when work success
    }

    protected override void OnIntegrityChanged()
    {
        base.OnIntegrityChanged();
        if(glitchEffect != null)
        {
            if(Health/MaxHealth <= glitchRollThreshold)
            {
                glitchEffect.Show();
                float t = 1 - (Health / MaxHealth);
                float easedT = t*t*t*t;
                glitchEffect.SetGlitchIntensity(easedT);
            }
            else glitchEffect.Hide();
        }
    }

    protected override void InFocusBehavior()
    {
        base.InFocusBehavior();
        RollChanceToTalk();
    }

    protected override void OutOfFocusBehavior()
    {
        base.OutOfFocusBehavior();
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

    protected override void SubmitInteract()
    {
        base.SubmitInteract();
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
