using UnityEngine;

[System.Serializable]
public class WorkerData
{
    [SerializeField] public float Health = 50f;
    [SerializeField] public WorkerAttributes BaseAttributes = new WorkerAttributes(0,0,0,0);
    [SerializeField] public WorkerAttributes AllocAttributes = new WorkerAttributes();
    [SerializeField] public WorkerAttributes TempAttributes = new WorkerAttributes(); //buff or debuff
    [SerializeField] public WorkerAttributes TotalAttributes;
    [SerializeField] public int MaxAttribute = 6;
    [SerializeField] public int Level = 1;
    [SerializeField] public WorkerStats DefaultStats = new WorkerStats(10f, 5f, 2.5f, 10, 60f, 0f, 1f, 5f);
    [SerializeField] public WorkerStats BaseStats = new WorkerStats();
    [SerializeField] public WorkerStats TempStats = new WorkerStats();
    [SerializeField] public WorkerStats statPerAttribute = new WorkerStats(5f,-0.5f, 0.5f, 2, 5f, 5f, -0.1f, -0.5f);
    [SerializeField] public WorkerStats TotalStats;

    public event System.Action OnAttributeOrStatChanged;

    public int AllocateAttributes(WorkerAttributes addedAttributes)
    {
        int allocedAttributes = 0;
        if(addedAttributes.Heart > 0)
        {
            int allocedHeart = Mathf.Min(addedAttributes.Heart, MaxAttribute - BaseAttributes.Heart - AllocAttributes.Heart);
            AllocAttributes.Heart += allocedHeart;
            allocedAttributes += allocedHeart;
        }
        else
        {
            int unallocedHeart = Mathf.Min(-addedAttributes.Heart, AllocAttributes.Heart);
            AllocAttributes.Heart -= unallocedHeart;
            allocedAttributes -= unallocedHeart;
        }
        if(addedAttributes.ErrorRecovery > 0)
        {
            int allocedErrorRecovery = Mathf.Min(addedAttributes.ErrorRecovery, MaxAttribute - BaseAttributes.ErrorRecovery - AllocAttributes.ErrorRecovery);
            AllocAttributes.ErrorRecovery += allocedErrorRecovery;
            allocedAttributes += allocedErrorRecovery;
        }
        else
        {
            int unallocedErrorRecovery = Mathf.Min(-addedAttributes.ErrorRecovery, AllocAttributes.ErrorRecovery);
            AllocAttributes.ErrorRecovery -= unallocedErrorRecovery;
            allocedAttributes -= unallocedErrorRecovery;
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
        return allocedAttributes;
    }
    public int ResetAllocAttributes()
    {
        int resetAttributes = AllocAttributes.Heart + + AllocAttributes.ErrorRecovery + AllocAttributes.Accuracy + AllocAttributes.Latency;
        AllocAttributes.Heart = 0;
        AllocAttributes.ErrorRecovery = 0;
        AllocAttributes.Accuracy = 0;
        AllocAttributes.Latency = 0;
        return resetAttributes;
    }

    public void ApplyAllocAttributes()
    {
        BaseAttributes.Heart += AllocAttributes.Heart;
        BaseAttributes.ErrorRecovery += AllocAttributes.ErrorRecovery;
        BaseAttributes.Accuracy += AllocAttributes.Accuracy;
        BaseAttributes.Latency += AllocAttributes.Latency;
        AllocAttributes = new WorkerAttributes(0,0,0,0);
        Level = Mathf.RoundToInt(BaseAttributes.Average());
        UpdateTotalAttribute();
    }

    public void AddTempAttributes(WorkerAttributes addedAttributes)
    {
        TempAttributes.Heart += addedAttributes.Heart;
        TempAttributes.ErrorRecovery += addedAttributes.ErrorRecovery;
        TempAttributes.Accuracy += addedAttributes.Accuracy;
        TempAttributes.Latency += addedAttributes.Latency;
        UpdateTotalAttribute();
        
    }

    public void AddTempStats(WorkerStats addedStats)
    {
        TempStats.MaxHealth += addedStats.MaxHealth;
        TempStats.RegenTime += addedStats.RegenTime;
        TempStats.RestoreAmount += addedStats.RestoreAmount;
        TempStats.TaskAmount += addedStats.TaskAmount;
        TempStats.TaskSuccessChance += addedStats.TaskSuccessChance;
        TempStats.DamageIgnoreChance += addedStats.DamageIgnoreChance;
        TempStats.TaskTime += addedStats.TaskTime;
        TempStats.ResponseTime += addedStats.ResponseTime;
        UpdateTotalStats();
    }


    public WorkerStats CalculateStatsFromAttributes(WorkerAttributes attributes)
    {
        WorkerStats stats = new WorkerStats();
        stats.MaxHealth = attributes.Heart * statPerAttribute.MaxHealth;
        stats.RegenTime = attributes.Heart * statPerAttribute.RegenTime;
        stats.RestoreAmount = attributes.ErrorRecovery * statPerAttribute.RestoreAmount;
        stats.TaskAmount = attributes.Accuracy * statPerAttribute.TaskAmount;
        stats.TaskSuccessChance = attributes.Accuracy * statPerAttribute.TaskSuccessChance;
        stats.DamageIgnoreChance = attributes.Accuracy * statPerAttribute.DamageIgnoreChance;
        stats.TaskTime = attributes.Latency * statPerAttribute.TaskTime;
        stats.ResponseTime = attributes.Latency * statPerAttribute.ResponseTime;
        return stats;
    }

    public void UpdateTotalAttribute()
    {
        TotalAttributes = BaseAttributes + TempAttributes;
        Level = Mathf.RoundToInt(TotalAttributes.Average());
        UpdateBaseStats();
    }
    
    public WorkerAttributes GetAllocedTotalAttributes()
    {
        return AllocAttributes + TotalAttributes;
    }
    public void UpdateBaseStats()
    {
        BaseStats = DefaultStats + CalculateStatsFromAttributes(BaseAttributes);
        UpdateTotalStats();
    }


    private void UpdateTotalStats()
    {
        TotalStats = BaseStats + TempStats + CalculateStatsFromAttributes(TempAttributes);
        OnAttributeOrStatChanged?.Invoke();
    }

}

[System.Serializable]
public struct WorkerAttributes
{
    public int Heart; //health, health regen rate
    public float HeartScore;
    public int ErrorRecovery; //repair amount
    public float ErrorRecoveryScore;
    public int Accuracy; //repair success chance
    public float AccuracyScore;
    public int Latency; //repair rate, recall cooldown
    public float LatencyScore;

    public WorkerAttributes(int heart, int errorRecovery, int accuracy , int latency)
    {
        Heart = heart;
        ErrorRecovery = errorRecovery;
        Accuracy = accuracy;
        Latency = latency;
        HeartScore = 0;
        ErrorRecoveryScore = 0;
        AccuracyScore = 0;
        LatencyScore = 0;
    }

    public float Sum()
    {
        return Heart + ErrorRecovery + Accuracy + Latency;
    }
    public float Average()
    {
        return (Heart + ErrorRecovery + Accuracy + Latency) / 4;
    }

    public static WorkerAttributes operator +(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Heart + b.Heart, a.ErrorRecovery + b.ErrorRecovery, a.Accuracy + b.Accuracy, a.Latency + b.Latency );
    }

    public static WorkerAttributes operator -(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Heart - b.Heart, a.ErrorRecovery - b.ErrorRecovery, a.Accuracy - b.Accuracy, a.Latency - b.Latency);
    }

    public static WorkerAttributes operator *(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Heart * b, a.ErrorRecovery * b, a.Accuracy * b, a.Latency * b);
    }

    public static WorkerAttributes operator /(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Heart / b, a.ErrorRecovery / b, a.Accuracy / b, a.Latency / b);
    }

    public static WorkerAttributes operator -(WorkerAttributes a)
    {
        return new WorkerAttributes(-a.Heart, -a.ErrorRecovery, -a.Accuracy, -a.Latency);
    }

}

[System.Serializable]
public struct WorkerStats
{
    public float MaxHealth;
    public float RegenTime; //time to regen full health
    public float RestoreAmount; //heal amount, damage amount per task
    public int TaskAmount; //how many tasks can be done per work
    public float TaskSuccessChance; 
    public float DamageIgnoreChance;
    public float TaskTime; //time to complete a task
    public float ResponseTime; //time to wait before starting new work
    
    public WorkerStats(float maxHealth, float regenTime, float restoreAmount, int workAmount, float workSuccessChance, float damageIgnoreChance, float workTime, float responseTime)
    {
        MaxHealth = maxHealth;
        RegenTime = regenTime;
        RestoreAmount = restoreAmount;
        TaskAmount = workAmount;
        TaskSuccessChance = workSuccessChance;
        DamageIgnoreChance = damageIgnoreChance;
        TaskTime = workTime;
        ResponseTime = responseTime;
    }

    public static WorkerStats operator +(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(
            a.MaxHealth + b.MaxHealth, 
            a.RegenTime + b.RegenTime,
            a.RestoreAmount + b.RestoreAmount,
            a.TaskAmount + b.TaskAmount,
            a.TaskSuccessChance + b.TaskSuccessChance,
            a.DamageIgnoreChance + b.DamageIgnoreChance,
            a.TaskTime + b.TaskTime,
            a.ResponseTime + b.ResponseTime
        );
    }

    public static WorkerStats operator -(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(
            a.MaxHealth - b.MaxHealth, 
            a.RegenTime - b.RegenTime, 
            a.RestoreAmount - b.RestoreAmount, 
            a.TaskAmount - b.TaskAmount,
            a.TaskSuccessChance - b.TaskSuccessChance, 
            a.DamageIgnoreChance - b.DamageIgnoreChance, 
            a.TaskTime - b.TaskTime, 
            a.ResponseTime - b.ResponseTime
        );
    }

    public static WorkerStats operator *(WorkerStats a, float b)
    {
        return new WorkerStats(
            a.MaxHealth * b, 
            a.RegenTime * b, 
            a.RestoreAmount * b, 
            a.TaskAmount * (int)b,
            a.TaskSuccessChance * b,
            a.DamageIgnoreChance * b, 
            a.TaskTime * b, 
            a.ResponseTime * b
        );
    }

    public static WorkerStats operator /(WorkerStats a, float b)
    {
        return new WorkerStats(
            a.MaxHealth / b,
            a.RegenTime / b,
            a.RestoreAmount / b, 
            a.TaskAmount / (int)b,
            a.TaskSuccessChance / b,
            a.DamageIgnoreChance / b, 
            a.TaskTime / b, 
            a.ResponseTime / b
        );
    }

    public static WorkerStats operator -(WorkerStats a)
    {
        return new WorkerStats(
            -a.MaxHealth, 
            -a.RegenTime, 
            -a.RestoreAmount, 
            -a.TaskAmount,
            -a.TaskSuccessChance,
            -a.DamageIgnoreChance, 
            -a.TaskTime, 
            -a.ResponseTime
        );
    }

}