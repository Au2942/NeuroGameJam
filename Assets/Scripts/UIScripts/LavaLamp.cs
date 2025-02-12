using UnityEngine;
using UnityEngine.UI;


public class LavaLamp : MonoBehaviour
{
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Material lavaLampMaterial;
    [SerializeField] private float lavaLampBaseSpeed = 1;
    [SerializeField] private Color maxLampColor;
    [SerializeField] private Color minLampColor; 
    [SerializeField] private TooltipTrigger tooltipTrigger;
    private Material tempMaterial;

    private System.Action<float> HealthChangedDelegate;

    void Awake()
    {
        HealthChangedDelegate = (t) => UpdateLavaLamp();
    }
    void OnEnable()
    {
        PlayerManager.Instance.OnHealthChangedEvent += HealthChangedDelegate;
    }
    void Start()
    {
        tempMaterial = new Material(lavaLampMaterial);
        lavaLampContent.material = tempMaterial;
        UpdateLavaLamp();
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
        tooltipTrigger.SetTooltipContent((PlayerManager.Instance.Health/PlayerManager.Instance.MaxHealth).ToString("P0"));

        lavaLampContent.materialForRendering.SetFloat("_Speed", lavaLampBaseSpeed * healthPercentage);
        lavaLampContent.materialForRendering.SetColor("_CoreColor", lerpedColor);
        lavaLampContent.materialForRendering.SetColor("_GlowColor", GetGlowColor(lerpedColor));
    }

    void OnDisable()
    {
        PlayerManager.Instance.OnHealthChangedEvent -= HealthChangedDelegate;
    }

    void OnDestroy()
    {
        Destroy(tempMaterial);
    }
   
 
}
