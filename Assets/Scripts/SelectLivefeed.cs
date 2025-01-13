using UnityEngine;
using UnityEngine.EventSystems;

public class SelectLivefeed : MonoBehaviour, IPointerClickHandler
{
    private int index {get; set;} = -1;
    public void OnPointerClick(PointerEventData eventData)
    {
        TimelineManager.Instance.SetEntityIndex(TimelineManager.Instance.entitiesCount - index - 1);
    }
    
    public void SetIndex(int newIndex)
    {
        index = newIndex;
    }

}
