
public class StreamEntity : Entity
{
    protected override void Start()
    {
        base.Start();
        Health = PlayerManager.Instance.Health;
        MaxHealth = PlayerManager.Instance.MaxHealth;
    }
    protected override void Update()
    {
        base.Update();
        Health = PlayerManager.Instance.Health;
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

    protected override void CorruptBehavior()
    {
        base.CorruptBehavior();

    }

}