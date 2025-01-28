using UnityEngine;

public class HeartMemory : MemoryEntity
{
    [SerializeField] private HeartStatusEffectSO heartStatusEffectSO;
    [SerializeField] private InteractableSpawner heartSpawner;
    [SerializeField] private float maxX = 600f;
    [SerializeField] private float minX = -600f;
    [SerializeField] private float maxY = 300f;
    [SerializeField] private float minY = -300f;


    private float damageCooldown = 2f;
    private float damageTimer = 0f;

    
    public override void ExitGlitchState()
    {
        base.ExitGlitchState();
        ClearSpawner();
    }

    private void ClearSpawner()
    {
        heartSpawner.StopSpawningInteractables();
        heartSpawner.ClearFallenObjects();    
    }

    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();
        // if(heartSpawner.isActive)
        // {
        //     if(damageTimer >= damageCooldown)
        //     {
        //         PlayerManager.Instance.TakeDamage(1);
        //         damageTimer = 0f;
        //     }
        //     damageTimer += Time.deltaTime;
        //     if(heartSpawner.objectInUse.Count == 0)
        //     {
        //         ExitGlitchState();
        //     }
        // }
        // else heartSpawner.StartSpawningInteractables(minX, maxX, minY, maxY);
    }

    public override void MaintainSuccess(Worker worker)
    {
        base.MaintainSuccess(worker);
        WorkerStatusEffect heartStatusEffect = heartStatusEffectSO.CreateWorkerStatusEffect();
        worker.ApplyStatusEffect(heartStatusEffect, this);
    }

}
