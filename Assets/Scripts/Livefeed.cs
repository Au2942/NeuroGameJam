using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Livefeed : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] public TextMeshProUGUI livefeedNameText; 
    [SerializeField] IntegrityIndicator integrityIndicator;
    public string LivefeedName {get; set;}
    public int LivefeedIndex {get; set;} = -1;
    public void OnPointerClick(PointerEventData eventData)
    {
        ChannelNavigationManager.Instance.SetChannelIndex(LivefeedIndex);
    }
    
    public void SetLivefeed(string name, int newIndex)
    {
        SetLivefeedName(name);
        LivefeedIndex = newIndex;
        integrityIndicator.SetEntityIndex(newIndex);
    }
    

    public void SetLivefeedName(string name)
    {
        LivefeedName = name;
        livefeedNameText.text = name;
    }



}
