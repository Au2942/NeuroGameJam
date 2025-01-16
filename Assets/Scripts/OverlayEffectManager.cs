using UnityEngine;
using System.Collections.Generic;

public class OverlayEffectManager : MonoBehaviour
{
    public static OverlayEffectManager Instance { get; private set; }
    [SerializeField] private GameObject overlayEffectPrefab;
    public List<GameObject> activeOverlayEffects = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject AddOverlayEffect(Vector3 position, Transform parent)
    {
        GameObject overlayEffect = Instantiate(overlayEffectPrefab, position, Quaternion.identity, parent);
        activeOverlayEffects.Add(overlayEffect);
        return overlayEffect;
    }
    public GameObject AddOverlayEffect(Transform parent)
    {
        GameObject overlayEffect = Instantiate(overlayEffectPrefab, parent);
        activeOverlayEffects.Add(overlayEffect);
        return overlayEffect;
    }

    public void RemoveOverlayEffect(GameObject overlayEffect)
    {
        activeOverlayEffects.Remove(overlayEffect);
        Destroy(overlayEffect);
    }
}
