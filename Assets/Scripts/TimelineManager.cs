using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private GameObject memoryLayout;

    public int currentMemoryIndex {get; set; }= 0;
    private GameObject nextMemory;
    public int MemoriesCount { get; set; } = 0;
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

        MemoryEntity[] memoryEntities = FindObjectsByType<MemoryEntity>( FindObjectsSortMode.None);
        foreach(MemoryEntity memoryEntity in memoryEntities)
        {
            nextMemory = AddMemoryToTimeline(memoryEntity.gameObject);
        }
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
            if(cooldownTimer < cooldownBetweenNavigation)
            {
                return;
            }
            cooldownTimer = 0f;
            if(horizontalInput > 0)
            {
                currentMemoryIndex--;
            }
            else if(horizontalInput < 0)
            {
                currentMemoryIndex++;
            }
            if(currentMemoryIndex < 0)
            {
                currentMemoryIndex = 0;
            }
            else if(currentMemoryIndex > MemoriesCount)
            {
                currentMemoryIndex = MemoriesCount;
            }
        }
        else
        {
            cooldownTimer = cooldownBetweenNavigation;
        }
    }

    private void UpdateMemoryLayoutPosition()
    {
        memoryLayout.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 + currentMemoryIndex*1920, 0);
    }

    public GameObject AddMemoryToTimeline(GameObject newMemory)
    {
        MemoriesCount++;
        GameObject memory = Instantiate(newMemory, memoryLayout.transform);
        memory.transform.SetSiblingIndex(1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(memoryLayout.GetComponent<RectTransform>());
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;
        return memory;
    }
    
}
