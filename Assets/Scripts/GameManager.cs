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
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] public List<MemoryEntity> MemoryEntities;
    [SerializeField] private float addScorePercentage = 1;

    public StreamSO CurrentStream { get; private set; }
    public int currentDay { get; private set; } = 0;
    public float streamTime {get; set;} = 30f;
    public bool isStreaming { get; set; } = false;

    private float nextScoreTime = 0f;
    private float nextScoreTimer = 0f;
    private float minInterval = 5f; // Minimum interval in seconds
    private float maxInterval = 20f; // Maximum interval in seconds

    private float streamTimer = 0f;

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
        MemoryEntities.AddRange(FindObjectsByType<MemoryEntity>(FindObjectsSortMode.None));
        DayStart();
    }

    public void DayStart()
    {
        //memoryEntities.ForEach(entity => entity.Integrity = entity.MaxIntegrity);
        currentDay++;
        dayText.text = "Day " + currentDay;
        streamSelector.OpenUI();
        foreach(MemoryEntity entity in MemoryEntities)
        {
            if(entity.phase == 0)
            {
                PlayerManager.Instance.Buff += 0.1f;
            }
        }
        OnDayStart?.Invoke();
    }



    void Update()
    {
        CheckCurrentRoom();
        if (isStreaming)
        { 
            streamTimer += Time.deltaTime;
            if (streamTimer >= streamTime)
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
                    PlayerManager.Instance.AddScoreByPercentage(addScorePercentage, 10);
                    nextScoreTime = UnityEngine.Random.Range(minInterval, maxInterval);
                }
            }
        }
    }

    public void SetStream(StreamSO newStream)
    {
        CurrentStream = newStream;
    }

    private void CheckCurrentRoom()
    {
        float scrollbarValue = scrollbarController.GetValue();
        int screenIndex = Mathf.RoundToInt(scrollbarValue * MemoryEntities.Count);
        if(screenIndex == 0)
        {
            roomText.text = "Stream";
            return;
        }
        streamVideoPlayer.SetDirectAudioVolume(0, 1f-screenIndex*0.25f);

        for(int i = 0; i < MemoryEntities.Count; i++)
        {
            MemoryEntity memoryEntity = MemoryEntities[i];
            if (i == MemoryEntities.Count - screenIndex)
            {
                memoryEntity.InFocus = true;
                roomText.text = "Memory of " + memoryEntity.name + " stream";
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
        streamTime += 10f;
        streamTimer = 0f;
        isStreaming = true;
        scrollbarController.SetValue(0);
    }

    public void DayEnd()
    {
        MemoryEntities.ForEach(entity => entity.InFocus = false);
        MemoryEntities.ForEach(entity => entity.ShutUp());
        isStreaming = false;
        streamVideoPlayer.Stop();
        
        MemoryEntity memoryEntity = CurrentStream.memory.GetComponent<MemoryEntity>();
        MemoryEntity existingEntity = MemoryEntities.Find(entity => entity.GetType() == memoryEntity.GetType());
        if (existingEntity == null)
        {
            GameObject memory = TimelineManager.Instance.AddMemoryToTimeline(CurrentStream.memory);
            memory.name = CurrentStream.streamName;
            MemoryEntities.Add(memory.GetComponent<MemoryEntity>());
        }
        else
        {
            Debug.Log("MemoryEntity of this type already exists.");
            existingEntity.AddIntegrity(25); // Increase integrity of the existing memory entity by 10
        }

        PlayerManager.Instance.Buff = 0;
        PlayerManager.Instance.Debuff = 0;
        PlayerManager.Instance.TakeDamage(-10);

        OnDayEnd?.Invoke();
    }
}
