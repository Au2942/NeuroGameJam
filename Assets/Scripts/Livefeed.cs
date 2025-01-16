using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Livefeed : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Image integrityBar;
    [SerializeField] public Color[] integrityColorStages;
    [SerializeField] public TextMeshProUGUI livefeedNameText; 
    public string LivefeedName {get; set;}
    public int LivefeedIndex {get; set;} = -1;
    public void OnPointerClick(PointerEventData eventData)
    {
        ChannelNavigationManager.Instance.SetChannelIndex(LivefeedIndex);
    }
    
    public void SetLivefeed(string name,int newIndex)
    {
        SetLivefeedName(name);
        LivefeedIndex = newIndex;
    }

    public void SetLivefeedName(string name)
    {
        LivefeedName = name;
        livefeedNameText.text = name;
    }

    void Update()
    {
        if(!GameManager.Instance.isStreaming)
        {
            return;
        }
        UpdateIntegrityBar();
    }

    private void UpdateIntegrityBar()
    {
        float integrity = GameManager.Instance.Entities[LivefeedIndex].Integrity;
        float maxIntegrity = GameManager.Instance.Entities[LivefeedIndex].MaxIntegrity;
        float percentage = integrity / maxIntegrity;
        int stage = Mathf.CeilToInt(percentage * (integrityColorStages.Length - 1)); 
        integrityBar.color = integrityColorStages[stage];
        integrityBar.fillAmount = percentage;
    }

}
