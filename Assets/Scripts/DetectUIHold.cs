using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class DetectUIHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public event Action<PointerEventData> OnBeginHoldEvent;
    public event Action<PointerEventData> OnEndHoldEvent;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnBeginHoldEvent?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndHoldEvent?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnEndHoldEvent?.Invoke(eventData);
    }
}


