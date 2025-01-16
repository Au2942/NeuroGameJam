using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ChannelNavigationManager : MonoBehaviour 
{
    public static ChannelNavigationManager Instance;
    [SerializeField] private Transform channelTransform;
    [SerializeField] private GameObject entityContainer;
    [SerializeField] private GameObject entityLayout;
    [SerializeField] public float spacing = 1216;

    private int _channelCount => GameManager.Instance.ChannelCount;
    public int CurrentChannelIndex {get; set; } = 0;

    public Dictionary<string, int> MemoryTypesCount {get; set;} = new Dictionary<string, int>();

    public event Action<int> OnChangeChannelIndex;
    public event Action OnNewStream;
    private Vector2 originalAnchorPosition;

    private float cooldownBetweenNavigation = 0.5f;
    private float cooldownTimer = 0f;

    void Awake()
    {
        if (Instance == null)
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
        originalAnchorPosition = channelTransform.transform.position;
        cooldownTimer = cooldownBetweenNavigation;
    }

    void Update()
    {
        if(GameManager.Instance.isStreaming)
        {
            CheckNavigationInput();
        }    
    }

    private void CheckNavigationInput()
    {
        float horizontalInput = InputManager.Instance.Navigate.ReadValue<Vector2>().x;
        if(horizontalInput != 0)
        {
            cooldownTimer += Time.deltaTime;
            int tempIndex = CurrentChannelIndex;
            if(cooldownTimer < cooldownBetweenNavigation)
            {
                return;
            }
            cooldownTimer = 0f;
            if(horizontalInput > 0)
            {
                tempIndex--;
            }
            else if(horizontalInput < 0)
            {
                tempIndex++;
            }

            if(tempIndex < 0)
            {
                CurrentChannelIndex = 0;
            }
            else if(tempIndex > _channelCount-1)
            {
                CurrentChannelIndex = _channelCount-1;
            }
            else 
            {
                SetChannelIndex(tempIndex);
            }
        }
        else
        {
            cooldownTimer = cooldownBetweenNavigation;
        }
    }

    public void SetChannelIndex(int index)
    {
        CurrentChannelIndex = index;
        UpdateChannelLayoutPosition();
        //Debug.Log("Current Entity Index: " + currentEntityIndex);
        OnChangeChannelIndex?.Invoke(CurrentChannelIndex);
    }

    private void UpdateChannelLayoutPosition()
    {
        if(_channelCount == 0) return;
        channelTransform.position = originalAnchorPosition - new Vector2(spacing * CurrentChannelIndex, 0);
    }


    public GameObject SetUpStream(StreamSO newStream)
    {
        GameObject stream = Instantiate(newStream.stream.gameObject, entityLayout.transform);
        GameManager.Instance.ChannelData.AddChannel("Subathon", stream.GetComponent<StreamEntity>());
        stream.name = newStream.streamName;

        if(entityLayout.transform.childCount > 1)
        {
            Destroy(entityLayout.transform.GetChild(0).gameObject);
        }
       
        stream.transform.SetAsFirstSibling();

        OnNewStream?.Invoke();
        
        return stream;
    }

    public GameObject AddStreamMemoryToChannel(StreamSO stream)
    {
        if(!MemoryTypesCount.TryAdd(stream.name, 1))
        {
            MemoryTypesCount[stream.name]++;
        }
        GameObject memory = Instantiate(stream.memory.gameObject, entityLayout.transform);
        ChannelInfo channelInfo = GameManager.Instance.ChannelData.ChannelInfos[_channelCount-1];
        channelInfo.name = stream.streamName;
        memory.name = stream.streamName;
        memory.transform.SetSiblingIndex(1);


        
        return memory;
    }
    
}
