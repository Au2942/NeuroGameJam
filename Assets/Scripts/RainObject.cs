using UnityEngine;

public abstract class RainObject : MonoBehaviour
{
    public RainingObject RainingObject { get; set; }

    public virtual void OnSpawn()
    {
        
    }

    public virtual void ReturnToPool()
    {
        Debug.Log("Button Clicked");
        RainingObject.ReturnObjectToPool(gameObject);
    }
}
