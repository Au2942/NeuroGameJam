using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    protected float shutupTimer = 0f;

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

    protected override void Interact()
    {
        base.Interact();
        Converse();
    }

    protected override void OnEndStream()
    {
        base.OnEndStream();
        if(glitched) ExitGlitchState();
    }

}
