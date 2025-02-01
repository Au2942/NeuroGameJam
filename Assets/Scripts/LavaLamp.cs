using UnityEngine;
using UnityEngine.UI;


public class LavaLamp : MonoBehaviour
{
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Material lavaLampMaterial;
    [SerializeField] private float lavaLampBaseSpeed = 1;
    [SerializeField] private Color maxLampColor;
    [SerializeField] private Color minLampColor; 
    private Material tempMaterial;

    private System.Action<float> HealthChangedEventHandler;

    void Awake()
    {
        HealthChangedEventHandler = (t) => UpdateLavaLamp();
    }
    void OnEnable()
    {
        PlayerManager.Instance.OnHealthChangedEvent += HealthChangedEventHandler;
    }
    void Start()
    {
        tempMaterial = new Material(lavaLampMaterial);
        lavaLampContent.material = tempMaterial;

        lavaLampContent.materialForRendering.SetFloat("_Speed", lavaLampBaseSpeed);
        lavaLampContent.materialForRendering.SetColor("_CoreColor", maxLampColor);
        lavaLampContent.materialForRendering.SetColor("_GlowColor", GetGlowColor(maxLampColor));
    }


    private Color GetGlowColor(Color coreColor)
    {
        Color.RGBToHSV(coreColor, out float h, out float s, out float v);
        h = h > 0.5f ? h + 0.05f : h - 0.05f;
        Color glowColor = Color.HSVToRGB(h, s, v * 0.8f);
        return glowColor;
    }

    private void UpdateLavaLamp()
    {
        float healthPercentage = PlayerManager.Instance.Health / PlayerManager.Instance.MaxHealth;
        Color lerpedColor = Color.Lerp(minLampColor, maxLampColor, healthPercentage);

        lavaLampContent.materialForRendering.SetFloat("_Speed", lavaLampBaseSpeed * healthPercentage);
        lavaLampContent.materialForRendering.SetColor("_CoreColor", lerpedColor);
        lavaLampContent.materialForRendering.SetColor("_GlowColor", GetGlowColor(lerpedColor));
    }

    void OnDisable()
    {
        PlayerManager.Instance.OnHealthChangedEvent -= HealthChangedEventHandler;
    }

    void OnDestroy()
    {
        Destroy(tempMaterial);
    }
   
 
}
