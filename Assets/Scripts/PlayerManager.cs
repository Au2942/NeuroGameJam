using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [SerializeField] public float Integrity = 100; //integrity of self
    [SerializeField] public float MaxIntegrity = 100;
    [SerializeField] public float MemoriesIntegrity = 100; //average of all memory entities integrity
    [SerializeField] public float MaxMemoriesIntegrity = 100;
    [SerializeField] public float RemainingStreamTime = 60f;
    [SerializeField] public float StreamTimeIncrease = 0.03f; //3 seconds per subscription
    [SerializeField] public int CurrentViewers = 0; //ccv 
    [SerializeField] public int PeakViewers = 0;
    [SerializeField] public int Subscriptions = 0;
    [SerializeField] public float CurrentHype = 2;
    [SerializeField] public float TargetHype = -999;
    [SerializeField] public float HypeChangeAmount = 0.1f;
    [SerializeField] public float HypeUpdateInterval = 10f; 

    [SerializeField] public PlayerViewersHandler viewersHandler;  

    [SerializeField] public PlayerSubscriptionsHandler subscriptionsHandler;
    public event System.Action<float> OnHypeChanged;

    public float bonus = 0f;

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
        StartCoroutine(viewersHandler.SimulateViewers());
        StartCoroutine(viewersHandler.SimulateViewersMovement());
        StartCoroutine(viewersHandler.SimulateViewersNoise());
        StartCoroutine(subscriptionsHandler.SimulateSingleSubscription());
        StartCoroutine(subscriptionsHandler.SimulateMassSubscription());
    }

    public void ProgressStream()
    {
        ElapsedStreamTime += Time.deltaTime;
        RemainingStreamTime -= Time.deltaTime;
        PeakViewers = Mathf.Max(PeakViewers, CurrentViewers);
        UpdateMemoryIntegrity();
    }

    private void UpdateMemoryIntegrity()
    {
        if(GameManager.Instance.Entities.Count <= 1)
        {
            MemoriesIntegrity = MaxMemoriesIntegrity;
            return;
        }
        float totalIntegrity = 0;
        float totalMaxIntegrity = 0;
        for(int i = 1; i < GameManager.Instance.Entities.Count; i++)
        {
            totalIntegrity += GameManager.Instance.Entities[i].Integrity;
            totalMaxIntegrity += GameManager.Instance.Entities[i].MaxIntegrity;
        }
        MemoriesIntegrity = totalIntegrity / totalMaxIntegrity * MaxMemoriesIntegrity;
    }

    public void UpdateBonus()
    {
        bonus = GetPerformance() + CurrentHype;
    }

    public float GetPerformance()
    {
        return (MemoriesIntegrity + Integrity) / (MaxMemoriesIntegrity + MaxIntegrity) - 1; // -1 to make it 0 when everything is at max 
    }

    public void IncreaseStreamTime()
    {
        RemainingStreamTime += StreamTimeIncrease;
    }

    public void AddViewers(int value)
    {
        CurrentViewers += value;
    }


    public void TakeDamage(int value)
    {
        Integrity -= value;
        if(Integrity > MaxIntegrity)
        {
            Integrity = MaxIntegrity;
        }

        if(Integrity < 0)
        {
            //to-do game over
            Integrity = 0;
            StartCoroutine(EndingManager.Instance.EndGame(0));
        }
    }

    public IEnumerator UpdateHype()
    {
        while (true)
        {
            if(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }
            if(CurrentHype < TargetHype)
            {
                CurrentHype += HypeChangeAmount;
                
                if(CurrentHype >= TargetHype)
                {
                    CurrentHype = TargetHype;
                    TargetHype = -999;
                }
            }
            else
            {
                CurrentHype -= HypeChangeAmount;
            }
            UpdateBonus();

            yield return new WaitForSeconds(HypeUpdateInterval);
        }
    }


}
