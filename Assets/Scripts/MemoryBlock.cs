using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MemoryBlock : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI MemoryDisplayNameText;
    [SerializeField] private HealthIndicator healthIndicator;
    [SerializeField] private UIEventHandler clickBlockDetector;
    public MemoryNavigator MemoryNavigator => MemoryManager.Instance.MemoryNavigator;
    public string MemoryDisplayName {get; set;}
    public int MemoryIndex {get; set;} = -1;
    System.Action<PointerEventData> pointerUpEventHandler;
    void Awake()
    {
        pointerUpEventHandler = (t) => OnPointerUp();
    }

    void OnEnable()
    {
        if(clickBlockDetector != null)
        {
            clickBlockDetector.OnPointerUpEvent += pointerUpEventHandler;
        }
    }

    public void OnPointerUp()
    {
        MemoryManager.Instance.SetCurrentMemoryIndex(MemoryIndex);
    }

    public void SetupMemoryBlock(string name, int newIndex)
    {
        SetMemoryDisplayName(name);
        MemoryIndex = newIndex;
        healthIndicator.SetEntity(MemoryManager.Instance.MemoryData.MemoryInfos[newIndex].entity);
    }

    public void SetMemoryDisplayName(string name)
    {
        MemoryDisplayName = name;
        MemoryDisplayNameText.text = MemoryDisplayName;
    }

    public void UpdateBlockOnScroll()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        ScrollRect scrollRect = MemoryNavigator.ScrollRect;
        float center = scrollRect.content.anchoredPosition.x;
        float distanceFromCenter = Mathf.Abs(MemoryIndex*320f - center);
        float scale = 0.5f;
        if(distanceFromCenter < 160f)
        {
            scale = 1f - distanceFromCenter / 320f;
            MemoryNavigator.SetNearestIndex(MemoryIndex);
        }
        rectTransform.localScale = new Vector3(scale, scale, 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    }

    public void OnDisable()
    {
        if(clickBlockDetector != null)
        {
            clickBlockDetector.OnPointerUpEvent -= pointerUpEventHandler;
        }
    }


}
