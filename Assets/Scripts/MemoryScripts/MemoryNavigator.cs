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
    [SerializeField] public float distanceBetweenIndex = 0;
    [SerializeField] private float cooldownBetweenNavigation = 0.2f;
    [SerializeField] public int CurrentMemoryIndex = 0;
    public MemoryData MemoryData => MemoryManager.Instance.MemoryData;
    public int MemoryCount => MemoryManager.Instance.MemoryCount;
    public event System.Action<int> OnChangeMemoryIndex;
    private int nearestIndex = -1;
    private float cooldownTimer = 0f;
    private float scrollDelta = 0f;
    public bool IsDraggingScroll = false;
    private Coroutine smoothScrollCoroutine;

    private System.Action<PointerEventData> BeginDragDelegate;
    private System.Action<PointerEventData> EndDragDelegate;
    private System.Action<PointerEventData> ScrollDelegate;

    void Awake()
    {
        BeginDragDelegate = (t) => {SetIsDraggingScroll(true);};
        EndDragDelegate = (t) => {SetIsDraggingScroll(false);};
        ScrollDelegate = (t) => {UpdateScrollingInput(t);};
    }
    void Start()
    {
        scrollRectEventHandler.OnBeginDragEvent += BeginDragDelegate;
        scrollRectEventHandler.OnEndDragEvent += EndDragDelegate;
        scrollRectEventHandler.OnScrollEvent += ScrollDelegate;
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
    public void UpdateScrollingInput(PointerEventData eventData)
    {
        scrollDelta = -eventData.scrollDelta.y;
    }

    public void SetIsDraggingScroll(bool isDragging)
    {
        IsDraggingScroll = isDragging;
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

    public void SetCurrentMemoryIndex(int index, bool smooth = true)
    {
        if(index < 0 || index >= MemoryCount)
        {
            return;
        }
        CurrentMemoryIndex = index;
        SetFocusMemory(CurrentMemoryIndex);
        ScrollToIndex(CurrentMemoryIndex, smooth);
        SetIntegrityBarEntity(MemoryData.GetMemoryEntity(CurrentMemoryIndex));
        OnChangeMemoryIndex?.Invoke(CurrentMemoryIndex);
    }


    public void SetFocusMemory(int index)
    {
        for(int i = 0; i < MemoryCount; i++)
        {
            MemoryInfo memoryInfo = MemoryData.GetMemoryInfo(i);
            MemoryEntity entity = memoryInfo.Entity;
            if (i == index)
            {
                entity.SetInFocus(true);
            }
            else
            {
                entity.SetInFocus(false);
            }
        }
    }

    private void ScrollToIndex(int index, bool smooth = true)
    {
        if(smooth)
        {
            if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
            smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(index));
        }
        else
        {
            InstantScrollTo(index);
        }
    }

    private void SetIntegrityBarEntity(MemoryEntity entity)
    {
        if(healthIndicator == null) return;
        healthIndicator.SetEntity(entity);
    }

    public void SetupMemoryBlock(string memoryName, MemoryInfo memoryInfo)
    {
        MemoryEntity memory = memoryInfo.Entity;
        MemoryBlock memoryBlock = memoryInfo.Block;
        memory.transform.SetParent(memoryContent);
        memoryBlock.SetupMemoryBlock(memoryName, MemoryCount-1);
        memory.transform.SetSiblingIndex(2 + MemoryCount-1);

    }

    public float GetMemoryWidth()
    {
        return memoryWidth;
    }

    public float GetMemoryBlockPosition(int index)
    {
        return distanceBetweenIndex * (2 + MemoryCount - index);
    }

    public float GetContentWidth()
    {
        int blockCount = MemoryCount + 5; // 4 buffers and 1 extra from the 2x size middle memory
        return blockCount * memoryWidth + (blockCount-1) * memoryContent.GetComponent<HorizontalLayoutGroup>().spacing;
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
            if(IsDraggingScroll)
            {
                while(IsDraggingScroll)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                SnapToNearestMemory();
            }

            yield return null;
        }
    }

    public void InstantScrollTo(int index)
    {
        memoryContent.anchoredPosition = new Vector2(-memoryWidth/2*(MemoryCount-1) + memoryWidth * index, memoryContent.anchoredPosition.y);
    }


    IEnumerator SmoothScrollTo(int index)
    {
        ScrollRect.StopMovement();
        float duration = 0.3f; // Duration of the scroll animation
        float elapsedTime = 0f;

        float contentOffset = memoryContent.anchoredPosition.x; // Offset of content
        float targetX = -memoryWidth/2*(MemoryCount-1) + memoryWidth * index;
    

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
        scrollRectEventHandler.OnBeginDragEvent -= BeginDragDelegate;
        scrollRectEventHandler.OnEndDragEvent -= EndDragDelegate;
        scrollRectEventHandler.OnScrollEvent -= ScrollDelegate;
    }
}