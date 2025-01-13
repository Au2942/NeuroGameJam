using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StreamCard : MonoBehaviour
{
    [SerializeField] private UIEventHandler cardEventHandler;
    [SerializeField] private Image icon;
    [SerializeField] private Image outline;
    [SerializeField] private TextMeshProUGUI streamDesc; 
    [SerializeField] private StreamSO stream;

    private Color originalColor;
    private Color transparentColor;

    public event Action<StreamSO> OnSelect;

    void Start()
    {
        originalColor = outline.color;
        transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        cardEventHandler.OnPointerDownEvent += (t) => Select();
        cardEventHandler.OnPointerEnterEvent += (t) => outline.color = originalColor;
        cardEventHandler.OnPointerExitEvent += (t) => outline.color = transparentColor;
        outline.color = transparentColor;
    }

    public void Setup(StreamCardSO streamCardSO)
    {
        icon.sprite = streamCardSO.icon;
        outline.sprite = streamCardSO.icon;
        streamDesc.text = streamCardSO.streamDesc;
        stream = streamCardSO.stream;
    }


    public void Select()
    {
        OnSelect?.Invoke(stream);
    }
}
