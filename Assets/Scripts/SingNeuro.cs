using UnityEngine;
using System.Collections.Generic;

public class SingNeuro : MemoryEntity
{
    [SerializeField] private RainingObject rainingObjectGood;
    [SerializeField] private RainingObject rainingObjectBad;
    [SerializeField] private float maxX = 600f;
    [SerializeField] private float minX = -600f;
    [SerializeField] private float maxY = 300f;
    [SerializeField] private float minY = -300f;
    [SerializeField] private float maxCooldown = 60f;
    [SerializeField] private float minCooldown = 20f;
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
        if (!rainingObjectGood.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else if (!rainingObjectGood.isActive)
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown);
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            rainingObjectGood.StartRainingObjects(maxX, minX, maxY, minY);
        }

        if (rainingObjectGood.isActive)
        {
            if (spawnTimer < spawnDuration)
            {
                spawnTimer += Time.deltaTime;
            }
            else
            {
                rainingObjectGood.StopRainingObjects();
                spawnTimer = 0f;
            }
        }
    }

    protected override void PreparePhaseOne()
    {
        base.PreparePhaseOne();
        rainingObjectGood.StopRainingObjects();
        rainingObjectBad.StopRainingObjects();
    }
    protected override void PreparePhaseTwo()
    {
        base.PreparePhaseThree();
        rainingObjectBad.StopRainingObjects();
    }

    protected override void PreparePhaseThree()
    {
        base.PreparePhaseThree();
        rainingObjectGood.StopRainingObjects();
    }

    //rainObjectBad will have an effect where if you cant clear it before spawnDuration end 
    //then a random memoryentity decay rate will be increased

    protected override void PhaseThreeBehaviour()
    {
        if (!rainingObjectBad.isActive && cooldownTimer < cooldown)
        {
            cooldownTimer += Time.deltaTime;
            return;
        }
        else if (!rainingObjectBad.isActive)
        {
            cooldownTimer = 0f;
            cooldown = Random.Range(minCooldown, maxCooldown);
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            rainingObjectBad.StartRainingObjects(maxX, minX, maxY, minY);
        }

        if (spawnTimer < spawnDuration)
        {
            if(rainingObjectBad.isActive)
            {
                spawnTimer += Time.deltaTime;
            }
            else
            {
                rainingObjectBad.StopRainingObjects();
                spawnTimer = 0f;
            }
        }
        else
        {
            //get a copy of the memory entity excluding self
            List<MemoryEntity> memoryEntities = new List<MemoryEntity>(GameManager.Instance.MemoryEntities).FindAll(entity => entity != this);
            int randomIndex = Random.Range(0, memoryEntities.Count);

            memoryEntities[randomIndex].AddIntegrity(-10);
            rainingObjectBad.StopRainingObjects();
            spawnTimer = 0f;
        }
    }

    protected override void OnDayEnd()
    {
        rainingObjectGood.StopRainingObjects();
        rainingObjectGood.ClearFallenObjects();
    }

}