using UnityEngine;

public class ChatMemory : MemoryEntity
{
    [SerializeField] private InteractableSpawner wordSpawner;
    [SerializeField] private float maxSpawnDuration = 30f;
    [SerializeField] private float minSpawnDuration = 5f;

    private float spawnDuration = 10f;
    private float spawnTimer = 0f;


    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();

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
    private void ClearSpawner()
    {
        wordSpawner.StopSpawningInteractables();
        wordSpawner.ClearFallenObjects();    
    }

}