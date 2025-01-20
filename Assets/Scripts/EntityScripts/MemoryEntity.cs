using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] GlitchOverlay glitchEffect;
    protected float shutupTimer = 0f;

    protected override void SharedBehavior()
    {
        base.SharedBehavior();
        
    }

    protected override void OnIntegrityChanged()
    {
        base.OnIntegrityChanged();
        if(glitchEffect != null)
        {
            if(Health/MaxHealth <= 0.7f)
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
        if(corrupted) ExitCorruptState();
    }

    public override void EnterCorruptState()
    {
        base.EnterCorruptState();
    }

    public override void ExitCorruptState()
    {
        base.ExitCorruptState();

    }

}
