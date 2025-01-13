using UnityEngine;

public class StreamEntity : Entity
{
    protected override void Interact()
    {
        base.Interact();
        StreamSelector.Instance.OpenUI();
    }

    protected override void NormalBehavior()
    {
        base.NormalBehavior();
        RollChanceToTalk();
    }
}