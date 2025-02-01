using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public PlayerViewersHandler ViewersHandler;  
    public PlayerSubscriptionsHandler SubscriptionsHandler;
    public PlayerDetailsUI StatUI;
    public RectTransform StreamRect;
    public StreamEntity StreamEntity;
    public string StreamName;
    public StreamSO CurrentStream;
    public float Health = 100; //integrity of self
    public float MaxHealth = 100;
    public float StabilityRecoverRate = 5f; //5 stability per second
    public float Performance = 0.1f; //average of all memory entities integrity
    public float MaxPerformance = 2f;
    public float RemainingStreamTime = 60f; //~1 hour
    public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
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

    public PlayerState state = PlayerState.normal;
    public CustomCursor repairCursor;
    //public event System.Action<float> OnInterestsChanged;
    public float CurrentStreamTimer {get; set;} = 0f;

    public event System.Action<float> OnTakeDamageEvent;
    public event System.Action<float> OnHealthChangedEvent;
    public event System.Action<StreamEntity> OnNewStreamEvent;

    public event System.Action<string> OnChangeStreamNameEvent;


    public enum PlayerState
    {
        normal,
        command,
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
        StartCoroutine(CorruptMemory());
        StartCoroutine(UpdateInterests());
        StartCoroutine(ViewersHandler.SimulateViewers());
        StartCoroutine(ViewersHandler.SimulateViewersMovement());
        StartCoroutine(ViewersHandler.SimulateViewersNoise());
        StartCoroutine(SubscriptionsHandler.SimulateSingleSubscription());
        StartCoroutine(SubscriptionsHandler.SimulateMassSubscription());
    }

    public GameObject SetUpStream(StreamSO newStream)
    {
        if(StreamEntity != null)
        {
            Destroy(StreamEntity.gameObject);
        }

        StreamEntity stream = Instantiate(newStream.stream, StreamRect);
        SetStreamName("Subathon " + newStream.streamName + " stream");
        //GameManager.Instance.MemoryData.AddMemory("Subathon " + newStream.streamName + " stream", stream);
        StreamEntity = stream;
        SetPlayerStream(newStream);

        OnNewStreamEvent?.Invoke(StreamEntity);
        
        return StreamEntity.gameObject;
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
        OnChangeStreamNameEvent?.Invoke(name);
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
        while(StreamEntity.Corruption < StreamEntity.MaxCorruption)
        {
            StreamEntity.RestoreCorruption(StabilityRecoverRate * Time.deltaTime);
            yield return null;
        }
        FinishResetting();
    }

    private IEnumerator CorruptMemory()
    {
        while(true)
        {
            while(state == PlayerState.sleep)
            {
                yield return null;
            }
            yield return new WaitForSeconds(MemoryCorruptionInterval);
            MemoryManager.Instance.CorruptRandomMemory(MemoryCorruptionDegree);
        }
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
        if(WorkerManager.Instance.SelectedWorker != null)
        {
            if(InputManager.Instance.Cancel.triggered || InputManager.Instance.RightClick.triggered)
            {
                if(state == PlayerState.command)
                {
                    SetState(PlayerState.normal);
                }
                WorkerManager.Instance.DeselectWorker();
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
        if(state == PlayerState.command)
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


    public void DamageHealth(float value)
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
            //StartCoroutine(EndingManager.Instance.EndGame(0));
        } 

        OnTakeDamageEvent?.Invoke(value);
        OnHealthChangedEvent?.Invoke(Health);
    }
    public void HealHealth(float value)
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
        OnHealthChangedEvent?.Invoke(Health);
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
