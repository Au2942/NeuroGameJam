using UnityEngine;
using TMPro;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    void Update()
    {
        healthText.text = PlayerManager.Instance.Health.ToString();
        scoreText.text = "viewers: " + PlayerManager.Instance.Score.ToString();
    }
}
