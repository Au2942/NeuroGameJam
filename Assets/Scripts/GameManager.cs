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
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] public List<Entity> Entities;
    [SerializeField] private Image integrityBar;

    [SerializeField] private StreamSO defaultStream;



    public StreamSO CurrentStream { get; private set; }
    public bool isStreaming { get; set; } = false;



    private float targetIntegrityBarValue = 0f;

    public event Action OnStartStream;
    public event Action OnEndStream;

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
        TimelineManager.Instance.OnChangeMemoryIndex += SetMemoryIndex;

        Entities.AddRange(FindObjectsByType<Entity>(FindObjectsSortMode.None));

        StartCoroutine(MoveIntegrityBar());
        
        InitialiseStream(defaultStream);

    }


    void Update()
    {
        if (isStreaming)
        { 
            UpdateIntegrityBar();
            PlayerManager.Instance.ProgressStream();
        }
        if(StreamSelector.Instance.isOpen)
        {
            if(InputManager.Instance.Cancel.triggered || InputManager.Instance.RightClick.triggered)
            {
                StreamSelector.Instance.CloseUI();
            }
        }
    }

    private void SetMemoryIndex(int index)
    {

        for(int i = 0; i < Entities.Count; i++)
        {

            Entity entity = Entities[i];
            if (i == index)
            {
                entity.SetInFocus(true);
                string roomName = "";
                if(index == Entities.Count-1)
                {   
                    roomText.text = "Livestream";
                }
                else 
                {
                    if(!entity.glitched)
                    {
                        roomName = "Memory of ";
                    }
                    else
                    {   
                        roomName = "Corrupted Memory of ";
                    }
                    roomText.text = roomName + entity.name + " stream";
                }
            }
            else
            {
                entity.SetInFocus(false);
            }
        }
    }

    private void UpdateIntegrityBar()
    {
        int index = TimelineManager.Instance.currentEntityIndex;
        targetIntegrityBarValue = Entities[^(index+1)].Integrity/(float)Entities[^(index+1)].MaxIntegrity;
    }

    private IEnumerator MoveIntegrityBar()
    {
        while (true)
        {
            integrityBar.fillAmount = Mathf.MoveTowards(integrityBar.fillAmount, targetIntegrityBarValue, Time.deltaTime);
            yield return null;
        }
    }
    public void InitialiseStream(StreamSO newStream)
    {
        CurrentStream = newStream;
        GameObject stream = TimelineManager.Instance.SetUpStream(CurrentStream);
        Entities.Add(stream.GetComponent<StreamEntity>());
        isStreaming = true;
        TimelineManager.Instance.SetEntityIndex(0);
        OnStartStream?.Invoke();
    }

    public void StartNewStream(StreamSO newStream)
    {
        CurrentStream = newStream;
        GameObject stream = TimelineManager.Instance.ChangeStream(CurrentStream);
        Entities.Add(stream.GetComponent<StreamEntity>());
        TimelineManager.Instance.SetEntityIndex(Entities.Count-1);
        foreach(Entity entity in Entities)
        {
            if(entity.Integrity > 50)
            {
                PlayerManager.Instance.Hype += 0.1f;
            }
        }
        isStreaming = true;
        OnStartStream?.Invoke();
    }

    public void ContinueStream()
    {
        Entities[TimelineManager.Instance.currentEntityIndex].SetInFocus(true);
        isStreaming = true;
    }

    public void StopStream()
    {
        Entities.ForEach(entity => entity.SetInFocus(false));
        Entities.ForEach(entity => entity.ShutUp());
        isStreaming = false;
    }

    public void EndStream()
    {

        StopStream();

        GameObject memory = TimelineManager.Instance.AddStreamToMemory(CurrentStream);
        Entities[^1] = memory.GetComponent<MemoryEntity>();
        PlayerManager.Instance.TakeDamage(-5);

        OnEndStream?.Invoke();
    }




    
}
