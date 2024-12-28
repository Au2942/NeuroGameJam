using UnityEngine;
using TMPro;

public class LavaLampVisual : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    
    void Update()
    {
        healthText.text = (PlayerManager.Instance.health/100).ToString();
    }
}
