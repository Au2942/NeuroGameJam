using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject memoryLayout;
    [SerializeField] private Scrollbar timelineScrollbar;
    private GameObject nextMemory;
    public int MemoriesCount { get; set; } = 0;
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
        timelineScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(60*(MemoriesCount+1), 60);
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;

        MemoryEntity[] memoryEntities = FindObjectsByType<MemoryEntity>( FindObjectsSortMode.None);
        foreach(MemoryEntity memoryEntity in memoryEntities)
        {
            nextMemory = AddMemoryToTimeline(memoryEntity.gameObject);
        }
        if(MemoriesCount == 0)
        {
            timelineScrollbar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        memoryLayout.GetComponent<RectTransform>().anchoredPosition 
            = new Vector2(timelineScrollbar.value * (layoutWidth - 1920) , 0f);
    }



    public GameObject AddMemoryToTimeline(GameObject newMemory)
    {
        MemoriesCount++;
        GameObject memory = Instantiate(newMemory, memoryLayout.transform);
        memory.transform.SetSiblingIndex(1);

        timelineScrollbar.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(memoryLayout.GetComponent<RectTransform>());
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;
        timelineScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(60*(MemoriesCount+1), 60);
        return memory;
    }
    
}
