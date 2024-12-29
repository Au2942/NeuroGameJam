using UnityEngine;
using UnityEngine.Video;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private VideoPlayer streamVideoPlayer;
    [SerializeField] private StreamSelector streamSelector;
    [SerializeField] private TextMeshProUGUI dayText;
    public StreamSO CurrentStream { get; private set; }
    public int currentDay { get; private set; } = 0;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DayStart()
    {
        currentDay++;
        dayText.text = "Day " + currentDay;
        streamSelector.OpenUI();
    }

    public void SetStream(StreamSO newStream)
    {
        CurrentStream = newStream;
    }

    public void PlayStream()
    {
        streamVideoPlayer.clip = CurrentStream.clip;
    }

    public void DayEnd()
    {
        TimelineManager.Instance.AddMemory(CurrentStream.memory);
    }
}
