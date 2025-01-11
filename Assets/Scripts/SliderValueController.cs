using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SliderValueController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Slider timelineSlider;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] DetectUIDrag handleDrag; 
    private int steps => TimelineManager.Instance.MemoriesCount; //steps will number of memories

    private bool shouldEase = false;
    private bool isDragging = false;
    private bool isHolding = false;
    private float previousValue = 0f;   
    private float currentValue = 0f;
    private float targetValue = 0f;

    private float roundingDelay = 0f;
    private float elapsedTime = 0f;
    


    void Start()
    {
        //handleDrag.OnBeginDragEvent += (eventData) => OnBeginDrag();
        handleDrag.OnDragEvent += (eventData) => OnDrag(eventData);
        handleDrag.OnEndDragEvent += (eventData)  => OnEndDrag();
    }


    private void OnDrag(PointerEventData eventData)
    {
        StopEase();
        isDragging = true;
        currentValue = GetValueFromPointer(eventData);
        targetValue = currentValue;
    }

    private void OnEndDrag()
    {
        isDragging = false;
    }

    public void SetValue(float value)
    {
        StopEase();
        previousValue = value;
        targetValue = value;
        currentValue = value;
        timelineSlider.value = value;
    }

    public float GetValue()
    {
        return timelineSlider.value;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        StartEase(GetValueFromPointer(eventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    private void StartEase(float value)
    {
        targetValue = value;
        previousValue = currentValue;
        elapsedTime = 0;
        shouldEase = true;
    }

    private void StopEase()
    {
        elapsedTime = 0;
        shouldEase = false;
    }

    void Update()
    {
        if(steps == 0)
        {
            return;
        }
        float inputValue = HandleInput();
        if(inputValue != 0)
        {
            StartEase(Mathf.Clamp01(currentValue + inputValue));
            roundingDelay = 0.5f;
        }

        if(isDragging || isHolding)
        {
            roundingDelay = 0.1f;
        }

        if(roundingDelay > 0)
        {
            roundingDelay -= Time.deltaTime;
        }
        else if(Mathf.Abs(currentValue - Mathf.Round(currentValue)) > 1e-2f)
        {
            StartEase(Mathf.Round(currentValue * steps) / steps);
        }
        //do the cubic easeout
        if (shouldEase)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            t = t - 1;
            currentValue = Mathf.Lerp(previousValue, targetValue, t * t * t + 1);
            if (t >= 1)
            {
                currentValue = targetValue;
                shouldEase = false;
                elapsedTime = 0f;
            }
        }

        Mathf.Clamp01(currentValue);
        timelineSlider.value = currentValue;
    }

    private float HandleInput()
    {
        Vector2 scrollWheelInput = InputManager.Instance.ScrollWheel.ReadValue<Vector2>();
        Vector2 navigateInput = InputManager.Instance.Navigate.ReadValue<Vector2>();
        if(scrollWheelInput.y < 0 || navigateInput.x > 0)
        {
            return -0.2f;
        }
        else if(scrollWheelInput.y > 0 || navigateInput.x < 0)
        {
            return 0.2f;
        }
        return 0;
    }

    private float GetValueFromPointer(PointerEventData eventData)
    {
       
        RectTransform parentRect = timelineSlider.handleRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint
        );

        float normalizedValue = Mathf.Clamp01((parentRect.rect.xMax - localPoint.x) / parentRect.rect.width);
        return normalizedValue;
    }




}
