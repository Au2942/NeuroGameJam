using UnityEngine.Rendering;
using UnityEngine;


public class ScreenEffectController : MonoBehaviour
{
    [SerializeField] Volume volumeComponent;

    private ScreenEffectVolumeComponent screenEffectVolumeComponent;
    void Start()
    {
        volumeComponent.profile.TryGet(out screenEffectVolumeComponent);
        Hide();
    }
    public void Show()
    {
        screenEffectVolumeComponent.intensity.value = 1;
    }
    public void Hide()
    {
        screenEffectVolumeComponent.intensity.value = 0;
    }

    public void SetScreenEffectSettings(float noiseStrength, float noiseAmount, float noiseIntensity, float scanlineStrength, float scanlineAmount)
    {
        screenEffectVolumeComponent.noiseStrength.value = noiseStrength;
        screenEffectVolumeComponent.noiseAmount.value = noiseAmount;
        screenEffectVolumeComponent.noiseIntensity.value = noiseIntensity;
        screenEffectVolumeComponent.scanlineStrength.value = scanlineStrength;
        screenEffectVolumeComponent.scanlineAmount.value = scanlineAmount;
    }

}
