using UnityEngine;
using UnityEngine.UI;

public class IntegrityIndicator : MonoBehaviour
{
    [SerializeField] private Image indicator;
    [SerializeField] private MemoryEntity entity; 
    private Color originalColor;

    void Start()
    {
        originalColor = indicator.color;
    }
    void Update()
    {
        float integrityPercentage = entity.Integrity / (float)entity.MaxIntegrity;
        Color.RGBToHSV(originalColor, out float h, out float s, out float v);
        integrityPercentage = Mathf.Clamp(integrityPercentage, 0, v);
        Color updatedColor = Color.HSVToRGB(h, s, integrityPercentage);
        indicator.color = updatedColor;
    }
}