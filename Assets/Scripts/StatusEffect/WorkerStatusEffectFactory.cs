using UnityEngine;
public abstract class WorkerStatusEffectFactory : ScriptableObject
{
    public abstract WorkerStatusEffect CreateWorkerStatusEffect();
}

public class WorkerStatusEffectFactory<DataType, StatusEffectType> : WorkerStatusEffectFactory 
    where StatusEffectType : WorkerStatusEffect<DataType>, new()
    where DataType : WorkerStatusEffectData, new()
{
    public DataType data;
    public override WorkerStatusEffect CreateWorkerStatusEffect()
    {
        return new StatusEffectType { data = data };
    }
}