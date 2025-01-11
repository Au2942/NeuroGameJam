using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LavaLampVisual : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Color maxColor;
    [SerializeField] private Color minColor; 
    
    void Update()
    {
        healthText.text = PlayerManager.Instance.Health.ToString();
        ChangeLavaLampColor();
    }

    private void ChangeLavaLampColor()
    {
        float healthPercentage = PlayerManager.Instance.Health / (float)PlayerManager.Instance.MaxHealth;
        Color updatedColor = Color.Lerp(minColor, maxColor, healthPercentage);
        lavaLampContent.color = updatedColor;
    }
}
