using UnityEngine;
public abstract class WorkerStatusEffectSO : ScriptableObject
{
    public abstract WorkerStatusEffect CreateWorkerStatusEffect();
}

public class WorkerStatusEffectSO<DataType, StatusEffectType> : WorkerStatusEffectSO 
    where DataType : WorkerStatusEffectData, new()
    where StatusEffectType : WorkerStatusEffect<DataType>, new()
{
    
    public DataType data;
    public override WorkerStatusEffect CreateWorkerStatusEffect()
    {
        return new StatusEffectType { data = data };
    }
}