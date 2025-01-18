using UnityEngine;
using System.Collections.Generic;

public class OverlayEffectManager : MonoBehaviour
{
    public static OverlayEffectManager Instance { get; private set; }
    [SerializeField] private GlitchOverlay overlayEffectPrefab;
    [SerializeField] private int initialPoolSize = 10;
    public Queue<GlitchOverlay> overlayEffectPool = new Queue<GlitchOverlay>();
    public List<GlitchOverlay> activeOverlayEffects = new List<GlitchOverlay>();


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

    void Start()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GlitchOverlay overlayEffect = Instantiate(overlayEffectPrefab);
            overlayEffect.gameObject.SetActive(false);
            overlayEffectPool.Enqueue(overlayEffect);
        }
    }

    public GlitchOverlay AddOverlayEffect(Vector3 position, Transform parent)
    {
        GlitchOverlay overlayEffect;
        if (overlayEffectPool.Count > 0)
        {
            overlayEffect = overlayEffectPool.Dequeue();
            overlayEffect.transform.position = position;
            overlayEffect.transform.SetParent(parent);
            overlayEffect.gameObject.SetActive(true);
        }
        else
        {
            overlayEffect = Instantiate(overlayEffectPrefab, position, Quaternion.identity, parent);
        }
        activeOverlayEffects.Add(overlayEffect);
        return overlayEffect;
    }

    public GlitchOverlay AddOverlayEffect(Transform parent)
    {
        GlitchOverlay overlayEffect;
        if (overlayEffectPool.Count > 0)
        {
            overlayEffect = overlayEffectPool.Dequeue();
            overlayEffect.transform.SetParent(parent);
            overlayEffect.gameObject.SetActive(true);
        }
        else
        {
            overlayEffect = Instantiate(overlayEffectPrefab, parent);
        }
        activeOverlayEffects.Add(overlayEffect);
        return overlayEffect;
    }

    public void RemoveOverlayEffect(GlitchOverlay overlayEffect)
    {
        activeOverlayEffects.Remove(overlayEffect);
        overlayEffect.gameObject.SetActive(false);
        overlayEffectPool.Enqueue(overlayEffect);
    }
}
