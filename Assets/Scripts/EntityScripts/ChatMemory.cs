using UnityEngine;

public class ChatMemory : MemoryEntity
{
    [SerializeField] private InteractableSpawner wordSpawner;
    [SerializeField] private float maxSpawnDuration = 30f;
    [SerializeField] private float minSpawnDuration = 5f;

    private float spawnDuration = 10f;
    private float spawnTimer = 0f;


    protected override void CorruptBehavior()
    {
        base.CorruptBehavior();

        if (!wordSpawner.isActive)
        {
            spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
            wordSpawner.StartSpawningInteractables();
        }

        if (wordSpawner.isActive)
        {
            if (spawnTimer >= spawnDuration || wordSpawner.objectInUse.Count == 0)
            {
                wordSpawner.StopSpawningInteractables();
                spawnTimer = 0f;
                ExitCorruptState();
            }
            else spawnTimer += Time.deltaTime;
        }
    }

    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        ClearSpawner();
    }
    private void ClearSpawner()
    {
        wordSpawner.StopSpawningInteractables();
        wordSpawner.ClearFallenObjects();    
    }

}