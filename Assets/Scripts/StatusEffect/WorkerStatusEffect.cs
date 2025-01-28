using UnityEngine;
public abstract class WorkerStatusEffect 
{
    public abstract void Apply(Entity source,Worker target);
    public abstract void OnUpdate(float deltaTime);
    public abstract bool ShouldExpire();
    
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
    public abstract void Remove();
}

[System.Serializable]
public class WorkerStatusEffectData
{
    public WorkerAttributes BuffAttributes;
    public WorkerStats BuffStats;
    public bool ExpireAfterLifetime = true;
    public float ModifierLifetime;
}

public abstract class WorkerStatusEffect<DataType> : WorkerStatusEffect where DataType : WorkerStatusEffectData
{
    public DataType data;
    public Entity source;
    public Worker target;

    public override void Apply(Entity entity, Worker worker)
    {
        source = entity;
        target = worker;
        target.AddTempAttributes(data.BuffAttributes);
        worker.AddTempStats(data.BuffStats);
    }

    public override void OnUpdate(float deltaTime)
    {
        //ModifierLifetime -= deltaTime;
    }

    public override bool ShouldExpire()
    {
        return data.ExpireAfterLifetime && data.ModifierLifetime <= 0;
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

    public override void Remove()
    {
        target.AddTempAttributes(-data.BuffAttributes);
        target.AddTempStats(-data.BuffStats);
        target.RemoveStatusEffect(this);
    } 
}