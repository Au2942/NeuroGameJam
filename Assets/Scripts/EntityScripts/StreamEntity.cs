using UnityEngine;

public class StreamEntity : Entity
{
    protected override void Start()
    {
        base.Start();
        Integrity = PlayerManager.Instance.GetPerformance();
        MaxIntegrity = PlayerManager.Instance.MaxIntegrity + PlayerManager.Instance.MaxMemoriesIntegrity;
    }
    protected override void Update()
    {
        base.Update();
        Integrity = PlayerManager.Instance.GetPerformance();
    }

    protected override void SubmitInteract()
    {
        base.SubmitInteract();
        StreamSelector.Instance.OpenUI();
    } 
    public override void EnterCorruptState()
    {
        base.EnterCorruptState();
        Interactable = false;
    }
    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        Interactable = true;
    }

}