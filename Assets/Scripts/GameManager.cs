using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private TextMeshProUGUI channelText;
    [SerializeField] public ChannelData ChannelData;
    [SerializeField] private StreamSO defaultStream;
    [SerializeField] private LivefeedScroller LivefeedRenderer;
    [SerializeField] public ScreenEffectController ScreenEffectController;
    public int ChannelCount => ChannelData.GetChannelCount();
    public int CurrentChannelIndex => ChannelNavigationManager.Instance.CurrentChannelIndex;
    public List<Entity> Entities => ChannelData.GetChannelEntities();

    public StreamSO CurrentStream { get; private set; }
    public bool isPause { get; set; } = false;



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

        PlayerManager.Instance.CheckPlayerInput();
        
        if (isPause)
        { 
            return;
        }

        PlayerManager.Instance.ProgressStream();
        
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.sleep)
        {
            return;
        }

        ChannelNavigationManager.Instance.CheckNavigationInput();

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
                channelText.text = channelInfo.name;
                if(entity.Glitched)
                {
                    ScreenEffectController.Show();
                }
                else
                {
                    ScreenEffectController.Hide();
                }
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
        OnStartStream?.Invoke();
    }
    private void SetPlayerStream(StreamSO stream)
    {
        PlayerManager.Instance.TargetHype = PlayerManager.Instance.CurrentHype + stream.hypePotential;
        PlayerManager.Instance.CurrentHype += stream.impactHype;
        PlayerManager.Instance.CurrentStreamTimer = 0;
    }
    public void ContinueStream()
    {
        ChannelData.GetChannelEntity(CurrentChannelIndex).SetInFocus(true);
    }

    public void StopStream()
    {
        List<Entity> Entities = ChannelData.GetChannelEntities();   
        Entities.ForEach(entity => entity.SetInFocus(false));
        Entities.ForEach(entity => entity.ShutUp());
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
