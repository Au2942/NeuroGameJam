using UnityEngine;

public class StreamEntity : Entity
{
    protected override void Interact()
    {
        base.Interact();
        StreamSelector.Instance.OpenUI();
    }
}