using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] public ChannelData ChannelData;
    [SerializeField] private StreamSO defaultStream;
    [SerializeField] private LivefeedRenderer LivefeedRenderer;
    [SerializeField] public ScreenEffectController ScreenEffectController;

    public int ChannelCount => ChannelData.GetChannelCount();
    public int CurrentChannelIndex => ChannelNavigationManager.Instance.CurrentChannelIndex;
    public List<Entity> Entities => ChannelData.GetChannelEntities();

    public StreamSO CurrentStream { get; private set; }
    public bool isStreaming { get; set; } = false;




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
        ChannelNavigationManager.Instance.OnChangeChannelIndex += SetChannelIndex;
        StartNewStream(defaultStream);
        SetChannelIndex(0);
    }


    void Update()
    {
        if (isStreaming)
        { 
            PlayerManager.Instance.ProgressStream();
            PlayerManager.Instance.CheckPlayerInput();
            if(ChannelData.GetChannelEntity(CurrentChannelIndex).corrupted)
            {
                ScreenEffectController.Show();
            }
            else
            {
                ScreenEffectController.Hide();
            }
        }
        if(StreamSelector.Instance.isOpen)
        {
            if(InputManager.Instance.Cancel.triggered || InputManager.Instance.RightClick.triggered)
            {
                StreamSelector.Instance.CloseUI();
            }
        }
    }

    private void SetChannelIndex(int index)
    {
        
        for(int i = 0; i < ChannelCount; i++)
        {
            ChannelInfo channelInfo = ChannelData.GetChannelInfo(i);
            Entity entity = channelInfo.entity;
            if (i == index)
            {
                entity.SetInFocus(true);
                roomText.text = LivefeedManager.Instance.Livefeeds[i].LivefeedName;
            }
            else
            {
                entity.SetInFocus(false);
            }
        }
    }


    public void StartNewStream(StreamSO newStream)
    {
        CurrentStream = newStream;
        GameObject stream = ChannelNavigationManager.Instance.SetUpStream(CurrentStream);
        SetPlayerStream(CurrentStream);
        ChannelNavigationManager.Instance.SetChannelIndex(ChannelCount-1);
        isStreaming = true;
        OnStartStream?.Invoke();
    }
    private void SetPlayerStream(StreamSO stream)
    {
        PlayerManager.Instance.TargetHype = PlayerManager.Instance.CurrentHype + stream.hypePotential;
        PlayerManager.Instance.CurrentHype += stream.impactHype;
    }
    public void ContinueStream()
    {
        ChannelData.GetChannelEntity(CurrentChannelIndex).SetInFocus(true);
        isStreaming = true;
    }

    public void StopStream()
    {
        List<Entity> Entities = ChannelData.GetChannelEntities();   
        Entities.ForEach(entity => entity.SetInFocus(false));
        Entities.ForEach(entity => entity.ShutUp());
        isStreaming = false;
    }

    public void EndStream()
    {
        StopStream();
        GameObject memory = ChannelNavigationManager.Instance.AddStreamMemoryToChannel(CurrentStream);
        ChannelData.ReplaceChannel(ChannelCount-1, CurrentStream.streamName, memory.GetComponent<MemoryEntity>());
        OnEndStream?.Invoke();
    }

    public Entity GetCurrentEntity()
    {
        return ChannelData.GetChannelEntity(CurrentChannelIndex);
    }

}
