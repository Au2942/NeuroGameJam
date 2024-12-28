using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StreamCard : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI streamDesc; 
    [SerializeField] private StreamSO stream;

    public event Action<StreamSO> OnSelect;

    public void Setup(StreamCardSO streamCardSO)
    {
        icon.sprite = streamCardSO.icon;
        streamDesc.text = streamCardSO.streamDesc;
        stream = streamCardSO.stream;
    }


    public void Select()
    {
        Debug.Log("Card Selected");
        OnSelect?.Invoke(stream);
    }
}
