using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MemoryCell : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] public TextMeshProUGUI MemoryDisplayNameText; 
    public MemoryNavigator MemoryNavigator => MemoryManager.Instance.MemoryNavigator;
    public string MemoryDisplayName {get; set;}
    public int MemoryIndex {get; set;} = -1;

    public void OnPointerClick(PointerEventData eventData)
    {
        MemoryManager.Instance.SetCurrentMemoryIndex(MemoryIndex);
    }

    public void SetupMemoryCell(string name, int newIndex)
    {
        SetMemoryDisplayName(name);
        MemoryIndex = newIndex;
    }

    public void SetMemoryDisplayName(string name)
    {
        MemoryDisplayName = name;
        MemoryDisplayNameText.text = MemoryDisplayName;
    }

    public void UpdateCellOnScroll()
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


}
