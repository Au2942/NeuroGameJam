using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject entityContainer;
    [SerializeField] private GameObject entityLayout;

    public int currentEntityIndex {get; set; } = 0;
    public int entitiesCount { get; set; } = 0;

    public Dictionary<string, int> MemoryEntityTypesCount {get; set;} = new Dictionary<string, int>();

    public event Action<int> OnChangeMemoryIndex;
    public event Action OnMemoryAdded;

    private float layoutWidth;
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(entityContainer.GetComponent<RectTransform>());
        layoutWidth = entityContainer.GetComponent<RectTransform>().rect.width;
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
            int tempIndex = currentEntityIndex;
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
                currentEntityIndex = 0;
            }
            else if(tempIndex > entitiesCount-1)
            {
                currentEntityIndex = entitiesCount-1;
            }
            else 
            {
                SetEntityIndex(tempIndex);
            }
        }
        else
        {
            cooldownTimer = cooldownBetweenNavigation;
        }
    }

    public void SetEntityIndex(int index)
    {
        currentEntityIndex = index;
        UpdateMemoryLayoutPosition();
        Debug.Log("Current Entity Index: " + currentEntityIndex);
        OnChangeMemoryIndex?.Invoke(currentEntityIndex);
    }

    private void UpdateMemoryLayoutPosition()
    {
        entityContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1152*(entitiesCount-1-currentEntityIndex), 0);
    }

    public GameObject SetUpStream(StreamSO newStream)
    {
        GameObject stream = Instantiate(newStream.stream.gameObject, entityLayout.transform);
        entitiesCount++;
        stream.name = newStream.streamName;
        stream.transform.SetAsFirstSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(entityContainer.GetComponent<RectTransform>());
        layoutWidth = entityContainer.GetComponent<RectTransform>().rect.width;
        return stream;
    }

    public GameObject ChangeStream(StreamSO newStream)
    {
        Destroy(entityLayout.transform.GetChild(0).gameObject);
        GameObject stream = Instantiate(newStream.stream.gameObject, entityLayout.transform);
        stream.name = newStream.streamName;
        stream.transform.SetAsFirstSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(entityContainer.GetComponent<RectTransform>());
        layoutWidth = entityContainer.GetComponent<RectTransform>().rect.width;
        
        return stream;
    }

    public GameObject AddStreamToMemory(StreamSO stream)
    {
        if(!MemoryEntityTypesCount.TryAdd(stream.name, 1))
        {
            MemoryEntityTypesCount[stream.name]++;
        }
        entitiesCount++;

        GameObject memory = Instantiate(stream.memory.gameObject, entityLayout.transform);
        memory.name = stream.streamName;
        memory.transform.SetSiblingIndex(1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(entityContainer.GetComponent<RectTransform>());
        layoutWidth = entityContainer.GetComponent<RectTransform>().rect.width;

        OnMemoryAdded?.Invoke();
        
        return memory;
    }
    
}
