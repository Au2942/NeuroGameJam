using UnityEngine.Rendering;

[VolumeComponentMenu("Custom/ScreenEffectVolumeComponent")]
public class ScreenEffectVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f, true);
    public ClampedFloatParameter noiseStrength = new ClampedFloatParameter(1f, 0f, 1f, true);
    public ClampedFloatParameter noiseAmount = new ClampedFloatParameter(50f, 0f, 100f, true);
    public ClampedFloatParameter noiseIntensity = new ClampedFloatParameter(0.1f, 0f, 1f, true);
    public ClampedFloatParameter scanlineStrength = new ClampedFloatParameter(1f, 0f, 1f, true);
    public ClampedFloatParameter scanlineAmount = new ClampedFloatParameter(600f, 0f, 1000f, true);
    public bool IsActive()
    {
        return intensity.value > 0f;
    }
}