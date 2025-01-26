using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Livefeed : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] public TextMeshProUGUI livefeedNameText; 
    public string LivefeedName {get; set;}
    public int LivefeedIndex {get; set;} = -1;
    public void OnPointerClick(PointerEventData eventData)
    {
        ChannelNavigationManager.Instance.SetChannelIndex(LivefeedIndex);
    }

    public void OnScroll()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        ScrollRect scrollRect = LivefeedManager.Instance.LivefeedScroller.scrollRect;
        float viewportWidth = scrollRect.viewport.rect.width;
        float contentWidth = scrollRect.content.rect.width;
        float contentOffset = scrollRect.content.anchoredPosition.x;
        float center = contentWidth/2 - contentOffset;
        float distanceFromCenter = Mathf.Abs(rectTransform.anchoredPosition.x - center);
        float scale = 1f;
        if(distanceFromCenter < 360f)
        {
            scale = 2f - distanceFromCenter / 360f;
        }
        rectTransform.sizeDelta = new Vector2(320 * scale, 180 * scale);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    
    }
    
    public void SetLivefeed(string name, int newIndex)
    {
        SetLivefeedName(name);
        LivefeedIndex = newIndex;
    }
    

    public void SetLivefeedName(string name)
    {
        LivefeedName = name;
        livefeedNameText.text = name;
    }



}
