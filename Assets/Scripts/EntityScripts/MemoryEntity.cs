using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    protected float shutupTimer = 0f;

    protected GameObject glitchEffect;

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
        glitchEffect = OverlayEffectManager.Instance.AddOverlayEffect(transform);
    }

    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        OverlayEffectManager.Instance.RemoveOverlayEffect(glitchEffect);
    }

}
