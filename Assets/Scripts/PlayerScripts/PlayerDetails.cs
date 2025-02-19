using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class PlayerDetails : MonoBehaviour
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
    private StringBuilder sb = new StringBuilder();

    private System.Action<string> OnChangeStreamNameDelegate;
    void Start()
    {
        OnChangeStreamNameDelegate = (t) => UpdateStreamNameText(t);
        PlayerManager.Instance.OnChangeStreamNameEvent += OnChangeStreamNameDelegate;
        DisplayRemainingStreamTime = PlayerManager.Instance.RemainingStreamTime;
        StartCoroutine(PeriodicValueSync());
    }
    
    void Update()
    {
        sb.Clear();
        sb.Append(PlayerManager.Instance.CurrentViewers.ToString());
        viewersText.text = sb.ToString();

        sb.Clear();
        sb.Append("SUBS: ").Append(PlayerManager.Instance.Subscriptions.ToString());
        subscribersText.text = sb.ToString();
        DisplayRemainingStreamTime -= Time.deltaTime;
        if(DisplayRemainingStreamTime <= 0)
        {
            ForceSyncRemainingStreamTime();
        }
        remainingStreamTime.text = TimescaleManager.Instance.FormatTimeString(DisplayRemainingStreamTime);
        elapsedStreamTime.text = TimescaleManager.Instance.FormatTimeString(PlayerManager.Instance.ElapsedStreamTime);
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

    public IEnumerator SpawnSubPopupNumber(int times)
    {
        int amount = Mathf.Min(times, popupNumberMax);
        Vector2 rangeMin = remainingStreamTime.textBounds.min;
        Vector2 rangeMax = remainingStreamTime.textBounds.max;
        for(int i = 0; i < amount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(rangeMin.x, rangeMax.x), Random.Range(rangeMin.y, rangeMax.y), 0);
            Vector3 worldPosition = remainingStreamTime.rectTransform.TransformPoint(randomOffset);
            PopupTextSpawner.Instance.SpawnPopupText(
                worldPosition, 
                "+" + Mathf.FloorToInt(PlayerManager.Instance.StreamTimeIncrease*timeMultiplier), 
                0.5f
            );
            yield return new WaitForSeconds(popupNumberDuration/amount);
        }
    }

    void OnDestroy()
    {
        PlayerManager.Instance.OnChangeStreamNameEvent -= OnChangeStreamNameDelegate;
    }
}
