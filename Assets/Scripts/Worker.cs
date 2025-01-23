using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public struct WorkerStats
{
    public int Robustness; //health, health regen
    public float RobustnessScore;
    public int Latency; //repair speed, repair cooldown
    public float LatencyScore;
    public int Reliability; //repair success chance
    public float ReliabilityScore;
    public int Fitness; //repair amount
    public float FitnessScore;

    public WorkerStats(int robustness, int latency, int reliability, int fitness)
    {
        Robustness = robustness;
        Latency = latency;
        Reliability = reliability;
        Fitness = fitness;
        RobustnessScore = 0;
        LatencyScore = 0;
        ReliabilityScore = 0;
        FitnessScore = 0;
    }
    public float AverageStat()
    {
        return (Robustness + Latency + Reliability + Fitness) / 4;
    }

}

public class Worker : MonoBehaviour
{

    [SerializeField] public string Identifier;
    [SerializeField] public Image WorkerIcon;
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public UIEventHandler ClickHandler; //can be put into its own class later maybe?
    [SerializeField] public Image CooldownIcon;
    [SerializeField] public WorkerStats Stats = new WorkerStats(1,1,1,1);
    [SerializeField] public WorkerStats AllocStats = new WorkerStats(0,0,0,0);
    [SerializeField] public int MaxStat = 5;
    [SerializeField] public int Level = 1;
    [SerializeField] public int health = 5;
    [SerializeField] public float RepairSpeed = 5f;
    [SerializeField] public float RepairSuccessChance = 60f;
    [SerializeField] public float RepairCooldown = 5f;
    [SerializeField] public float RepairAmount = 30f;
    [SerializeField] public bool IsAvailable = true;

    public event System.Action<Worker> OnSelected; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Level = Mathf.RoundToInt(Stats.AverageStat());
        ClickHandler.OnLeftClickEvent += (eventData) => Selected();
    }

    public int IncreaseAllocStats(WorkerStats stats)
    {
        int allocedStats = 0;
        if(stats.Robustness > 0)
        {
            int allocedRobustness = Mathf.Min(stats.Robustness, MaxStat - Stats.Robustness);
            AllocStats.Robustness = allocedRobustness;
            allocedStats += allocedRobustness;
        }
        if(stats.Latency > 0)
        {
            int allocedLatency = Mathf.Min(stats.Latency, MaxStat - Stats.Latency);
            AllocStats.Latency = allocedLatency;
            allocedStats += allocedLatency;
        }
        if(stats.Reliability > 0)
        {
            int allocedReliability = Mathf.Min(stats.Reliability, MaxStat - Stats.Reliability);
            AllocStats.Reliability = allocedReliability;
            allocedStats += allocedReliability;
        }
        if(stats.Fitness > 0)
        {
            int allocedFitness = Mathf.Min(stats.Fitness, MaxStat - Stats.Fitness);
            AllocStats.Fitness = allocedFitness;
            allocedStats += allocedFitness;
        }
        return allocedStats;
    }
    public int ResetAllocStats()
    {
        int resetStats = AllocStats.Robustness + AllocStats.Latency + AllocStats.Reliability + AllocStats.Fitness;
        AllocStats.Robustness = 0;
        AllocStats.Latency = 0;
        AllocStats.Reliability = 0;
        AllocStats.Fitness = 0;
        return resetStats;
    }

    public void ApplyAllocStats()
    {
        Stats.Robustness += AllocStats.Robustness;
        Stats.Latency += AllocStats.Latency;
        Stats.Reliability += AllocStats.Reliability;
        Stats.Fitness += AllocStats.Fitness;
        Level = Mathf.RoundToInt(Stats.AverageStat());
    }

    public void Selected()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair || !IsAvailable) return;
        PlayerManager.Instance.SetState(PlayerManager.PlayerState.repair);
        SetAvailability(false);
        OnSelected?.Invoke(this);
    }

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
        if(IsAvailable)
        {
            CooldownIcon.gameObject.SetActive(false);
        }
        else
        {
            CooldownIcon.gameObject.SetActive(true);
        }
    }

    public virtual void StartRepairing(MemoryEntity entity)
    {
        entity.IsBeingRepaired = true;
        entity.Interactable = false;
        StartCoroutine(Repairing(entity));
    }

    private IEnumerator Repairing(MemoryEntity entity)
    {
        yield return new WaitForSeconds(RepairSpeed);
        RollRepairSuccessChance(entity);
        FinishRepairing(entity);
    }

    private void RollRepairSuccessChance(MemoryEntity entity)
    {
        float roll = Random.Range(0, 100);
        if(roll < RepairSuccessChance)
        {
            entity.AddIntegrity(RepairAmount);
            entity.Recall();
        }
        else
        {

            entity.RollChanceToGlitch();
        }
    } 

    public virtual void FinishRepairing(MemoryEntity entity)
    {
        entity.IsBeingRepaired = false;
        entity.Interactable = true;
        IsAvailable = true;
    }


    public void FinishWork()
    {
        StartCoroutine(StartCooldown());
    }

    public IEnumerator StartCooldown()
    {
        float elapsedTime = 0f;;
        while(elapsedTime < RepairCooldown)
        {
            CooldownIcon.fillAmount = 1-(elapsedTime / RepairCooldown);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetAvailability(true);
    }
    

    
    void OnDestroy()
    {
        ClickHandler.OnLeftClickEvent -= (eventData) => Selected();
    }
}
