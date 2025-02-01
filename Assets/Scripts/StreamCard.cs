using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    public event System.Action<StreamSO> OnSelect;
    public System.Action<PointerEventData> OnPointerUpEventHandler;
    public System.Action<PointerEventData> OnPointerEnterHandler;
    public System.Action<PointerEventData> OnPointerExitHandler;

    void Awake()
    {
        OnPointerUpEventHandler = (t) => Select();
        OnPointerEnterHandler = (t) => outline.color = originalColor;
        OnPointerExitHandler = (t) => outline.color = transparentColor;
    }
    void OnEnable()
    {
        cardEventHandler.OnPointerUpEvent += OnPointerUpEventHandler;
        cardEventHandler.OnPointerEnterEvent += OnPointerEnterHandler;
        cardEventHandler.OnPointerExitEvent += OnPointerExitHandler;
    }
    void Start()
    {
        originalColor = outline.color;
        transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
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

    void OnDisable()
    {
        cardEventHandler.OnPointerUpEvent -= OnPointerUpEventHandler;
        cardEventHandler.OnPointerEnterEvent -= OnPointerEnterHandler;
        cardEventHandler.OnPointerExitEvent -= OnPointerExitHandler;
    }
}
