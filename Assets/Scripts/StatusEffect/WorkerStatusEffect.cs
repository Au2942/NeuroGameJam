using UnityEngine;

public abstract class WorkerStatusEffect 
{
    public abstract WorkerStatusEffectData GetData();
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

public class WorkerStatusEffectData
{
    public Entity source;
    public Worker target;
    public string ID;
    public Sprite Icon;
    public string Name;
    public string Description;
    public WorkerAttributes BuffAttributes;
    public WorkerStats BuffStats;
    public bool ExpireAfterLifetime = true;
    public float ModifierLifetime;
    public bool ExpireNextUpdate = false;
}


public abstract class WorkerStatusEffect<DataType> : WorkerStatusEffect where DataType : WorkerStatusEffectData, new()
{
    public DataType data;

    public override WorkerStatusEffectData GetData()
    {
        return data;
    }

    public override void Apply(Entity entity, Worker worker)
    {
        data.source = entity;
        data.target = worker;
        data.target.AddTempAttributes(data.BuffAttributes);
        worker.AddTempStats(data.BuffStats);
    }

    public override void OnUpdate(float deltaTime)
    {
        //ModifierLifetime -= deltaTime;
    }

    public override bool ShouldExpire()
    {
        return (data.ExpireAfterLifetime && data.ModifierLifetime <= 0) || data.ExpireNextUpdate;
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
        data.target.AddTempAttributes(-data.BuffAttributes);
        data.target.AddTempStats(-data.BuffStats);
        data.target.RemoveStatusEffect(this);
    } 
}




