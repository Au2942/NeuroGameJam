using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] public PlayerViewersHandler ViewersHandler;  
    [SerializeField] public PlayerSubscriptionsHandler SubscriptionsHandler;
    [SerializeField] public PlayerStatsUI StatUI;
    [SerializeField] public RectTransform StreamRect;
    [SerializeField] public StreamEntity StreamEntity;
    [SerializeField] public string StreamName;
    [SerializeField] public StreamSO CurrentStream;
    [SerializeField] public float Health = 100; //integrity of self
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Performance = 0.1f; //average of all memory entities integrity
    [SerializeField] public float MaxPerformance = 2f;
    [SerializeField] public float ResetTime = 5f;
    [SerializeField] public float RemainingStreamTime = 60f; //~1 hour
    [SerializeField] public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
    [SerializeField] public float NewStreamCD = 60f; //60 seconds before you can start a new stream
    [SerializeField] public int CurrentViewers = 0; //ccv 
    [SerializeField] public int PeakViewers = 0;
    [SerializeField] public int Subscriptions = 0;
    [SerializeField] public float CurrentInterests = 0.5f;
    [SerializeField] public float TargetInterests = -999;
    [SerializeField] public float MaxInterests = 2f;
    [SerializeField] public float MinInterests = -2f;
    [SerializeField] public float InterestsDropBelowZero = 0.1f;
    [SerializeField] public float InterestsUpdateInterval = 5f; 

    [SerializeField] public PlayerState state = PlayerState.normal;
    [SerializeField] public CustomCursor repairCursor;
    //public event System.Action<float> OnInterestsChanged;
    public float CurrentStreamTimer {get; set;} = 0f;

    public event System.Action<float> OnTakeDamage;
    public event System.Action<StreamEntity> OnNewStream;

    public event System.Action<string> OnChangeStreamName;


    public enum PlayerState
    {
        normal,
        repair,
        sleep
    }
    public float ElapsedStreamTime {get; set;} = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {

        StartCoroutine(UpdateInterests());
        StartCoroutine(ViewersHandler.SimulateViewers());
        StartCoroutine(ViewersHandler.SimulateViewersMovement());
        StartCoroutine(ViewersHandler.SimulateViewersNoise());
        StartCoroutine(SubscriptionsHandler.SimulateSingleSubscription());
        StartCoroutine(SubscriptionsHandler.SimulateMassSubscription());
        
    }

    public GameObject SetUpStream(StreamSO newStream)
    {
        if(StreamRect.transform.childCount > 1)
        {
            Destroy(StreamRect.transform.GetChild(0).gameObject);
        }

        StreamEntity stream = Instantiate(newStream.stream, StreamRect);
        SetStreamName("Subathon " + newStream.streamName + " stream");
        //GameManager.Instance.MemoryData.AddMemory("Subathon " + newStream.streamName + " stream", stream);

        
        SetPlayerStream(newStream);

        OnNewStream?.Invoke(stream);
        
        return stream.gameObject;
    }

    private void SetPlayerStream(StreamSO stream)
    {
        TargetInterests = CurrentInterests + stream.interestsPotential;
        CurrentInterests += stream.impactInterests;
        CurrentStreamTimer = 0;
        CurrentStream = stream;
    }

    public void SetStreamName(string name)
    {
        StreamName = name;
        OnChangeStreamName?.Invoke(name);
    }

    public void ProgressStream()
    {
        CurrentStreamTimer += Time.deltaTime;
        ElapsedStreamTime += Time.deltaTime;
        RemainingStreamTime -= Time.deltaTime;
        PeakViewers = Mathf.Max(PeakViewers, CurrentViewers);
    }

    public bool TryReset()
    {
        if(!StreamEntity.Glitched)
        {
            return false;
        }
        if(StreamEntity.Body != null)
        {
            StreamEntity.Body.SetActive(false);
        }
        StartCoroutine(Resetting());
        return true;
    }

    private IEnumerator Resetting()
    {
        yield return new WaitForSeconds(ResetTime);
        FinishResetting();
    }

    public void FinishResetting()
    {
        if(StreamEntity.Body != null)
        {
            StreamEntity.Body.SetActive(true);
        }
        StreamEntity.ExitGlitchState();
    }

    public bool TryOpenSleepSettingsScreen()
    {
        if(state == PlayerState.sleep)
        {
            return false;
        }
        SleepSettingsScreen.Instance.OpenUI();
        return true;
    }

    public void Sleep(float sleepTime)
    {
        if(StreamEntity != null) StreamEntity.EnterSleepState();
        CurrentInterests = -0.9f; // expected viewers -> 10% of baseline viewers
        SetState(PlayerState.sleep);
        SetStreamName("Snooze Stream");
        GameManager.Instance.StopStream();
        TimescaleManager.Instance.SetTimescale(50);
        StartCoroutine(Sleeping(sleepTime));
    }

    private IEnumerator Sleeping(float sleepTime)
    {
        yield return new WaitForSeconds(sleepTime);
        WakeUp();
    }

    public void WakeUp()
    {
        TimescaleManager.Instance.ResetTimescale();
        StreamSelector.Instance.OpenUI(true);
        SetState(PlayerState.normal);
        CurrentInterests = 0; // reset interests
        if(StreamEntity != null) StreamEntity.ExitSleepState();
    }

    public bool TryOpenStreamSelector()
    {
        if(CurrentStreamTimer < NewStreamCD)
        {
            //show message
            return false;
        }
        StreamSelector.Instance.OpenUI();
        return true;
    }

    public void CheckPlayerInput()
    {
        if(WorkerManager.Instance.selectedRepairWorker != null)
        {
            if(InputManager.Instance.Cancel.triggered || InputManager.Instance.RightClick.triggered)
            {
                if(state == PlayerState.repair)
                {
                    SetState(PlayerState.normal);
                }
                WorkerManager.Instance.DeselectRepairWorker();
            }
        }

    }

    public void SetState(PlayerState newState, bool force = false)
    {
        if(!force && state == newState)
        {
            return;
        }
        state = newState;
        if(state == PlayerState.repair)
        {
            CursorManager.Instance.SetCustomCursor(repairCursor);
        }
        else if(state == PlayerState.normal)
        {
            CursorManager.Instance.SetDefaultCursor();
        }
        else if(state == PlayerState.sleep)
        {
            
        }
    }

    public void IncreaseStreamTime(int multiplier)
    {
        RemainingStreamTime += StreamTimeIncrease*multiplier;
    }

    public void AddViewers(int value)
    {
        CurrentViewers += value;
    }


    public void TakeDamage(float value)
    {
        if(value < 0)
        {
            return;
        }

        Health -= value;
        
        if(Health < 0)
        {
            //to-do game over
            Health = 0;
            StartCoroutine(EndingManager.Instance.EndGame(0));
        } //should move this somewhere else and trigger it by listening to the OnTakeDamage event instead maybe?

        OnTakeDamage?.Invoke(value);
    }
    public void Heal(float value)
    {
        if(value < 0)
        {
            return;
        }

        Health += value;
        if(Health > MaxHealth)
        {
            Health = MaxHealth;
        }
        
    }

    public IEnumerator UpdateInterests()
    {
        while (true)
        {
            while (GameManager.Instance.isPause || state == PlayerState.sleep)
            {
                yield return null;
            }

            if (TargetInterests != -999)
            {
                CurrentInterests += CurrentStream.interestsGain;

                if (CurrentInterests >= TargetInterests)
                {
                    CurrentInterests = TargetInterests;
                    yield return new WaitForSeconds(CurrentStream.interestsPeakDuration);
                    TargetInterests = -999;
                }
            }
            else
            {
                if (CurrentInterests > 0)
                {
                    CurrentInterests = Mathf.Max(CurrentInterests - CurrentStream.interestsDrop, 0);
                }
                else
                {
                    CurrentInterests -= InterestsDropBelowZero;
                }
            }

            CurrentInterests = Mathf.Clamp(CurrentInterests, MinInterests, MaxInterests);

            yield return new WaitForSeconds(InterestsUpdateInterval);
        }
    }


}
