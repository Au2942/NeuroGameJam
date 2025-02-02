using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;  

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string header;
    [SerializeField][TextArea(3, 10)] private string content;
    [SerializeField] private float delay = 0.5f;

    private bool isHovering;
    private Coroutine delayTooltip;

    public void SetTooltip(string header, string content)
    {
        this.header = header;
        this.content = content;
    }
    public void SetTooltipHeader(string header)
    {
        this.header = header;
    }

    public void SetTooltipContent(string content)
    {
        this.content = content;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(string.IsNullOrEmpty(header) || string.IsNullOrEmpty(content))
        {
            return;
        }
        isHovering = true;
        delayTooltip = StartCoroutine(DelayTooltip());
    }

    private IEnumerator DelayTooltip()
    {
        yield return new WaitForSeconds(delay);
        if(isHovering)
        {
            GameManager.Instance.Tooltip.ShowTooltip(header, content);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        if(delayTooltip != null)
        {
            StopCoroutine(delayTooltip);
        }
        GameManager.Instance.Tooltip.HideTooltip();
    }
}
