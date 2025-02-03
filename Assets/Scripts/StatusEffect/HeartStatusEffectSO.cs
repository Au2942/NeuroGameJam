using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Status Effect/Heart")]
public class HeartStatusEffectSO: WorkerStatusEffectSO<HeartStatusEffectData, HeartStatusEffect> {}

[System.Serializable]
public class HeartStatusEffectData : WorkerStatusEffectData
{
    public bool heartBroken;
}

public class HeartStatusEffect : WorkerStatusEffect<HeartStatusEffectData>
{
    public override void OnStartMaintain()
    {
        base.OnStartMaintain();
        if(data.source != null)
        {
            if(data.target.assignedEntity != null && data.target.assignedEntity == data.source)
            {
                //guaranteed success chance
                WorkerStats bonusStats = new WorkerStats();
                bonusStats.WorkSuccessChance = Mathf.Max(data.target.workerData.TotalStats.WorkSuccessChance, 100);
                data.target.AddTempStats(bonusStats);
            }
            else
            {
                //lower success rate then remove the status effect
                data.heartBroken = true;
            }
        }

    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if(data.source == null)
        {
            Remove();
        }
    }
    public override void OnFinishMaintain()
    {
        base.OnFinishMaintain();
        if(data.heartBroken)
        {
            if(data.source is MemoryEntity memoryEntity)
            {
                data.target.TakeDamage(data.target.workerData.TotalStats.MaxHealth/2, memoryEntity);
            }
            data.ExpireNextUpdate = true;
        }
    }
}