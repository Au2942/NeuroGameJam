using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemoryNavigator : MonoBehaviour
{
    [SerializeField] public ScrollRect ScrollRect;
    [SerializeField] private UIEventHandler scrollRectEventHandler;
    [SerializeField] private RectTransform memoryContent;
    [SerializeField] private HealthIndicator healthIndicator;
    [SerializeField] private float memoryWidth = 320;
    [SerializeField] public float distanceBetweenIndex = 896;
    [SerializeField] private float cooldownBetweenNavigation = 0.2f;
    [SerializeField] public int CurrentMemoryIndex = 0;
    public MemoryData MemoryData => MemoryManager.Instance.MemoryData;
    public int MemoryCount => MemoryManager.Instance.MemoryCount;
    public event System.Action<int> OnChangeMemoryIndex;
    private int nearestIndex = -1;
    private float cooldownTimer = 0f;
    private float scrollDelta = 0f;
    private bool isDraggingScroll = false;
    private Coroutine smoothScrollCoroutine;

    private System.Action<PointerEventData> BeginDragEventHandler;
    private System.Action<PointerEventData> EndDragEventHandler;
    private System.Action<PointerEventData> ScrollEventHandler;

    void Awake()
    {
        BeginDragEventHandler = (t) => {isDraggingScroll = true;};
        EndDragEventHandler = (t) => {isDraggingScroll = false;};
        ScrollEventHandler = (t) => {UpdateScrollingInput(t);};
    }
    void Start()
    {
        scrollRectEventHandler.OnBeginDragEvent += BeginDragEventHandler;
        scrollRectEventHandler.OnEndDragEvent += EndDragEventHandler;
        scrollRectEventHandler.OnScrollEvent += ScrollEventHandler;
        distanceBetweenIndex = memoryWidth + memoryContent.GetComponent<HorizontalLayoutGroup>().spacing;
        StartCoroutine(SnapToNearestMemoryRoutine());
    }

    public void UpdateMemoryManager()
    {
        CheckNavigationInput();
    }

    public void CheckNavigationInput()
    {
        float horizontalInput = InputManager.Instance.Navigate.ReadValue<Vector2>().x;
        float navigationInput = horizontalInput + scrollDelta;
        Navigate(navigationInput);
        scrollDelta = 0;
    }
    private void UpdateScrollingInput(PointerEventData eventData)
    {
        scrollDelta = -eventData.scrollDelta.y;
    }

    private void Navigate(float input)
    {
        if(input != 0)
        {
            if(cooldownTimer > Time.time)
            {
                return;
            }

            int tempIndex = CurrentMemoryIndex;
            
            if(input > 0)
            {
                tempIndex--;
            }
            else if(input < 0)
            {
                tempIndex++;
            }

            if(tempIndex < 0)
            {
                CurrentMemoryIndex = 0;
            }
            else if(tempIndex > MemoryCount-1)
            {
                CurrentMemoryIndex = MemoryCount-1;
            }
            else 
            {
                SetCurrentMemoryIndex(tempIndex);
            }
            cooldownTimer = Time.time + cooldownBetweenNavigation;
        }
        else
        {
            cooldownTimer = 0;
        }
    }

    public void SetCurrentMemoryIndex(int index)
    {
        if(index < 0 || index >= MemoryCount)
        {
            return;
        }
        CurrentMemoryIndex = index;
        SetFocusMemory(CurrentMemoryIndex);
        ScrollToIndex(CurrentMemoryIndex);
        SetIntegrityBarEntity(MemoryData.GetMemoryEntity(CurrentMemoryIndex));
        OnChangeMemoryIndex?.Invoke(CurrentMemoryIndex);
    }


    public void SetFocusMemory(int index)
    {
        for(int i = 0; i < MemoryCount; i++)
        {
            MemoryInfo memoryInfo = MemoryData.GetMemoryInfo(i);
            MemoryEntity entity = memoryInfo.entity;
            if (i == index)
            {
                entity.SetInFocus(true);
                if(entity.Glitched)
                {
                    GameManager.Instance.ScreenEffectController.Show();
                }
                else
                {
                    GameManager.Instance.ScreenEffectController.Hide();
                }
            }
            else
            {
                entity.SetInFocus(false);
            }
        }
    }

    private void ScrollToIndex(int index)
    {
        if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
        smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(index));
    }

    private void SetIntegrityBarEntity(MemoryEntity entity)
    {
        if(healthIndicator == null) return;
        healthIndicator.SetEntity(entity);
    }

    public void SetupMemoryBlock(MemoryEntity memory, MemoryInfo memoryInfo)
    {

        memory.transform.SetParent(memoryContent);

        MemoryBlock memoryBlock = memory.GetComponent<MemoryBlock>();
        memoryBlock.SetupMemoryBlock(memoryInfo.name, MemoryCount-1);

        ScrollRect.onValueChanged.AddListener(delegate {memoryBlock.UpdateBlockOnScroll();});
        memory.transform.SetSiblingIndex(2 + MemoryCount-1);

    }

    

    public void SnapToNearestMemory()
    {   
        SetCurrentMemoryIndex(nearestIndex);
    }

    public void SetNearestIndex(int index)
    {
        nearestIndex = index;
    }

    IEnumerator SnapToNearestMemoryRoutine()
    {
        while(true)
        {
            while(GameManager.Instance.isPause)
            {
                yield return null;
            }
            if(isDraggingScroll)
            {
                while(isDraggingScroll)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                SnapToNearestMemory();
            }

            yield return null;
        }
    }

    IEnumerator SmoothScrollTo(int index)
    {
        ScrollRect.StopMovement();
        float duration = 0.3f; // Duration of the scroll animation
        float elapsedTime = 0f;

        float contentOffset = memoryContent.anchoredPosition.x; // Offset of content
        float targetX = index * memoryWidth;
    

        while (elapsedTime < duration)
        {
            if(ScrollRect.velocity.magnitude > 0.1f)
            {
                yield break;
            }
            // Calculate normalized position
            elapsedTime += Time.unscaledDeltaTime;
            float newPositionX = Mathf.Lerp(contentOffset, targetX, elapsedTime / duration);
            memoryContent.anchoredPosition = new Vector2(newPositionX, memoryContent.anchoredPosition.y);
            yield return null;
        }

        memoryContent.anchoredPosition = new Vector2(targetX, memoryContent.anchoredPosition.y);
    }

    void OnDestroy()
    {
        scrollRectEventHandler.OnBeginDragEvent -= BeginDragEventHandler;
        scrollRectEventHandler.OnEndDragEvent -= EndDragEventHandler;
        scrollRectEventHandler.OnScrollEvent -= ScrollEventHandler;
    }
}