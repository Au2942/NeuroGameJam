using UnityEngine.Rendering;

[VolumeComponentMenu("Custom/ScreenEffectVolumeComponent")]
public class ScreenEffectVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f, true);
    public ClampedFloatParameter noiseStrength = new ClampedFloatParameter(1f, 0f, 1f, true);
    public ClampedFloatParameter noiseAmount = new ClampedFloatParameter(50f, 0f, 100f, true);
    public ClampedFloatParameter noiseIntensity = new ClampedFloatParameter(0.1f, 0f, 1f, true);
    public ClampedFloatParameter scanlineStrength = new ClampedFloatParameter(1f, 0f, 1f, true);
    public ClampedIntParameter scanlineAmount = new ClampedIntParameter(600, 0, 1000, true);
    public bool IsActive()
    {
        return intensity.value > 0f;
    }
}