using UnityEngine;

public abstract class MemoryEntity : Entity
{

    [SerializeField] protected float timeToShutup = 5f;
    protected float shutupTimer = 0f;


    protected override void Update()
    {
        base.Update();

        if(inFocus)
        {
            if(InputManager.Instance.Submit.triggered)
            {
                Interact();
            }
            //random chance to talk
            RollChanceToTalk();
        }
        else if (dialogueManager.IsDialoguePlaying)
        {
            if (shutupTimer >= timeToShutup)
            {
                ShutUp();
                shutupTimer = 0f;
            }
            shutupTimer += Time.deltaTime;
        }

    }

    protected override void Interact()
    {
        base.Interact();
        Converse();
    }

    protected override void OnDayEnd()
    {
        base.OnDayEnd();
        if(glitched) ExitGlitchState();
    }

}
