using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteController : MonoBehaviour
{
    public Volume volumeComponent;
    [SerializeField] private float vignetteIntensity = 0f;

    private Vignette vignetteVolume;

    void Start()
    {
        volumeComponent.profile.TryGet(out Vignette vignette);
        vignetteVolume = vignette;
        SetVignetteIntensity(vignetteIntensity);
    }

    public void SetVignetteIntensity(float intensity)
    {
        vignetteVolume.intensity.value = intensity;
    }

    public void FadeVignette(float startIntensity, float targetIntensity, float duration, bool unscaledTime = false)
    {
        StartCoroutine(FadingVignette(startIntensity, targetIntensity, duration, unscaledTime));
    }

    private IEnumerator FadingVignette(float startIntensity, float targetIntensity, float duration, bool unscaledTime = false)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            vignetteVolume.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, progress);
            elapsedTime += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        vignetteVolume.intensity.value = targetIntensity;
    }
}