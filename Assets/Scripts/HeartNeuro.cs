using UnityEngine;

public class HeartNeuro : MemoryEntity
{
    [SerializeField] private RainingObject rainingObject;
    [SerializeField] private float maxCooldown = 60f;
    [SerializeField] private float minCooldown = 20f;
    private float cooldownTimer = 0f;
    private float cooldown = 0f;
    private float damageCooldown = 2f;
    private float damageTimer = 0f;

    protected override void PhaseOneBehaviour()
    {
    }

    protected override void PhaseTwoBehaviour()
    {
        if(!rainingObject.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown);
        }
        if(rainingObject.isActive)
        {
            if(damageTimer < damageCooldown)
            {
                damageTimer += Time.deltaTime;
                return;
            }
            else
            {
                PlayerManager.Instance.TakeDamage(1);
                damageTimer = 0f;
            }
        }
        else rainingObject.StartRainingObjects(-600f, 600 , -300, 300);
    }

    protected override void PhaseThreeBehaviour()
    {
        if(!rainingObject.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown/2)/2;
        }
        if(rainingObject.isActive)
        {
            if(damageTimer < damageCooldown/2f)
            {
                damageTimer += Time.deltaTime;
                return;
            }
            else
            {
                PlayerManager.Instance.TakeDamage(1);
                damageTimer = 0f;
            }
        }
        else rainingObject.StartRainingObjects();
    }

    protected override void OnDayEnd()
    {
        rainingObject.StopRainingObjects();
        rainingObject.ClearFallenObjects();
    }

}