using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public InteractableSpawner InteractableSpawner { get; set; }

    public virtual void OnSpawn()
    {
        
    }

    public virtual void ReturnToPool()
    {
        Debug.Log("Button Clicked");
        InteractableSpawner.ReturnObjectToPool(gameObject);
    }
}
