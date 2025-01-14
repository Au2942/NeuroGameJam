using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI viewersText;
    [SerializeField] private TextMeshProUGUI subscribersText;

    [SerializeField] private TextMeshProUGUI remainingStreamTime;
    [SerializeField] private TextMeshProUGUI elapsedStreamTime;
    
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Color maxLampColor;
    [SerializeField] private Color minLampColor; 
    
    [SerializeField] public float timeMultiplier = 60f;

    [SerializeField] private Image integrityBar;
    private float targetIntegrityBarValue = 0f;

    void Start()
    {
        StartCoroutine(MoveIntegrityBar());
    }


    private void ChangeLavaLampColor()
    {
        float healthPercentage = PlayerManager.Instance.MemoriesIntegrity / PlayerManager.Instance.MaxMemoriesIntegrity;
        Color updatedColor = Color.Lerp(minLampColor, maxLampColor, healthPercentage);
        lavaLampContent.color = updatedColor;
    }
    
    void Update()
    {
        healthText.text = PlayerManager.Instance.MemoriesIntegrity.ToString("F0");
        viewersText.text = PlayerManager.Instance.CurrentViewers.ToString();
        subscribersText.text = "SUBS: " + PlayerManager.Instance.Subscriptions.ToString();
        remainingStreamTime.text = FloatToTimeString(PlayerManager.Instance.RemainingStreamTime*timeMultiplier);
        elapsedStreamTime.text = FloatToTimeString(PlayerManager.Instance.ElapsedStreamTime*timeMultiplier);

        ChangeLavaLampColor();
        UpdateIntegrityBar();
    }

    private string FloatToTimeString(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600F);
        int minutes = Mathf.FloorToInt((timeInSeconds - hours * 3600) / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - hours * 3600 - minutes * 60);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds); 
    }
    private void UpdateIntegrityBar()
    {
        int index = TimelineManager.Instance.currentEntityIndex;
        targetIntegrityBarValue = GameManager.Instance.Entities[index].Integrity/GameManager.Instance.Entities[index].MaxIntegrity;
    }

    private IEnumerator MoveIntegrityBar()
    {
        while (true)
        {
            integrityBar.fillAmount = Mathf.MoveTowards(integrityBar.fillAmount, targetIntegrityBarValue, Time.deltaTime);
            yield return null;
        }
    }
}
