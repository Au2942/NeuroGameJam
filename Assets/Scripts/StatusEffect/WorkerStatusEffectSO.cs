public abstract class WorkerStatusEffectSO: StatusEffectSO
{
    public override StatusEffect CreateStatusEffect()
    {
        return CreateWorkerStatusEffect();
    }
    public abstract WorkerStatusEffect CreateWorkerStatusEffect();
}

public class WorkerStatusEffectSO<DataType, WorkerStatusEffectType> : WorkerStatusEffectSO
    where DataType : WorkerStatusEffectData, new()
    where WorkerStatusEffectType : WorkerStatusEffect<DataType>, new()
{
    public DataType data;
    public override WorkerStatusEffect CreateWorkerStatusEffect()
    {
        return new WorkerStatusEffectType { Data = data };
    }
}