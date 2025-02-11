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
        if(Source != null && Source is MemoryEntity entity)
        {
            if(Target.assignedEntity != null && Target.assignedEntity != entity)
            {
                Data.heartBroken = true;
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if(Source == null)
        {
            Data.ExpireNextUpdate = true;
        }
    }
    public override void OnFinishMaintain()
    {
        base.OnFinishMaintain();
        if(Data.heartBroken)
        {
            if(Source is MemoryEntity memoryEntity)
            {
                Target.TakeDamage(memoryEntity.AttackDamage*Data.Stack, DamageType.True ,memoryEntity);
            }
            Data.ExpireNextUpdate = true;
        }
    }
}