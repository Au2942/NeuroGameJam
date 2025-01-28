using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Worker : MonoBehaviour
{

    [SerializeField] public string Identifier;
    [SerializeField] public Image WorkerIcon;
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public UIEventHandler ClickHandler; //can be put into its own class later maybe?
    [SerializeField] public Image CooldownIcon;
    [SerializeField] public WorkerAttributes Attributes = new WorkerAttributes(1,1,1,1);
    [SerializeField] public WorkerAttributes AllocAttributes = new WorkerAttributes();
    [SerializeField] public WorkerAttributes TempAttributes = new WorkerAttributes(); //buff or debuff
    [SerializeField] public int MaxAttribute = 5;
    [SerializeField] public int Level = 1;
    [SerializeField] public WorkerStats Stats = new WorkerStats(5f,5f,5f,5f,60f,30f);
    [SerializeField] public WorkerStats TempStats = new WorkerStats();
    [SerializeField] public int health = 5;
    [SerializeField] public List<WorkerStatusEffect> WorkerStatusEffects = new List<WorkerStatusEffect>();
    [SerializeField] public bool IsAvailable = true;
    [SerializeField] public WorkHandler WorkHandler;

    public MemoryEntity assignedEntity => WorkHandler.entity;

    public event System.Action<Worker> OnSelected; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Level = Mathf.RoundToInt(Attributes.AverageAttributes());
        UpdateStats();
        ClickHandler.OnLeftClickEvent += (eventData) => Select();
    }

    void Update()
    {
        foreach(WorkerStatusEffect statusEffect in WorkerStatusEffects)
        {
            statusEffect.OnUpdate(Time.deltaTime);
            if(statusEffect.ShouldExpire())
            {
                statusEffect.Remove();
                WorkerStatusEffects.Remove(statusEffect);
            }
        }
    }

    public void ApplyStatusEffect(WorkerStatusEffect statusEffect, Entity source = null)
    {
        WorkerStatusEffects.Add(statusEffect);
        statusEffect.Apply(source, this);
    }
    
    public void RemoveStatusEffect(WorkerStatusEffect statusEffect)
    {
        WorkerStatusEffects.Remove(statusEffect);
    }

    public int AddAllocAttributes(WorkerAttributes addedAttributes)
    {
        int allocedAttributes = 0;
        if(addedAttributes.Robustness > 0)
        {
            int allocedRobustness = Mathf.Min(addedAttributes.Robustness, MaxAttribute - Attributes.Robustness - AllocAttributes.Robustness);
            AllocAttributes.Robustness += allocedRobustness;
            allocedAttributes += allocedRobustness;
        }
        else
        {
            int unallocedRobustness = Mathf.Min(-addedAttributes.Robustness, AllocAttributes.Robustness);
            AllocAttributes.Robustness -= unallocedRobustness;
            allocedAttributes -= unallocedRobustness;
        }
        if(addedAttributes.Latency > 0)
        {
            int allocedLatency = Mathf.Min(addedAttributes.Latency, MaxAttribute - Attributes.Latency - AllocAttributes.Latency);
            AllocAttributes.Latency += allocedLatency;
            allocedAttributes += allocedLatency;
        }
        else
        {
            int unallocedLatency = Mathf.Min(-addedAttributes.Latency, AllocAttributes.Latency);
            AllocAttributes.Latency -= unallocedLatency;
            allocedAttributes -= unallocedLatency;
        }
        if(addedAttributes.Accuracy > 0)
        {
            int allocedAccuracy = Mathf.Min(addedAttributes.Accuracy, MaxAttribute - Attributes.Accuracy - AllocAttributes.Accuracy);
            AllocAttributes.Accuracy += allocedAccuracy;
            allocedAttributes += allocedAccuracy;
        }
        else
        {
            int unallocedAccuracy = Mathf.Min(-addedAttributes.Accuracy, AllocAttributes.Accuracy);
            AllocAttributes.Accuracy -= unallocedAccuracy;
            allocedAttributes -= unallocedAccuracy;
        }
        if(addedAttributes.Fitness > 0)
        {
            int allocedFitness = Mathf.Min(addedAttributes.Fitness, MaxAttribute - Attributes.Fitness - AllocAttributes.Fitness);
            AllocAttributes.Fitness += allocedFitness;
            allocedAttributes += allocedFitness;
        }
        else
        {
            int unallocedFitness = Mathf.Min(-addedAttributes.Fitness, AllocAttributes.Fitness);
            AllocAttributes.Fitness -= unallocedFitness;
            allocedAttributes -= unallocedFitness;
        }
        return allocedAttributes;
    }
    public int ResetAllocAttributes()
    {
        int resetAttributes = AllocAttributes.Robustness + AllocAttributes.Latency + AllocAttributes.Accuracy + AllocAttributes.Fitness;
        AllocAttributes.Robustness = 0;
        AllocAttributes.Latency = 0;
        AllocAttributes.Accuracy = 0;
        AllocAttributes.Fitness = 0;
        return resetAttributes;
    }

    public void ApplyAllocAttributes()
    {
        Attributes.Robustness += AllocAttributes.Robustness;
        Attributes.Latency += AllocAttributes.Latency;
        Attributes.Accuracy += AllocAttributes.Accuracy;
        Attributes.Fitness += AllocAttributes.Fitness;
        AllocAttributes = new WorkerAttributes(0,0,0,0);
        Level = Mathf.RoundToInt(Attributes.AverageAttributes());
        UpdateStats();
    }

    public void AddTempAttributes(WorkerAttributes addedAttributes)
    {
        TempAttributes.Robustness += addedAttributes.Robustness;
        TempAttributes.Latency += addedAttributes.Latency;
        TempAttributes.Accuracy += addedAttributes.Accuracy;
        TempAttributes.Fitness += addedAttributes.Fitness;
        UpdateStats();
    }

    public void AddTempStats(WorkerStats addedStats)
    {
        TempStats.MaxHealth += addedStats.MaxHealth;
        TempStats.RegenSpeed += addedStats.RegenSpeed;
        TempStats.WorkSpeed += addedStats.WorkSpeed;
        TempStats.WorkCooldown += addedStats.WorkCooldown;
        TempStats.WorkSuccessChance += addedStats.WorkSuccessChance;
        TempStats.WorkAmount += addedStats.WorkAmount;
        UpdateStats();
    }

    public WorkerAttributes GetTotalAttributes()
    {
        return Attributes + TempAttributes;
    }

    public WorkerStats GetTotalStats()
    {
        return Stats + TempStats;
    }

    public void Select()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.command || !IsAvailable) return;
        if(!GameManager.Instance.isPause)
        {
            PlayerManager.Instance.SetState(PlayerManager.PlayerState.command);
        }
        SetAvailability(false);
        OnSelected?.Invoke(this);
    }


    public void Deselect()
    {
        SetAvailability(true);
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
    public void DoMaintenance(MemoryEntity entity)
    {
        WorkHandler.InitiateMaintenanceWork(entity, this);
    }

    public void DoRepair(MemoryEntity entity)
    {
        WorkHandler.InitiateRepairWork(entity, this);
    }

    public IEnumerator StartCooldown()
    {
        float elapsedTime = 0f;;
        while(elapsedTime < Stats.WorkCooldown)
        {
            CooldownIcon.fillAmount = 1-(elapsedTime / Stats.WorkCooldown);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetAvailability(true);
    }
    
    private void UpdateStats()
    {
        Stats.WorkSpeed = 5f - (Attributes.Latency * 0.5f);
        Stats.WorkCooldown = 5f - (Attributes.Latency * 0.5f);
        Stats.WorkSuccessChance = 60f + (Attributes.Accuracy * 5f);
        Stats.WorkAmount = 30f + (Attributes.Fitness * 5f);
    }
    
    void OnDestroy()
    {
        ClickHandler.OnLeftClickEvent -= (eventData) => Select();
    }
}

[System.Serializable]
public struct WorkerAttributes
{
    public int Robustness; //health, health regen
    public float RobustnessScore;
    public int Latency; //repair speed, repair cooldown
    public float LatencyScore;
    public int Accuracy; //repair success chance
    public float AccuracyScore;
    public int Fitness; //repair amount
    public float FitnessScore;

    public WorkerAttributes(int robustness, int latency, int accuracy, int fitness)
    {
        Robustness = robustness;
        Latency = latency;
        Accuracy = accuracy;
        Fitness = fitness;
        RobustnessScore = 0;
        LatencyScore = 0;
        AccuracyScore = 0;
        FitnessScore = 0;
    }
    public float AverageAttributes()
    {
        return (Robustness + Latency + Accuracy + Fitness) / 4;
    }

    public static WorkerAttributes operator +(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Robustness + b.Robustness, a.Latency + b.Latency, a.Accuracy + b.Accuracy, a.Fitness + b.Fitness);
    }

    public static WorkerAttributes operator -(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Robustness - b.Robustness, a.Latency - b.Latency, a.Accuracy - b.Accuracy, a.Fitness - b.Fitness);
    }

    public static WorkerAttributes operator *(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Robustness * b, a.Latency * b, a.Accuracy * b, a.Fitness * b);
    }

    public static WorkerAttributes operator /(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Robustness / b, a.Latency / b, a.Accuracy / b, a.Fitness / b);
    }

    public static WorkerAttributes operator -(WorkerAttributes a)
    {
        return new WorkerAttributes(-a.Robustness, -a.Latency, -a.Accuracy, -a.Fitness);
    }

}
[System.Serializable]
public struct WorkerStats
{
    public float MaxHealth;
    public float RegenSpeed;
    public float WorkSpeed;
    public float WorkCooldown;
    public float WorkSuccessChance;
    public float WorkAmount;
    
    public WorkerStats(float maxHealth, float regenSpeed, float workSpeed, float workCooldown, float workSuccessChance, float workAmount)
    {
        MaxHealth = maxHealth;
        RegenSpeed = regenSpeed;
        WorkSpeed = workSpeed;
        WorkCooldown = workCooldown;
        WorkSuccessChance = workSuccessChance;
        WorkAmount = workAmount;
    }

    public static WorkerStats operator +(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth + b.MaxHealth, a.RegenSpeed + b.RegenSpeed, a.WorkSpeed + b.WorkSpeed, a.WorkCooldown + b.WorkCooldown, a.WorkSuccessChance + b.WorkSuccessChance, a.WorkAmount + b.WorkAmount);
    }

    public static WorkerStats operator -(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth - b.MaxHealth, a.RegenSpeed - b.RegenSpeed, a.WorkSpeed - b.WorkSpeed, a.WorkCooldown - b.WorkCooldown, a.WorkSuccessChance - b.WorkSuccessChance, a.WorkAmount - b.WorkAmount);
    }

    public static WorkerStats operator *(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth * b, a.RegenSpeed * b, a.WorkSpeed * b, a.WorkCooldown * b, a.WorkSuccessChance * b, a.WorkAmount * b);
    }

    public static WorkerStats operator /(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth / b, a.RegenSpeed / b, a.WorkSpeed / b, a.WorkCooldown / b, a.WorkSuccessChance / b, a.WorkAmount / b);
    }

    public static WorkerStats operator -(WorkerStats a)
    {
        return new WorkerStats(-a.MaxHealth, -a.RegenSpeed, -a.WorkSpeed, -a.WorkCooldown, -a.WorkSuccessChance, -a.WorkAmount);
    }

}