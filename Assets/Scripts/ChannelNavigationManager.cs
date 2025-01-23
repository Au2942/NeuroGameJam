using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChannelNavigationManager : MonoBehaviour 
{
    public static ChannelNavigationManager Instance;
    [SerializeField] private RectTransform channelTransform;
    [SerializeField] private RectTransform entityContainer;
    [SerializeField] private RectTransform entityLayout;
    [SerializeField] public float spacing = 896;
    [SerializeField] private float cooldownBetweenNavigation = 0.2f;

    private int _channelCount => GameManager.Instance.ChannelCount;
    public int CurrentChannelIndex {get; set; } = 0;

    public Dictionary<string, int> MemoryTypesCount {get; set;} = new Dictionary<string, int>();

    public event Action<int> OnChangeChannelIndex;
    public event Action OnUpdateChannelLayout;
    public event Action OnNewStream;
    private Vector2 originalAnchorPosition;

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
        originalAnchorPosition = entityContainer.anchoredPosition;
        spacing = entityContainer.rect.width + entityLayout.GetComponent<HorizontalLayoutGroup>().spacing;
    }

    public Entity GetCurrentEntity()
    {
        return GameManager.Instance.ChannelData.GetChannelEntity(CurrentChannelIndex);
    }

    public void CheckNavigationInput()
    {
        float horizontalInput = InputManager.Instance.Navigate.ReadValue<Vector2>().x;
        if(horizontalInput != 0)
        {
            if(cooldownTimer > Time.time)
            {
                return;
            }

            int tempIndex = CurrentChannelIndex;
            
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
            cooldownTimer = Time.time + cooldownBetweenNavigation;
        }
        else
        {
            cooldownTimer = 0;
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
        entityContainer.anchoredPosition = originalAnchorPosition + new Vector2(spacing * CurrentChannelIndex, 0);
        OnUpdateChannelLayout?.Invoke();
    }


    public GameObject SetUpStream(StreamSO newStream)
    {
        StreamEntity stream = Instantiate(newStream.stream, entityLayout.transform);
        GameManager.Instance.ChannelData.AddChannel("Subathon " + newStream.streamName + " stream", stream);
        stream.name = newStream.streamName;

        if(entityLayout.transform.childCount > 1)
        {
            Destroy(entityLayout.transform.GetChild(0).gameObject);
        }
       
        stream.transform.SetAsFirstSibling();

        OnNewStream?.Invoke();
        
        return stream.gameObject;
    }

    public GameObject AddStreamMemoryToChannel(StreamSO stream)
    {
        if(!MemoryTypesCount.TryAdd(stream.name, 1))
        {
            MemoryTypesCount[stream.name]++;
        }
        MemoryEntity memory = Instantiate(stream.memory, entityLayout.transform);
        LivefeedManager.Instance.Livefeeds[^1].SetLivefeedName("Memory of " + stream.streamName + " stream #" + MemoryTypesCount[stream.name]);
        memory.name = stream.streamName;
        memory.transform.SetSiblingIndex(1);


        
        return memory.gameObject;
    }
    
}
