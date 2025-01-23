using UnityEngine;
using System.Collections.Generic;

public class SingMemory : MemoryEntity
{
    [SerializeField] private InteractableSpawner GoodNoteSpawner;
    [SerializeField] private InteractableSpawner BadNoteSpawner;
    [SerializeField] private float maxX = 600f;
    [SerializeField] private float minX = -600f;
    [SerializeField] private float maxY = 300f;
    [SerializeField] private float minY = -300f;
    [SerializeField] private float maxSpawnDuration = 30f;
    [SerializeField] private float minSpawnDuration = 5f;


    private float spawnDuration = 10f;
    private float spawnTimer = 0f;

    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();

        if (!GoodNoteSpawner.isActive)
        {
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            GoodNoteSpawner.StartSpawningInteractables(maxX, minX, maxY, minY);
        }
        else if (GoodNoteSpawner.isActive)
        {
            if (spawnTimer >= spawnDuration || GoodNoteSpawner.objectInUse.Count == 0)
            {
                spawnTimer = 0f;
                ExitGlitchState();
            }
            else spawnTimer += Time.deltaTime;
        }
    }

    public override void ExitGlitchState()
    {
        base.ExitGlitchState();
        ClearSpawner();
    }

    public void ClearSpawner()
    {
        GoodNoteSpawner.StopSpawningInteractables();
        BadNoteSpawner.StopSpawningInteractables();
        GoodNoteSpawner.ClearFallenObjects();
        BadNoteSpawner.ClearFallenObjects();
    }


}