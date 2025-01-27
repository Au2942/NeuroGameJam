using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, 
    IScrollHandler
{
    public event Action<PointerEventData> OnPointerDownEvent;
    public event Action<PointerEventData> OnRightClickEvent;
    public event Action<PointerEventData> OnLeftClickEvent;
    public event Action<PointerEventData> OnMiddleClickEvent;

    public event Action<PointerEventData> OnPointerUpEvent;
    public event Action<PointerEventData> OnPointerEnterEvent;
    public event Action<PointerEventData> OnPointerExitEvent;
    public event Action<PointerEventData> OnBeginDragEvent;
    public event Action<PointerEventData> OnDragEvent;
    public event Action<PointerEventData> OnEndDragEvent;
    public event Action<PointerEventData> OnScrollEvent;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownEvent?.Invoke(eventData);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClickEvent?.Invoke(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClickEvent?.Invoke(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            OnMiddleClickEvent?.Invoke(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpEvent?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragEvent?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragEvent?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragEvent?.Invoke(eventData);
    }

    public void OnScroll(PointerEventData eventData)
    {
        OnScrollEvent?.Invoke(eventData);
    }
}
