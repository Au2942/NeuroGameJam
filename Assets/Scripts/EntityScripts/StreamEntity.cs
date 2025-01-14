using UnityEngine;

public class StreamEntity : Entity
{
    protected override void Start()
    {
        base.Start();
        Integrity = PlayerManager.Instance.Integrity;
        MaxIntegrity = PlayerManager.Instance.MaxIntegrity;
    }
    protected override void Update()
    {
        base.Update();
        Integrity = PlayerManager.Instance.Integrity;
    }
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