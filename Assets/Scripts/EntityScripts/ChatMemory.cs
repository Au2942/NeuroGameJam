using UnityEngine;

public class ChatMemory : MemoryEntity
{
    [SerializeField] private InteractableSpawner wordSpawner;
    // [SerializeField] private float maxSpawnDuration = 30f;
    // [SerializeField] private float minSpawnDuration = 5f;


    protected override void GlitchBehavior()
    {
        base.GlitchBehavior();

        if (!wordSpawner.isActive)
        {
            wordSpawner.StartSpawningInteractables();
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