using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] public PlayerViewersHandler ViewersHandler;  
    [SerializeField] public PlayerSubscriptionsHandler SubscriptionsHandler;
    [SerializeField] public PlayerStatUI StatUI;

    [SerializeField] public float Health = 100; //integrity of self
    [SerializeField] public float MaxHealth = 100;
    [SerializeField] public float Performance = 0.1f; //average of all memory entities integrity
    [SerializeField] public float MaxPerformance = 2f;
    [SerializeField] public float RemainingStreamTime = 60f;
    [SerializeField] public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
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






    public enum PlayerState
    {
        normal,
        repair,
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
        ElapsedStreamTime += Time.deltaTime;
        RemainingStreamTime -= Time.deltaTime;
        PeakViewers = Mathf.Max(PeakViewers, CurrentViewers);
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
    }

    public void IncreaseStreamTime(int multiplier)
    {
        RemainingStreamTime += StreamTimeIncrease*multiplier;
    }

    public void AddViewers(int value)
    {
        CurrentViewers += value;
    }


    public void TakeDamage(int value)
    {
        Health -= value;
        if(Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        if(Health < 0)
        {
            //to-do game over
            Health = 0;
            StartCoroutine(EndingManager.Instance.EndGame(0));
        }
    }

    public IEnumerator UpdateHype()
    {
        while (true)
        {
            while (!GameManager.Instance.isStreaming)
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
