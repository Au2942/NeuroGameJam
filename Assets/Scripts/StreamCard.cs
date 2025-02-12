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
    public System.Action<PointerEventData> OnPointerUpDelegate;
    public System.Action<PointerEventData> OnPointerEnterDelegate;
    public System.Action<PointerEventData> OnPointerExitDelegate;

    void Awake()
    {
        OnPointerUpDelegate = (t) => Select();
        OnPointerEnterDelegate = (t) => outline.color = originalColor;
        OnPointerExitDelegate = (t) => outline.color = transparentColor;
    }
    void OnEnable()
    {
        cardEventHandler.OnPointerUpEvent += OnPointerUpDelegate;
        cardEventHandler.OnPointerEnterEvent += OnPointerEnterDelegate;
        cardEventHandler.OnPointerExitEvent += OnPointerExitDelegate;
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
        cardEventHandler.OnPointerUpEvent -= OnPointerUpDelegate;
        cardEventHandler.OnPointerEnterEvent -= OnPointerEnterDelegate;
        cardEventHandler.OnPointerExitEvent -= OnPointerExitDelegate;
    }
}
