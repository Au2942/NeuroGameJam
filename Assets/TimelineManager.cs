using UnityEngine;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour 
{
    public static TimelineManager Instance;
    [SerializeField] private Scrollbar timelineScrollbar;
    [SerializeField] private GameObject memoryLayout;

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
        layoutWidth = memoryLayout.GetComponent<RectTransform>().rect.width;
    }

    void Update()
    {
        memoryLayout.GetComponent<RectTransform>().anchoredPosition 
            = new Vector2(timelineScrollbar.value * (layoutWidth - Screen.width), 0f);
    }




}
