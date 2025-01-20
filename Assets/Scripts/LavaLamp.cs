using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LavaLamp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Color maxLampColor;
    [SerializeField] private Color minLampColor; 


    private void ChangeLavaLampColor()
    {
        float healthPercentage = PlayerManager.Instance.Health / PlayerManager.Instance.MaxHealth;
        Color updatedColor = Color.Lerp(minLampColor, maxLampColor, healthPercentage);
        lavaLampContent.color = updatedColor;
    }
    
    void Update()
    {
        healthText.text = PlayerManager.Instance.Health.ToString("F0");
        ChangeLavaLampColor();
    }
   
 
}
