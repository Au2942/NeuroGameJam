using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject memoriesLayout;
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(memoriesLayout.GetComponent<RectTransform>());
        layoutWidth = memoriesLayout.GetComponent<RectTransform>().rect.width;
    }

    void Update()
    {
        memoriesLayout.GetComponent<RectTransform>().anchoredPosition 
            = new Vector2(timelineScrollbar.value * layoutWidth, 0f);
    }

    public void AddMemory(GameObject memory)
    {
        MemoriesCount++;
        memory = Instantiate(memory, memoriesLayout.transform);
        memory.name = "Memory " + MemoriesCount;
        layoutWidth = memoriesLayout.GetComponent<RectTransform>().rect.width;
        timelineScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(200*MemoriesCount, 60);
    }
    

}
