using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private RectTransform tooltipBox;
    [SerializeField] private TextMeshProUGUI tooltipHeader;
    [SerializeField] private TextMeshProUGUI tooltipContent;
    [SerializeField] private LayoutElement layoutElement;
    private RectTransform headerRect;
    private RectTransform contentRect;
    void Awake()
    {
        headerRect = tooltipHeader.GetComponent<RectTransform>();
        contentRect = tooltipContent.GetComponent<RectTransform>();
    }

    public void SetText(string header, string content)
    {
        tooltipHeader.text = header;
        tooltipContent.text = content;
    }
    
    public void ShowTooltip(string header, string content)
    {
        tooltipHeader.text = header;
        tooltipContent.text = content;
        tooltipBox.gameObject.SetActive(true);
        UpdateLayoutElement();
        UpdateTransform();
    }

    void Update()
    {
        if (!tooltipBox.gameObject.activeSelf) return;
        UpdateLayoutElement();
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        tooltipRect.position = InputManager.Instance.Point.ReadValue<Vector2>();
        float rightEdge = tooltipRect.position.x + tooltipBox.anchoredPosition.x + tooltipBox.rect.width;
        //Debug.Log("x: " + rightEdge + "," + tooltipRect.position.x  + "," + tooltipBox.anchoredPosition.x + "," + tooltipBox.rect.width);
        float bottomEdge = tooltipRect.position.y + tooltipBox.anchoredPosition.y - tooltipBox.rect.height;
        //Debug.Log("y: " + bottomEdge + "," + tooltipRect.position.y + "," + tooltipBox.anchoredPosition.y + "," + tooltipBox.rect.height);
        if (rightEdge > Screen.width)
        {
            tooltipRect.position = new Vector2(tooltipRect.position.x - (rightEdge - Screen.width), tooltipRect.position.y);
        }
        if (bottomEdge < 0)
        {
            tooltipRect.position = new Vector2(tooltipRect.position.x, tooltipRect.position.y - bottomEdge);
        }
    }

    private void UpdateLayoutElement()
    {
        layoutElement.enabled = tooltipContent.preferredWidth > layoutElement.preferredWidth || tooltipHeader.preferredWidth > layoutElement.preferredWidth;
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipBox);
    }

    public void HideTooltip()
    {
        tooltipBox.gameObject.SetActive(false);
    }

}