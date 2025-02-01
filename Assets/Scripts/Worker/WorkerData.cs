using UnityEngine;

[System.Serializable]
public class WorkerData
{
    [SerializeField] public float Health = 50f;
    [SerializeField] public WorkerAttributes BaseAttributes = new WorkerAttributes(1,1,1,1);
    [SerializeField] public WorkerAttributes AllocAttributes = new WorkerAttributes();
    [SerializeField] public WorkerAttributes TempAttributes = new WorkerAttributes(); //buff or debuff
    [SerializeField] public WorkerAttributes TotalAttributes;
    [SerializeField] public int MaxAttribute = 5;
    [SerializeField] public int Level = 1;
    [SerializeField] public WorkerStats BaseStats = new WorkerStats(5f,5f,5f,5f,60f,30f);
    [SerializeField] public WorkerStats TempStats = new WorkerStats();
    [SerializeField] public WorkerStats TotalStats;

    public int AllocateAttributes(WorkerAttributes addedAttributes)
    {
        int allocedAttributes = 0;
        if(addedAttributes.Robustness > 0)
        {
            int allocedRobustness = Mathf.Min(addedAttributes.Robustness, MaxAttribute - BaseAttributes.Robustness - AllocAttributes.Robustness);
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
            int allocedLatency = Mathf.Min(addedAttributes.Latency, MaxAttribute - BaseAttributes.Latency - AllocAttributes.Latency);
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
            int allocedAccuracy = Mathf.Min(addedAttributes.Accuracy, MaxAttribute - BaseAttributes.Accuracy - AllocAttributes.Accuracy);
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
            int allocedFitness = Mathf.Min(addedAttributes.Fitness, MaxAttribute - BaseAttributes.Fitness - AllocAttributes.Fitness);
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
        BaseAttributes.Robustness += AllocAttributes.Robustness;
        BaseAttributes.Latency += AllocAttributes.Latency;
        BaseAttributes.Accuracy += AllocAttributes.Accuracy;
        BaseAttributes.Fitness += AllocAttributes.Fitness;
        AllocAttributes = new WorkerAttributes(0,0,0,0);
        Level = Mathf.RoundToInt(BaseAttributes.Average());
        UpdateTotalAttribute();

    }

    public void AddTempAttributes(WorkerAttributes addedAttributes)
    {
        TempAttributes.Robustness += addedAttributes.Robustness;
        TempAttributes.Latency += addedAttributes.Latency;
        TempAttributes.Accuracy += addedAttributes.Accuracy;
        TempAttributes.Fitness += addedAttributes.Fitness;
        UpdateTotalAttribute();
        
    }

    public void AddTempStats(WorkerStats addedStats)
    {
        TempStats.MaxHealth += addedStats.MaxHealth;
        TempStats.RegenTime += addedStats.RegenTime;
        TempStats.WorkTime += addedStats.WorkTime;
        TempStats.RecallCooldown += addedStats.RecallCooldown;
        TempStats.WorkSuccessChance += addedStats.WorkSuccessChance;
        TempStats.WorkAmount += addedStats.WorkAmount;
        UpdateBaseStats();
    }


    public void UpdateTotalAttribute()
    {
        TotalAttributes = BaseAttributes + TempAttributes;
        Level = Mathf.RoundToInt(TotalAttributes.Average());
        UpdateBaseStats();
    }
    
    private void UpdateBaseStats()
    {
        BaseStats.MaxHealth = 50f + (TotalAttributes.Robustness * 10f);
        BaseStats.RegenTime = 5f - (TotalAttributes.Robustness * 0.5f);
        BaseStats.WorkTime = 5f - (TotalAttributes.Latency * 0.5f);
        BaseStats.RecallCooldown = 5f - (TotalAttributes.Latency * 0.5f);
        BaseStats.WorkSuccessChance = 60f + (TotalAttributes.Accuracy * 5f);
        BaseStats.WorkAmount = 30f + (TotalAttributes.Fitness * 5f);
        UpdateTotalStats();
    }

    private void UpdateTotalStats()
    {
        TotalStats = BaseStats + TempStats;
    }
}

[System.Serializable]
public struct WorkerAttributes
{
    public int Robustness; //health, health regen rate
    public float RobustnessScore;
    public int Latency; //repair rate, recall cooldown
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

    public float Sum()
    {
        return Robustness + Latency + Accuracy + Fitness;
    }
    public float Average()
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
    public float RegenTime; //time to regen full health
    public float WorkTime; //time to complete work, heal and deal damage
    public float RecallCooldown; //time to wait before starting new work
    public float WorkSuccessChance; 
    public float WorkAmount; //heal amount, damage amount
    
    public WorkerStats(float maxHealth, float regenTime, float workTime, float recallCooldown, float workSuccessChance, float workAmount)
    {
        MaxHealth = maxHealth;
        RegenTime = regenTime;
        WorkTime = workTime;
        RecallCooldown = recallCooldown;
        WorkSuccessChance = workSuccessChance;
        WorkAmount = workAmount;
    }

    public static WorkerStats operator +(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth + b.MaxHealth, a.RegenTime + b.RegenTime, a.WorkTime + b.WorkTime, a.RecallCooldown + b.RecallCooldown, a.WorkSuccessChance + b.WorkSuccessChance, a.WorkAmount + b.WorkAmount);
    }

    public static WorkerStats operator -(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth - b.MaxHealth, a.RegenTime - b.RegenTime, a.WorkTime - b.WorkTime, a.RecallCooldown - b.RecallCooldown, a.WorkSuccessChance - b.WorkSuccessChance, a.WorkAmount - b.WorkAmount);
    }

    public static WorkerStats operator *(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth * b, a.RegenTime * b, a.WorkTime * b, a.RecallCooldown * b, a.WorkSuccessChance * b, a.WorkAmount * b);
    }

    public static WorkerStats operator /(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth / b, a.RegenTime / b, a.WorkTime / b, a.RecallCooldown / b, a.WorkSuccessChance / b, a.WorkAmount / b);
    }

    public static WorkerStats operator -(WorkerStats a)
    {
        return new WorkerStats(-a.MaxHealth, -a.RegenTime, -a.WorkTime, -a.RecallCooldown, -a.WorkSuccessChance, -a.WorkAmount);
    }

}