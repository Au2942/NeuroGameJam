using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IStatusEffectable
{
    public static PlayerManager Instance;
    public PlayerViewersHandler ViewersHandler;  
    public PlayerSubscriptionsHandler SubscriptionsHandler;
    public PlayerDetails StatUI;
    public RectTransform StreamRect;
    public StreamEntity StreamEntity;
    public string StreamName;
    public StreamSO CurrentStream;
    public PlayerData PlayerData = new PlayerData();
    public float Health {get => PlayerData.Health; set => PlayerData.Health = value;}
    public float MaxHealth {get => PlayerData.MaxHealth; set => PlayerData.MaxHealth = value;}
    public float StabilityRecoverRate {get => PlayerData.StabilityRecoverRate; set => PlayerData.StabilityRecoverRate = value;}
    public float Performance {get => PlayerData.Performance; set => PlayerData.Performance = value;}
    public float MaxPerformance {get => PlayerData.MaxPerformance; set => PlayerData.MaxPerformance = value;}
    public float RemainingStreamTime {get => PlayerData.RemainingStreamTime; set => PlayerData.RemainingStreamTime = value;}
    public float StreamTimeIncrease {get => PlayerData.StreamTimeIncrease; set => PlayerData.StreamTimeIncrease = value;}
    public float CurrentStreamTimer {get => PlayerData.CurrentStreamTimer; set => PlayerData.CurrentStreamTimer = value;}
    public float ElapsedStreamTime {get => PlayerData.ElapsedStreamTime; set => PlayerData.ElapsedStreamTime = value;}
    public float NewStreamCD {get => PlayerData.NewStreamCD; set => PlayerData.NewStreamCD = value;}
    public float MemoryCorruptionInterval {get => PlayerData.MemoryCorruptionInterval; set => PlayerData.MemoryCorruptionInterval = value;}
    public float MemoryCorruptionDegree {get => PlayerData.MemoryCorruptionDegree; set => PlayerData.MemoryCorruptionDegree = value;}
    public int CurrentViewers {get => PlayerData.CurrentViewers; set => PlayerData.CurrentViewers = value;}
    public int PeakViewers {get => PlayerData.PeakViewers; set => PlayerData.PeakViewers = value;}
    public int Subscriptions {get => PlayerData.Subscriptions; set => PlayerData.Subscriptions = value;}
    public float CurrentInterests {get => PlayerData.CurrentInterests; set => PlayerData.CurrentInterests = value;}
    public float TargetInterests {get => PlayerData.TargetInterests; set => PlayerData.TargetInterests = value;}
    public float MaxInterests {get => PlayerData.MaxInterests; set => PlayerData.MaxInterests = value;}
    public float MinInterests {get => PlayerData.MinInterests; set => PlayerData.MinInterests = value;}
    public float InterestsDropBelowZero {get => PlayerData.InterestsDropBelowZero; set => PlayerData.InterestsDropBelowZero = value;}
    public float InterestsUpdateInterval {get => PlayerData.InterestsUpdateInterval; set => PlayerData.InterestsUpdateInterval = value;}
    public PlayerState State {get => PlayerData.state; set => PlayerData.state = value;}
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();
    List<StatusEffect> IStatusEffectable.StatusEffects { get => StatusEffects; set => StatusEffects = value; }


    public CustomCursor repairCursor;
    public event System.Action<float> OnInterestsChanged;

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
        if(PlayerData == null)
        {
            PlayerData = new PlayerData();
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
        if(StreamEntity.EntityBody != null)
        {
            StreamEntity.EntityBody.gameObject.SetActive(false);
        }
        StartCoroutine(Resetting());
        return true;
    }

    private IEnumerator Resetting()
    {
        while(StreamEntity.ErrorIndex < StreamEntity.MaxErrorIndex)
        {
            yield return null;
            StreamEntity.ReduceErrorIndex(StabilityRecoverRate * Time.deltaTime);
        }
        FinishResetting();
    }

    private IEnumerator CorruptMemory()
    {
        while(true)
        {
            while(State == PlayerState.sleep)
            {
                yield return null;
            }
            yield return new WaitForSeconds(MemoryCorruptionInterval);
            MemoryManager.Instance.CorruptRandomMemory(MemoryCorruptionDegree);
        }
    }

    public void FinishResetting()
    {
        if(StreamEntity.EntityBody != null)
        {
            StreamEntity.EntityBody.gameObject.SetActive(true);
        }
        StreamEntity.ExitGlitchState();
    }

    public bool TryOpenSleepSettingsScreen()
    {
        if(State == PlayerState.sleep)
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
        GameManager.Instance.VignetteController.FadeVignette(0, 1, 1, true);
        StartCoroutine(Sleeping(sleepTime));
    } 

    private IEnumerator Sleeping(float sleepTime)
    {
        yield return new WaitForSeconds(sleepTime);
        TimescaleManager.Instance.ResetTimescale();
        StreamSelector.Instance.OpenUI(true);
        while(StreamSelector.Instance.isOpen)
        {
            yield return null;
        }
        WakeUp();
    }

    public void WakeUp()
    {
        GameManager.Instance.VignetteController.FadeVignette(1, 0, 1, true);
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
                if(State == PlayerState.command)
                {
                    SetState(PlayerState.normal);
                }
                WorkerManager.Instance.DeselectWorker();
            }
        }

    }

    public void SetState(PlayerState newState, bool force = false)
    {
        if(!force && State == newState)
        {
            return;
        }
        State = newState;
        if(State == PlayerState.command)
        {
            CursorManager.Instance.SetCustomCursor(repairCursor);
        }
        else if(State == PlayerState.normal)
        {
            CursorManager.Instance.SetDefaultCursor();
        }
        else if(State == PlayerState.sleep)
        {
            StreamEntity.ShutUp();
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
            while (GameManager.Instance.isPause || State == PlayerState.sleep)
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

    public void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source = null)
    {
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
    }

    public void ChangeStatusEffectStack(StatusEffect statusEffect, int stack)
    {
    }
}
