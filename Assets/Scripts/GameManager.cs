using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private VideoPlayer streamVideoPlayer;
    [SerializeField] private StreamSelector streamSelector;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private List<MemoryEntity> memoryEntities;

    public StreamSO CurrentStream { get; private set; }
    public int currentDay { get; private set; } = 0;
    public float streamTime {get; set;} = 10f;
    public bool isStreaming { get; set; } = false;

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
    void Start()
    {
        memoryEntities.AddRange(FindObjectsByType<MemoryEntity>(FindObjectsSortMode.None));
        DayStart();
    }

    public void DayStart()
    {
        currentDay++;
        dayText.text = "Day " + currentDay;
        streamSelector.OpenUI();
    }

    void Update()
    {
        CheckEntityInteractability();
        if (isStreaming)
        { 
            streamTime -= Time.deltaTime;
            if (streamTime <= 0)
            {
                isStreaming = false;
                DayEnd();
                DayStart();
            }
        }
    }

    public void SetStream(StreamSO newStream)
    {
        CurrentStream = newStream;
    }

    private void CheckEntityInteractability()
    {
        memoryEntities.ForEach(memoryEntity => memoryEntity.Interactable = false);
        float scrollbarValue = TimelineManager.Instance.GetValue();
        int screenIndex = Mathf.RoundToInt(scrollbarValue * (memoryEntities.Count));
        Debug.Log(screenIndex);

        if (screenIndex > 0 && screenIndex <= memoryEntities.Count)
        {
            MemoryEntity memoryEntity = memoryEntities[memoryEntities.Count - screenIndex];
            memoryEntity.Interactable = true;
        }
    }

    public void StartStream()
    {
        streamVideoPlayer.clip = CurrentStream.clip;
        streamTime = 10f * currentDay;
        isStreaming = true;
        TimelineManager.Instance.SetValue(0);
    }

    public void DayEnd()
    {
        TimelineManager.Instance.AddMemory(CurrentStream.memory);
        memoryEntities.Add(CurrentStream.memory.GetComponent<MemoryEntity>());
    }
}
