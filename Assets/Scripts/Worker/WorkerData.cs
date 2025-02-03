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
        if(addedAttributes.Erm > 0)
        {
            int allocedErm = Mathf.Min(addedAttributes.Erm, MaxAttribute - BaseAttributes.Erm - AllocAttributes.Erm);
            AllocAttributes.Erm += allocedErm;
            allocedAttributes += allocedErm;
        }
        else
        {
            int unallocedErm = Mathf.Min(-addedAttributes.Erm, AllocAttributes.Erm);
            AllocAttributes.Erm -= unallocedErm;
            allocedAttributes -= unallocedErm;
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
        int resetAttributes = AllocAttributes.Heart + + AllocAttributes.Erm + AllocAttributes.Accuracy + AllocAttributes.Latency;
        AllocAttributes.Heart = 0;
        AllocAttributes.Erm = 0;
        AllocAttributes.Accuracy = 0;
        AllocAttributes.Latency = 0;
        return resetAttributes;
    }

    public void ApplyAllocAttributes()
    {
        BaseAttributes.Heart += AllocAttributes.Heart;
        BaseAttributes.Erm += AllocAttributes.Erm;
        BaseAttributes.Accuracy += AllocAttributes.Accuracy;
        BaseAttributes.Latency += AllocAttributes.Latency;
        AllocAttributes = new WorkerAttributes(0,0,0,0);
        Level = Mathf.RoundToInt(BaseAttributes.Average());
        UpdateTotalAttribute();

    }

    public void AddTempAttributes(WorkerAttributes addedAttributes)
    {
        TempAttributes.Heart += addedAttributes.Heart;
        TempAttributes.Erm += addedAttributes.Erm;
        TempAttributes.Accuracy += addedAttributes.Accuracy;
        TempAttributes.Latency += addedAttributes.Latency;
        UpdateTotalAttribute();
        
    }

    public void AddTempStats(WorkerStats addedStats)
    {
        TempStats.MaxHealth += addedStats.MaxHealth;
        TempStats.RegenTime += addedStats.RegenTime;
        TempStats.WorkAmount += addedStats.WorkAmount;
        TempStats.WorkSuccessChance += addedStats.WorkSuccessChance;
        TempStats.WorkTime += addedStats.WorkTime;
        TempStats.RecallCooldown += addedStats.RecallCooldown;
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
        BaseStats.MaxHealth = 50f + (TotalAttributes.Heart * 10f);
        BaseStats.RegenTime = 5f - (TotalAttributes.Heart * 0.5f);
        BaseStats.WorkAmount = 30f + (TotalAttributes.Erm * 5f);
        BaseStats.WorkSuccessChance = 60f + (TotalAttributes.Accuracy * 5f);
        BaseStats.WorkTime = 5f - (TotalAttributes.Latency * 0.5f);
        BaseStats.RecallCooldown = 5f - (TotalAttributes.Latency * 0.5f);
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
    public int Heart; //health, health regen rate
    public float HeartScore;
    public int Erm; //repair amount
    public float ErmScore;
    public int Accuracy; //repair success chance
    public float AccuracyScore;
    public int Latency; //repair rate, recall cooldown
    public float LatencyScore;

    public WorkerAttributes(int heart, int erm, int latency, int accuracy )
    {
        Heart = heart;
        Latency = latency;
        Accuracy = accuracy;
        Erm = erm;
        HeartScore = 0;
        ErmScore = 0;
        AccuracyScore = 0;
        LatencyScore = 0;
    }

    public float Sum()
    {
        return Heart + Erm + Accuracy + Latency;
    }
    public float Average()
    {
        return (Heart + Erm + Accuracy + Latency) / 4;
    }

    public static WorkerAttributes operator +(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Heart + b.Heart, a.Erm + b.Erm, a.Accuracy + b.Accuracy, a.Latency + b.Latency );
    }

    public static WorkerAttributes operator -(WorkerAttributes a, WorkerAttributes b)
    {
        return new WorkerAttributes(a.Heart - b.Heart, a.Erm - b.Erm, a.Accuracy - b.Accuracy, a.Latency - b.Latency);
    }

    public static WorkerAttributes operator *(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Heart * b, a.Erm * b, a.Accuracy * b, a.Latency * b);
    }

    public static WorkerAttributes operator /(WorkerAttributes a, int b)
    {
        return new WorkerAttributes(a.Heart / b, a.Erm / b, a.Accuracy / b, a.Latency / b);
    }

    public static WorkerAttributes operator -(WorkerAttributes a)
    {
        return new WorkerAttributes(-a.Heart, -a.Erm, -a.Accuracy, -a.Latency);
    }

}
[System.Serializable]
public struct WorkerStats
{
    public float MaxHealth;
    public float RegenTime; //time to regen full health
    public float WorkAmount; //heal amount, damage amount
    public float WorkSuccessChance; 
    public float WorkTime; //time to complete work, heal and deal damage
    public float RecallCooldown; //time to wait before starting new work
    
    public WorkerStats(float maxHealth, float workAmount, float regenTime, float workSuccessChance, float workTime, float recallCooldown)
    {
        MaxHealth = maxHealth;
        RegenTime = regenTime;
        WorkAmount = workAmount;
        WorkSuccessChance = workSuccessChance;
        WorkTime = workTime;
        RecallCooldown = recallCooldown;
    }

    public static WorkerStats operator +(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth + b.MaxHealth, a.RegenTime + b.RegenTime, a.WorkAmount + b.WorkAmount, a.WorkSuccessChance + b.WorkSuccessChance, a.WorkTime + b.WorkTime, a.RecallCooldown + b.RecallCooldown);
    }

    public static WorkerStats operator -(WorkerStats a, WorkerStats b)
    {
        return new WorkerStats(a.MaxHealth - b.MaxHealth, a.RegenTime - b.RegenTime, a.WorkAmount - b.WorkAmount, a.WorkSuccessChance - b.WorkSuccessChance, a.WorkTime - b.WorkTime, a.RecallCooldown - b.RecallCooldown);
    }

    public static WorkerStats operator *(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth * b, a.RegenTime * b, a.WorkAmount * b, a.WorkSuccessChance * b, a.WorkTime * b, a.RecallCooldown * b);
    }

    public static WorkerStats operator /(WorkerStats a, float b)
    {
        return new WorkerStats(a.MaxHealth / b, a.RegenTime / b, a.WorkAmount / b, a.WorkSuccessChance / b, a.WorkTime / b, a.RecallCooldown / b);
    }

    public static WorkerStats operator -(WorkerStats a)
    {
        return new WorkerStats(-a.MaxHealth, -a.RegenTime, -a.WorkAmount, -a.WorkSuccessChance, -a.WorkTime, -a.RecallCooldown);
    }

}