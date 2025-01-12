using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject memoryLayout;

    public int currentMemoryIndex {get; set; } = 0;
    public int MemoriesCount { get; set; } = 0;

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
        LayoutRebuilder.ForceRebuildLayoutImmediate(memoryLayout.GetComponent<RectTransform>());
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;
        cooldownTimer = cooldownBetweenNavigation;
    }

    void Update()
    {
        CheckNavigationInput();
        UpdateMemoryLayoutPosition();
    }

    private void CheckNavigationInput()
    {
        float horizontalInput = InputManager.Instance.Navigate.ReadValue<Vector2>().x;
        if(horizontalInput != 0)
        {
            cooldownTimer += Time.deltaTime;
            int tempIndex = currentMemoryIndex;
            if(cooldownTimer < cooldownBetweenNavigation)
            {
                return;
            }
            cooldownTimer = 0f;
            if(horizontalInput > 0)
            {
                tempIndex++;
            }
            else if(horizontalInput < 0)
            {
                tempIndex--;
            }

            if(tempIndex < 0)
            {
                currentMemoryIndex = 0;
            }
            else if(tempIndex > MemoriesCount)
            {
                currentMemoryIndex = MemoriesCount;
            }
            else 
            {
                currentMemoryIndex = tempIndex;
                OnChangeMemoryIndex?.Invoke(currentMemoryIndex);
            }
        }
        else
        {
            cooldownTimer = cooldownBetweenNavigation;
        }
    }

    public void SetMemoryIndex(int index)
    {
        currentMemoryIndex = index;
        OnChangeMemoryIndex?.Invoke(currentMemoryIndex);
    }

    private void UpdateMemoryLayoutPosition()
    {
        memoryLayout.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 - currentMemoryIndex*1440, 0);
    }

    public GameObject AddStreamToMemory(StreamSO stream)
    {
        if(!MemoryEntityTypesCount.TryAdd(stream.name, 1))
        {
            MemoryEntityTypesCount[stream.name]++;
        }
        MemoriesCount++;

        GameObject memory = Instantiate(stream.memory, memoryLayout.transform);
        memory.name = stream.streamName;
        memory.transform.SetSiblingIndex(1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(memoryLayout.GetComponent<RectTransform>());
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;

        OnMemoryAdded?.Invoke();
        
        return memory;
    }
    
}
