using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Video;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private VideoPlayer streamVideoPlayer;
    [SerializeField] private StreamSelector streamSelector;
    [SerializeField] private ScrollbarValueController scrollbarController;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private List<MemoryEntity> memoryEntities;
    [SerializeField] private int baseScore = 100;

    public StreamSO CurrentStream { get; private set; }
    public int currentDay { get; private set; } = 0;
    public float streamTime {get; set;} = 10f;
    public bool isStreaming { get; set; } = false;

    private float nextScoreTime = 0f;
    private float nextScoreTimer = 0f;
    private float minInterval = 1f; // Minimum interval in seconds
    private float maxInterval = 5f; // Maximum interval in seconds

    public event Action OnDayEnd;
    public event Action OnDayStart;

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
        streamVideoPlayer.prepareCompleted += OnPrepareCompleted;
        memoryEntities.AddRange(FindObjectsByType<MemoryEntity>(FindObjectsSortMode.None));
        DayStart();
    }

    public void DayStart()
    {
        //memoryEntities.ForEach(entity => entity.Integrity = entity.MaxIntegrity);
        currentDay++;
        dayText.text = "Day " + currentDay;
        streamSelector.OpenUI();
        OnDayStart?.Invoke();
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
            else
            {
                nextScoreTimer += Time.deltaTime;
                if (nextScoreTimer >= nextScoreTime)
                {
                    nextScoreTimer = 0f;
                    PlayerManager.Instance.AddUncertainScore(baseScore, 10);
                    nextScoreTime = UnityEngine.Random.Range(minInterval, maxInterval);
                }
            }
        }
    }

    public void SetStream(StreamSO newStream)
    {
        CurrentStream = newStream;
    }

    private void CheckEntityInteractability()
    {
        float scrollbarValue = scrollbarController.GetValue();
        int screenIndex = Mathf.RoundToInt(scrollbarValue * memoryEntities.Count);

        for(int i = 0; i < memoryEntities.Count; i++)
        {
            MemoryEntity memoryEntity = memoryEntities[i];
            if (i == memoryEntities.Count - screenIndex)
            {
                memoryEntity.InFocus = true;
            }
            else
            {
                memoryEntity.InFocus = false;
            }
        }
    }

    public void PrepareStream()
    {
        
        streamVideoPlayer.clip = CurrentStream.clip;
        streamVideoPlayer.Prepare();

    }

    void OnPrepareCompleted(VideoPlayer videoPlayer)
    {
        StartStream();
        videoPlayer.Play();
    }

    public void StartStream()
    {
        streamTime = 10f * currentDay;
        isStreaming = true;
        scrollbarController.SetValue(0);
    }

    public void DayEnd()
    {
        memoryEntities.ForEach(entity => entity.InFocus = false);
        memoryEntities.ForEach(entity => entity.ShutUp());
        isStreaming = false;
        streamVideoPlayer.Stop();
        
        MemoryEntity memoryEntity = CurrentStream.memory.GetComponent<MemoryEntity>();
        if (!memoryEntities.Exists(entity => entity.GetType() == memoryEntity.GetType()))
        {
            GameObject memory = TimelineManager.Instance.AddMemoryToTimeline(CurrentStream.memory);
            memoryEntities.Add(memory.GetComponent<MemoryEntity>());
        }
        else
        {
            Debug.Log("MemoryEntity of this type already exists.");
        }
        OnDayEnd?.Invoke();
    }
}
