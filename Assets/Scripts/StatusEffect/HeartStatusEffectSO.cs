using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Status Effect/Heart")]
public class HeartStatusEffectSO: WorkerStatusEffectFactory<HeartStatusEffectData, HeartStatusEffect> {}

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
        if(source != null)
        {
            if(target.assignedEntity != null && target.assignedEntity == source)
            {
                //guaranteed success chance
                WorkerStats bonusStats = new WorkerStats();
                bonusStats.WorkSuccessChance = Mathf.Max(target.GetTotalStats().WorkSuccessChance, 100);
                target.AddTempStats(bonusStats);
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
        if(source == null)
        {
            Remove();
        }
    }
    public override void OnFinishMaintain()
    {
        base.OnFinishMaintain();
        if(data.heartBroken)
        {
            Remove();
        }
    }
}