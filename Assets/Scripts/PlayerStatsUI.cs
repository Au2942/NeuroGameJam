using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerDetailsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI viewersText;
    [SerializeField] private TextMeshProUGUI subscribersText;
    [SerializeField] private TextMeshProUGUI streamNameText;
    [SerializeField] private TextMeshProUGUI remainingStreamTime;
    [SerializeField] private TextMeshProUGUI elapsedStreamTime;
    [SerializeField] private float syncRemainingStreamTimeInterval = 10f;
    [SerializeField] private int popupNumberMax = 20;
    [SerializeField] private float popupNumberDuration = 2f;
    
    private float timeMultiplier => TimescaleManager.Instance.displayTimeMultiplier;

    private float DisplayRemainingStreamTime = 0f;

    void Start()
    {
        PlayerManager.Instance.OnChangeStreamName += (t) => UpdateStreamNameText(t);
        DisplayRemainingStreamTime = PlayerManager.Instance.RemainingStreamTime;
        StartCoroutine(PeriodicValueSync());
    }


    
    void Update()
    {
        viewersText.text = "<sprite name=\"viewers\">" + PlayerManager.Instance.CurrentViewers.ToString();
        subscribersText.text = "SUBS: " + PlayerManager.Instance.Subscriptions.ToString();
        DisplayRemainingStreamTime -= Time.deltaTime;
        if(DisplayRemainingStreamTime <= 0)
        {
            ForceSyncRemainingStreamTime();
        }
        remainingStreamTime.text = FloatToTimeString(DisplayRemainingStreamTime*timeMultiplier);
        elapsedStreamTime.text = FloatToTimeString(PlayerManager.Instance.ElapsedStreamTime*timeMultiplier);

    }

    private void UpdateStreamNameText(string streamName)
    {
        streamNameText.text = streamName;
    }
   
    private IEnumerator PeriodicValueSync()
    {
        while(true)
        {
            DisplayRemainingStreamTime = PlayerManager.Instance.RemainingStreamTime;
            yield return new WaitForSeconds(syncRemainingStreamTimeInterval);
        }
    } 


    public void ForceSyncRemainingStreamTime()
    {
        DisplayRemainingStreamTime = PlayerManager.Instance.RemainingStreamTime;
    }

    private string FloatToTimeString(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600F);
        int minutes = Mathf.FloorToInt((timeInSeconds - hours * 3600) / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - hours * 3600 - minutes * 60);
        return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00"); 
    }

    public IEnumerator SpawnSubPopupNumber(int times)
    {
        int amount = Mathf.Min(times, popupNumberMax);
        Vector2 rangeMin = remainingStreamTime.textBounds.min;
        Vector2 rangeMax = remainingStreamTime.textBounds.max;
        for(int i = 0; i < amount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(rangeMin.x, rangeMax.x), Random.Range(rangeMin.y, rangeMax.y), 0);
            Vector3 worldPosition = remainingStreamTime.rectTransform.TransformPoint(randomOffset);
            PopupTextSpawner.Instance.SpawnPopupText(remainingStreamTime.transform ,worldPosition, "+" + Mathf.FloorToInt(PlayerManager.Instance.StreamTimeIncrease*timeMultiplier));
            yield return new WaitForSeconds(popupNumberDuration/amount);
        }
    }
}
