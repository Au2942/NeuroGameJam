using UnityEngine;

public abstract class WorkerStatusEffect : StatusEffect
{
    public abstract void OnStartWork();
    public abstract void OnWorkSuccess();
    public abstract void OnWorkFail();
    public abstract void OnFinishWork();
    public abstract void OnStartMaintain();
    public abstract void OnMaintainSuccess();
    public abstract void OnMaintainFail();
    public abstract void OnFinishMaintain();
    public abstract void OnStartRepair();
    public abstract void OnRepairSuccess();
    public abstract void OnRepairFail();
    public abstract void OnFinishRepair();
}

public class WorkerStatusEffectData : StatusEffectData
{
    public WorkerAttributes BuffAttributes;
    public WorkerStats BuffStats;
}


public abstract class WorkerStatusEffect<DataType> : WorkerStatusEffect 
    where DataType : WorkerStatusEffectData, new()
{
    public DataType Data;
    public Worker Target;
    public IStatusEffectSource Source;
    public override void Apply(IStatusEffectable target, IStatusEffectSource source = null)
    {
        if (target is Worker workerTarget)
        {
            Apply(workerTarget, source);
        }
    }

    public override StatusEffectData GetData()
    {
        return Data;
    }
    public override IStatusEffectable GetTarget()
    {
        return Target;
    }


    public override IStatusEffectSource GetSource()
    {
        return Source;
    }


    public virtual void Apply(Worker target, IStatusEffectSource source = null)
    {
        Target = target;
        Source = source;

        Target.AddTempAttributes(Data.BuffAttributes);
        Target.AddTempStats(Data.BuffStats);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Data.ExpireAfterLifetime)
        {
            Data.Lifetime -= deltaTime;
            if (Data.Lifetime <= 0)
            {
                Data.ExpireNextUpdate = true;
            }
        }
    }

    public override bool TryAddStack(int stack)
    {
        if (Data.Stack < Data.MaxStack)
        {
            int stackToApply = Mathf.Min(stack, Data.MaxStack - Data.Stack);
            Data.Stack += stackToApply;
            for(int i = 0; i < stackToApply; i++)
            {
                Target.AddTempAttributes(Data.BuffAttributes);
                Target.AddTempStats(Data.BuffStats);
            }
            return true;
        }
        return false;
    }

    public override bool ShouldExpire()
    {
        return Data.ExpireNextUpdate;
    }
    public override void Expire()
    {
        if(Data.Stack > 0)
        {
            Data.Stack--;
        }
        Target.AddTempAttributes(-Data.BuffAttributes);
        Target.AddTempStats(-Data.BuffStats);
    } 

    public override void OnStartWork() {}
    public override void OnWorkSuccess() {}
    public override void OnWorkFail() {}
    public override void OnFinishWork() {}

    public override void OnStartMaintain() {}
    public override void OnMaintainSuccess() {}
    public override void OnMaintainFail() {}
    public override void OnFinishMaintain() {}

    public override void OnStartRepair() {}
    public override void OnRepairSuccess() {}
    public override void OnRepairFail() {}
    public override void OnFinishRepair() {}

}




