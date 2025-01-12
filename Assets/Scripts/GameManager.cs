using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private VideoPlayer streamVideoPlayer;
    [SerializeField] private StreamSelector streamSelector;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] public List<MemoryEntity> MemoryEntities;
    [SerializeField] private Image integrityBar;
    [SerializeField] private float addScorePercentage = 1;
    [SerializeField] private DialogueManager endGameDialogueManager;
    [SerializeField] private DialogueInfoSO[] endGameDialogues;
    [SerializeField] private TextMeshProUGUI endScoreText;

    [SerializeField] private AudioClip[] endGameSFXs;

    public StreamSO CurrentStream { get; private set; }
    public int currentDay { get; private set; } = 0;
    public float streamTime {get; set;} = 10f;
    public bool isStreaming { get; set; } = false;

    private float nextScoreTime = 0f;
    private float nextScoreTimer = 0f;
    private float minInterval = 5f; // Minimum interval in seconds
    private float maxInterval = 20f; // Maximum interval in seconds

    private float streamTimer = 0f;

    private float targetIntegrityBarValue = 0f;

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
        StartCoroutine(MoveIntegrityBar());
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
        int currentIndex = TimelineManager.Instance.currentMemoryIndex;
        if(currentIndex == 0)
        {
            roomText.text = "Livestream";
            targetIntegrityBarValue = 1f;
            streamVideoPlayer.SetDirectAudioVolume(0, 1);
        }
        else streamVideoPlayer.SetDirectAudioVolume(0, 0);

        for(int i = 0; i < MemoryEntities.Count; i++)
        {

            MemoryEntity memoryEntity = MemoryEntities[i];
            if (currentIndex != 0 && i == MemoryEntities.Count - currentIndex)
            {
                targetIntegrityBarValue = MemoryEntities[i].Integrity/(float)MemoryEntities[i].MaxIntegrity;
                memoryEntity.SetInFocus(true);
                string roomName = "";
                switch(memoryEntity.phase)
                {
                    case 0:
                        roomName = "Memory of ";
                        break;
                    case 1:
                        roomName = "Corrupted memory of ";
                        break;
                    case 2:
                        roomName = "Remnants of ";
                        break;
                }
                roomText.text = roomName + memoryEntity.name + " stream";
            }
            else
            {
                memoryEntity.SetInFocus(false);
            }
        }
    }

    private IEnumerator MoveIntegrityBar()
    {
        while (true)
        {
            integrityBar.fillAmount = Mathf.MoveTowards(integrityBar.fillAmount, targetIntegrityBarValue, Time.deltaTime);
            yield return null;
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
        TimelineManager.Instance.currentMemoryIndex = 0;
    }

    public void DayEnd()
    {
        if(PlayerManager.Instance.Score >= 100000)
        {
           StartCoroutine(EndGame(2));
        }
        else if(PlayerManager.Instance.Score <= 0)
        {
            StartCoroutine(EndGame(1));
        }
        MemoryEntities.ForEach(entity => entity.SetInFocus(false));
        MemoryEntities.ForEach(entity => entity.ShutUp());
        isStreaming = false;
        streamVideoPlayer.Stop();

        GameObject memory = TimelineManager.Instance.AddStreamToMemory(CurrentStream);
        MemoryEntities.Add(memory.GetComponent<MemoryEntity>());

        PlayerManager.Instance.Buff = 0;
        PlayerManager.Instance.Debuff = 0;
        PlayerManager.Instance.TakeDamage(-5);

        OnDayEnd?.Invoke();

    }

    public IEnumerator EndGame(int endingIndex)
    {
        isStreaming = false;
        streamVideoPlayer.Stop();
        endGameDialogueManager.gameObject.SetActive(true);
        if(endGameSFXs[endingIndex] != null)
        {
            SFXManager.Instance.PlaySoundFX(endGameSFXs[endingIndex], transform);
            yield return new WaitForSeconds(endGameSFXs[endingIndex].length);
        }
        
        endGameDialogueManager.PlayDialogue(endGameDialogues[endingIndex]);
        endScoreText.text = "You've lasted " +  currentDay + " days and got " + PlayerManager.Instance.Score + " viewers!";
        while(endGameDialogueManager.IsTyping)
        {
            yield return null;
        }
        while(true)
        {
            if(InputManager.Instance.Submit.triggered || InputManager.Instance.Cancel.triggered || InputManager.Instance.Click.triggered) 
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break;
            }
            yield return null;
        }
    }


    
}
