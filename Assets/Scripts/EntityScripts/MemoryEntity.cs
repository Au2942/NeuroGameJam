using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    [SerializeField] GlitchOverlay glitchEffect;
    protected float shutupTimer = 0f;

    protected override void SharedBehavior()
    {
        base.SharedBehavior();
        
        glitchEffect.SetGlitchIntensity(1 - Integrity/MaxIntegrity);
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
        Converse();
    }

    protected override void OnEndStream()
    {
        base.OnEndStream();
        if(corrupted) ExitCorruptState();
    }

    public override void EnterCorruptState()
    {
        base.EnterCorruptState();
        glitchEffect.FlickerAndScanline(true);
    }

    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        glitchEffect.FlickerAndScanline(false);

    }

}
