using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SnapScrollbarValue : MonoBehaviour, IPointerDownHandler ,IPointerUpHandler, IDragHandler
{
    [SerializeField] private Scrollbar timelineScrollbar;
    [SerializeField] int steps = 1;
    [SerializeField] private float transitionDuration = 1f;
    private bool shouldEase = false;
    private float previousValue = 0f;   
    private float currentValue = 0f;
    private float targetValue = 0f;

    private float elapsedTime = 0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        Ease(GetValueFromPointer(eventData));
    }

    private void Ease(float value)
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
        //do the cubic easeout
        if (shouldEase)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            t = t - 1;
            currentValue = Mathf.Lerp(previousValue, targetValue, t * t * t + 1);
            if (t >= 0)
            {
                currentValue = targetValue;
                shouldEase = false;
                elapsedTime = 0f;
            }
        }

        timelineScrollbar.value = currentValue;
    }


    public void OnDrag(PointerEventData eventData)
    {
        currentValue = timelineScrollbar.value;
        StopEase();
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        float roundedValue = Mathf.Round(targetValue*steps) / steps;
        Ease(roundedValue);
    }
 
    private float GetValueFromPointer(PointerEventData eventData)
    {
       
        RectTransform parentRect = timelineScrollbar.handleRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint
        );

        float normalizedValue = Mathf.Clamp01((parentRect.rect.xMax - localPoint.x) / parentRect.rect.width);
        return normalizedValue;
    }




}
