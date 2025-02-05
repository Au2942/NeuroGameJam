[System.Serializable]
public class PlayerData
{
    public float Health = 100; //integrity of self
    public float MaxHealth = 100;
    public float StabilityRecoverRate = 5f; //5 stability per second
    public float Performance = 0.1f; //average of all memory entities integrity
    public float MaxPerformance = 2f;
    public float RemainingStreamTime = 60f; //~1 hour
    public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
    public float ElapsedStreamTime = 0f;
    public float CurrentStreamTimer = 0f;
    public float NewStreamCD = 60f; //60 seconds before you can start a new stream
    public float MemoryCorruptionInterval = 5f; //seconds before lower a memory health
    public float MemoryCorruptionDegree = 10f; //5 health per corruption
    public int CurrentViewers = 0; //ccv 
    public int PeakViewers = 0;
    public int Subscriptions = 0;
    public float CurrentInterests = 0.5f;
    public float TargetInterests = -999;
    public float MaxInterests = 2f;
    public float MinInterests = -2f;
    public float InterestsDropBelowZero = 0.1f;
    public float InterestsUpdateInterval = 5f; 
    public PlayerManager.PlayerState state = PlayerManager.PlayerState.normal;

}