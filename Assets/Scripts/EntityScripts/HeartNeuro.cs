using UnityEngine;

public class HeartNeuro : MemoryEntity
{
    [SerializeField] private InteractableSpawner heartSpawner;
    [SerializeField] private float maxX = 600f;
    [SerializeField] private float minX = -600f;
    [SerializeField] private float maxY = 300f;
    [SerializeField] private float minY = -300f;


    private float damageCooldown = 2f;
    private float damageTimer = 0f;

    
    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        ClearSpawner();
    }

    private void ClearSpawner()
    {
        heartSpawner.StopSpawningInteractables();
        heartSpawner.ClearFallenObjects();    
    }

    protected override void CorruptBehavior()
    {
        base.CorruptBehavior();
        if(heartSpawner.isActive)
        {
            if(damageTimer >= damageCooldown)
            {
                PlayerManager.Instance.TakeDamage(1);
                damageTimer = 0f;
            }
            damageTimer += Time.deltaTime;
            if(heartSpawner.objectInUse.Count == 0)
            {
                ExitCorruptState();
            }
        }
        else heartSpawner.StartSpawningInteractables(minX, maxX, minY, maxY);
        
    }

}
