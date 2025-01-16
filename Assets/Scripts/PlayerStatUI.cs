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
    [SerializeField] private int popupNumberMax = 20;
    [SerializeField] private float popupNumberDuration = 2f;
    
    [SerializeField] private Image lavaLampContent;
    [SerializeField] private Color maxLampColor;
    [SerializeField] private Color minLampColor; 
    
    [SerializeField] public float timeMultiplier = 60f;

    [SerializeField] private Image integrityBar;
    private float targetIntegrityBarValue = 0f;

    void Start()
    {
        StartCoroutine(UpdateIntegrityBar());
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
        UpdateIntegrityBarTargetValue();
    }

    private string FloatToTimeString(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600F);
        int minutes = Mathf.FloorToInt((timeInSeconds - hours * 3600) / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - hours * 3600 - minutes * 60);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds); 
    }
    private void UpdateIntegrityBarTargetValue()
    {
        int index = ChannelNavigationManager.Instance.CurrentChannelIndex;
        targetIntegrityBarValue = LivefeedManager.Instance.Livefeeds[index].integrityBar.fillAmount;
        integrityBar.color = LivefeedManager.Instance.Livefeeds[index].integrityBar.color;
    }

    private IEnumerator UpdateIntegrityBar()
    {
        while (true)
        {
            integrityBar.fillAmount = Mathf.MoveTowards(integrityBar.fillAmount, targetIntegrityBarValue, Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator SpawnSubPopupNumber(int times)
    {
        int amount = Mathf.Min(times, popupNumberMax);
        float rangeX = remainingStreamTime.textBounds.size.x;
        float rangeY = remainingStreamTime.textBounds.size.y;
        for(int i = 0; i < amount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), 0);
            Vector3 worldPosition = remainingStreamTime.rectTransform.TransformPoint(remainingStreamTime.textBounds.center + randomOffset);
            PopupTextSpawner.Instance.SpawnPopupText(remainingStreamTime.transform ,worldPosition, "+" + Mathf.FloorToInt(PlayerManager.Instance.StreamTimeIncrease*timeMultiplier));
            yield return new WaitForSeconds(popupNumberDuration/amount);
        }
    }
}
