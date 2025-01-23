using UnityEngine;
using UnityEngine.Rendering.Universal;


public class ScreenEffectController : MonoBehaviour
{
    [SerializeField] ScreenEffectRendererFeature screenEffectRendererFeature;
    [SerializeField][Range(0f,1f)] private float noiseStrength = 1f;
    [SerializeField][Range(0f,100f)] private float noiseAmount = 50f;
    [SerializeField][Range(0f,1f)] private float noiseIntensity = 0.1f;
    [SerializeField][Range(0f,1f)] private float scanlineStrength = 1f;
    [SerializeField][Range(0f,1000f)] private float scanlineAmount = 600f;


    void Start()
    {
        screenEffectRendererFeature.SetActive(false);
    }
    public void Show()
    {
        screenEffectRendererFeature.SetActive(true);
    }
    public void Hide()
    {
        screenEffectRendererFeature.SetActive(false);
    }

    public void SetSettingsValue()
    {
        ScreenEffectsSettings settings = new ScreenEffectsSettings(
            noiseStrength,
            noiseAmount,
            noiseIntensity,
            scanlineStrength,
            scanlineAmount
        );
        screenEffectRendererFeature.settings = settings;
    }
}
