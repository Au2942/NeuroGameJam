using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class MemoryManager : MonoBehaviour 
{
    public static MemoryManager Instance;
    [SerializeField] public MemoryData MemoryData;
    [SerializeField] public ScrollRect ScrollRect;
    [SerializeField] private UIEventHandler scrollRectEventHandler;
    [SerializeField] private RectTransform memoryContent;
    [SerializeField] private HealthIndicator healthIndicator;

    [SerializeField] public int CurrentMemoryIndex = 0;
    [SerializeField] private float memoryWidth = 320;
    [SerializeField] public float distanceBetweenIndex = 896;
    [SerializeField] private float cooldownBetweenNavigation = 0.2f;

    private int MemoryCount => MemoryData.MemoryInfos.Count;

    public Dictionary<string, int> MemoryTypesCount {get; set;} = new Dictionary<string, int>();

    public event Action<int> OnChangeMemoryIndex;
    private int nearestIndex = -1;
    private float cooldownTimer = 0f;
    private float scrollDelta = 0f;
    private bool isDraggingScroll = false;
    private Coroutine smoothScrollCoroutine;

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
        scrollRectEventHandler.OnBeginDragEvent += (t) => {isDraggingScroll = true;};
        scrollRectEventHandler.OnEndDragEvent += (t) => {isDraggingScroll = false;};
        scrollRectEventHandler.OnScrollEvent += UpdateScrollingInput;
        distanceBetweenIndex = memoryWidth + memoryContent.GetComponent<HorizontalLayoutGroup>().spacing;
        StartCoroutine(SnapToNearestMemoryRoutine());
    }

    public void UpdateMemoryManager()
    {
        CheckNavigationInput();
    }

    private void CheckNavigationInput()
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


    private void SetFocusMemory(int index)
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
        healthIndicator.SetEntity(entity);
    }


/* 
    public GameObject SetUpStream(StreamSO newStream)
    {
        StreamEntity stream = Instantiate(newStream.stream, entityLayout.transform);
        GameManager.Instance.MemoryData.AddMemory("Subathon " + newStream.streamName + " stream", stream);
        stream.name = newStream.streamName;

        if(entityLayout.transform.childCount > 1)
        {
            Destroy(entityLayout.transform.GetChild(0).gameObject);
        }
       
        stream.transform.SetAsFirstSibling();

        OnNewStream?.Invoke();
        
        return stream.gameObject;
    }
 */
    public GameObject AddMemoryFromStream(StreamSO stream)
    {
        if(!MemoryTypesCount.TryAdd(stream.name, 1))
        {
            MemoryTypesCount[stream.name]++;
        }

        MemoryEntity memory = Instantiate(stream.memory, memoryContent.transform);
        MemoryInfo memoryInfo = MemoryData.AddMemory("Memory of " + stream.name + "stream #" + MemoryTypesCount[stream.name], memory);
        
        MemoryCell memoryCell = memory.GetComponent<MemoryCell>();
        memoryCell.SetupMemoryCell(memoryInfo.name, MemoryCount-1);
        ScrollRect.onValueChanged.AddListener(delegate {memoryCell.UpdateCellOnScroll();});

        memory.transform.SetSiblingIndex(2 + MemoryCount-1);
        SetCurrentMemoryIndex(MemoryCount-1);

        return memory.gameObject;
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
    
}
