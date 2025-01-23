using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] public PlayerViewersHandler ViewersHandler;  
    [SerializeField] public PlayerSubscriptionsHandler SubscriptionsHandler;
    [SerializeField] public PlayerStatsUI StatUI;

    [SerializeField] public float Health = 100; //integrity of self
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Performance = 0.1f; //average of all memory entities integrity
    [SerializeField] public float MaxPerformance = 2f;
    [SerializeField] public float ResetTime = 5f;
    [SerializeField] public float SleepTime = 480f; //should be 2 hour per stats u wanna raise for swarmbot / swarmbot level 
    [SerializeField] public float RemainingStreamTime = 60f; //~1 hour
    [SerializeField] public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
    [SerializeField] public float NewStreamCD = 60f; //60 seconds before you can start a new stream
    [SerializeField] public int CurrentViewers = 0; //ccv 
    [SerializeField] public int PeakViewers = 0;
    [SerializeField] public int Subscriptions = 0;
    [SerializeField] public float CurrentHype = 0.5f;
    [SerializeField] public float TargetHype = -999;
    [SerializeField] public float MaxHype = 2f;
    [SerializeField] public float MinHype = -2f;
    [SerializeField] public float HypeDropBelowZero = 0.1f;
    [SerializeField] public float HypeUpdateInterval = 5f; 

    [SerializeField] public PlayerState state = PlayerState.normal;
    [SerializeField] public CustomCursor repairCursor;
    //public event System.Action<float> OnHypeChanged;
    private StreamSO currentStream => GameManager.Instance.CurrentStream;
    private StreamEntity currentStreamEntity => GameManager.Instance.ChannelData.GetStreamEntity();
    public float CurrentStreamTimer {get; set;} = 0f;

    public event System.Action<float> OnTakeDamage;


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

        StartCoroutine(UpdateHype());
        StartCoroutine(ViewersHandler.SimulateViewers());
        StartCoroutine(ViewersHandler.SimulateViewersMovement());
        StartCoroutine(ViewersHandler.SimulateViewersNoise());
        StartCoroutine(SubscriptionsHandler.SimulateSingleSubscription());
        StartCoroutine(SubscriptionsHandler.SimulateMassSubscription());
        
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
        if(!currentStreamEntity.Glitched)
        {
            return false;
        }
        if(currentStreamEntity.Body != null)
        {
            currentStreamEntity.Body.SetActive(false);
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
        if(currentStreamEntity.Body != null)
        {
            currentStreamEntity.Body.SetActive(true);
        }
        currentStreamEntity.ExitGlitchState();
    }

    public bool TrySleep()
    {
        if(state == PlayerState.sleep)
        {
            return false;
        }
        if(currentStreamEntity != null) currentStreamEntity.EnterSleepState();
        CurrentHype = -0.9f; // expected viewers -> 10% of baseline viewers
        SetState(PlayerState.sleep);
        GameManager.Instance.StopStream();
        TimescaleManager.Instance.SetTimescale(50); // 8 times faster
        StartCoroutine(Sleeping());
        return true;
    }

    private IEnumerator Sleeping()
    {
        yield return new WaitForSeconds(SleepTime);
        WakeUp();
    }

    public void WakeUp()
    {
        TimescaleManager.Instance.ResetTimescale();
        StreamSelector.Instance.OpenUI(true);
        SetState(PlayerState.normal);
        CurrentHype = 0; // reset hype
        if(currentStreamEntity != null) currentStreamEntity.ExitSleepState();
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
        if(state == PlayerState.repair)
        {
            if(InputManager.Instance.Cancel.triggered || InputManager.Instance.RightClick.triggered)
            {
                SetState(PlayerState.normal);
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

    public IEnumerator UpdateHype()
    {
        while (true)
        {
            while (GameManager.Instance.isPause || state == PlayerState.sleep)
            {
                yield return null;
            }

            if (TargetHype != -999)
            {
                CurrentHype += currentStream.hypeGain;

                if (CurrentHype >= TargetHype)
                {
                    CurrentHype = TargetHype;
                    yield return new WaitForSeconds(currentStream.hypePeakDuration);
                    TargetHype = -999;
                }
            }
            else
            {
                if (CurrentHype > 0)
                {
                    CurrentHype = Mathf.Max(CurrentHype - currentStream.hypeDrop, 0);
                }
                else
                {
                    CurrentHype -= HypeDropBelowZero;
                }
            }

            CurrentHype = Mathf.Clamp(CurrentHype, MinHype, MaxHype);

            yield return new WaitForSeconds(HypeUpdateInterval);
        }
    }


}
