using UnityEngine;
using TMPro;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    void Update()
    {
        healthText.text = (PlayerManager.Instance.health/100).ToString();
        scoreText.text = "viewers: " + PlayerManager.Instance.score.ToString();
    }
}
