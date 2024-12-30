using UnityEngine;

public class ChatNeuro : MemoryEntity
{
    [SerializeField] private RainingObject rainingObject;
    [SerializeField] private float maxCooldown = 30f;
    [SerializeField] private float minCooldown = 15f;
    [SerializeField] private float maxSpawnDuration = 30f;
    [SerializeField] private float minSpawnDuration = 5f;
    private float cooldownTimer = 0f;
    private float cooldown = 0f;

    private float spawnDuration = 10f;
    private float spawnTimer = 0f;

    protected override void PhaseOneBehaviour()
    {
    }

    protected override void PhaseTwoBehaviour()
    {
        if (!rainingObject.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else if (!rainingObject.isActive)
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown);
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            rainingObject.StartRainingObjects();
        }

        if (rainingObject.isActive)
        {
            if (spawnTimer < spawnDuration)
            {
                spawnTimer += Time.deltaTime;
            }
            else
            {
                rainingObject.StopRainingObjects();
                spawnTimer = 0f;
            }
        }
    }

    protected override void PhaseThreeBehaviour()
    {
        if (!rainingObject.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else if (!rainingObject.isActive)
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown);
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            rainingObject.StartRainingObjects();
        }

        if (rainingObject.isActive)
        {
            if (spawnTimer < spawnDuration)
            {
                spawnTimer += Time.deltaTime;
            }
            else
            {
                rainingObject.StopRainingObjects();
                spawnTimer = 0f;
            }
        }
    }

    protected override void OnDayEnd()
    {
        rainingObject.StopRainingObjects();
        rainingObject.ClearFallenObjects();
    }

}