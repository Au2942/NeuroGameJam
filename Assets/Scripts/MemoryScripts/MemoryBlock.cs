using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MemoryBlock : MonoBehaviour, IScrollHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [SerializeField] public TextMeshProUGUI MemoryDisplayNameText;
    [SerializeField] private HealthIndicator healthIndicator;
    [SerializeField] private UIEventHandler clickBlockDetector;
    public MemoryNavigator MemoryNavigator => MemoryManager.Instance.MemoryNavigator;
    public ScrollRect ScrollRect => MemoryNavigator.ScrollRect;
    public string MemoryDisplayName;
    public int MemoryIndex = -1;
    System.Action<PointerEventData> pointerUpEventHandler;
    UnityEngine.Events.UnityAction<Vector2> scrollRectValueChangeEventHandler;
    void Awake()
    {
        pointerUpEventHandler = (t) => OnPointerUp();
        scrollRectValueChangeEventHandler = (t) => UpdateBlockOnScroll();
    }

    void OnEnable()
    {
        if(MemoryManager.Instance.MemoryNavigator == null)
        {
            gameObject.SetActive(false);
        }
        if(clickBlockDetector != null)
        {
            clickBlockDetector.OnPointerUpEvent += pointerUpEventHandler;
            clickBlockDetector.OnScrollEvent += OnScroll;
            clickBlockDetector.OnBeginDragEvent += OnBeginDrag;
            clickBlockDetector.OnDragEvent += OnDrag;
            clickBlockDetector.OnEndDragEvent += OnEndDrag;
        }
        ScrollRect.onValueChanged.AddListener(scrollRectValueChangeEventHandler);
    }

    public void OnPointerUp()
    {
        if(MemoryNavigator == null || MemoryNavigator.IsDraggingScroll) return;
        MemoryNavigator.SetCurrentMemoryIndex(MemoryIndex);
    }

    public void SetupMemoryBlock(string name, int newIndex)
    {
        MemoryIndex = newIndex;
        SetMemoryDisplayName(name);
        healthIndicator.SetEntity(MemoryManager.Instance.MemoryData.MemoryInfos[newIndex].Entity);
    }

    public void EnableBlockClickDetector(bool enable)
    {
        clickBlockDetector.gameObject.SetActive(enable);
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
        float memoryWidth = MemoryNavigator.GetMemoryWidth();
        float center = MemoryNavigator.GetContentWidth()/2 - scrollRect.content.anchoredPosition.x; // width/2 - position.x
        float blockPosition = MemoryNavigator.GetMemoryBlockPosition(MemoryIndex);
        float distanceFromCenter = Mathf.Abs(blockPosition - center);
        float scale = 0.5f;
        if(distanceFromCenter < memoryWidth/2)
        {
            scale = 1f - distanceFromCenter / memoryWidth;
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
            clickBlockDetector.OnScrollEvent -= MemoryNavigator.UpdateScrollingInput;
            clickBlockDetector.OnBeginDragEvent -= OnBeginDrag;
            clickBlockDetector.OnDragEvent -= OnDrag;
            clickBlockDetector.OnEndDragEvent -= OnEndDrag;
        }
        ScrollRect.onValueChanged.RemoveListener(scrollRectValueChangeEventHandler);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if(MemoryManager.Instance.MemoryNavigator == null) return;
        MemoryNavigator.UpdateScrollingInput(eventData);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = ScrollRect.gameObject;
        ScrollRect.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = ScrollRect.gameObject;
        ScrollRect.OnBeginDrag(eventData);
        if(MemoryManager.Instance.MemoryNavigator == null) return;
        MemoryNavigator.SetIsDraggingScroll(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = ScrollRect.gameObject;
        ScrollRect.OnDrag(eventData);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = ScrollRect.gameObject;
        ScrollRect.OnEndDrag(eventData);
        if(MemoryManager.Instance.MemoryNavigator == null) return;
        MemoryNavigator.SetIsDraggingScroll(false);
    }

}
