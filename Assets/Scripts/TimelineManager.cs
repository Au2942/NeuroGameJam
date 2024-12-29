using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject memoryLayout;
    [SerializeField] private Scrollbar timelineScrollbar;
    private GameObject nextMemory;
    public int MemoriesCount { get; set; } = 1;
    private float layoutWidth;

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
    }

    void Update()
    {
        memoryLayout.GetComponent<RectTransform>().anchoredPosition 
            = new Vector2(timelineScrollbar.value * (layoutWidth - 1920) , 0f);
    }

    public void SetValue(float value)
    {
        timelineScrollbar.value = value;
    }

    public float GetValue()
    {
        return timelineScrollbar.value;
    }

    public void AddMemory(GameObject memory)
    {
        MemoriesCount++;
        memory = Instantiate(memory, memoryLayout.transform);
        memory.transform.SetSiblingIndex(1);
        memory.name = "Memory " + MemoriesCount;
        LayoutRebuilder.ForceRebuildLayoutImmediate(memoryLayout.GetComponent<RectTransform>());
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;
        timelineScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(200*MemoriesCount, 60);
    }
    
}
