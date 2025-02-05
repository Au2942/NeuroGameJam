using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class WorkerDetails : MonoBehaviour
{
    [SerializeField] private RectTransform workerDetailsRect;
    [SerializeField] private RectTransform SideButtonsRect;
    [SerializeField] private RectTransform statusEffectsRect;
    [SerializeField] private TextMeshProUGUI workerNameText;
    [SerializeField] public Button ResetButton;
    [SerializeField] public Button DeleteButton;
    [SerializeField] public Button NewButton;
    [SerializeField] public Button SubtractHeartButton;
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] public Button AddHeartButton;
    [SerializeField] public Button SubtractERMButton;
    [SerializeField] private TextMeshProUGUI ERMText;
    [SerializeField] public Button AddERMButton;
    [SerializeField] public Button SubtractAccuracyButton;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] public Button AddAccuracyButton;
    [SerializeField] public Button SubtractLatencyButton;
    [SerializeField] private TextMeshProUGUI latencyText;
    [SerializeField] public Button AddLatencyButton;
    [SerializeField] private Color allocColor = new Color(1,1,1);
    [SerializeField] private Color modifyColor = new Color(1,1,1);
    private List<StatusEffectIcon> statusEffectIcons = new List<StatusEffectIcon>();
    private StringBuilder sb = new StringBuilder();
    void Start()
    {
        ClearAttributesText();
        HideButtons();
    }

    public void UpdateDisplayDetails(Worker worker)
    {
        if(worker != null)
        {
            UpdateInfos(worker);
            UpdateAttributesText(worker);
        }
    }

    private void UpdateInfos(Worker worker)
    {
        if(worker == null) return;
        sb.Clear();
        sb = sb.Append("ID: ").Append(worker.Name);
        workerNameText.text = sb.ToString();

        for(int i = 0; i < statusEffectIcons.Count; i++)
        {
            StatusEffectIcon icon = statusEffectIcons[i];
            StatusEffectIconManager.Instance.ReturnStatusEffectIcon(icon);
            statusEffectIcons.Remove(icon);
        }

        foreach(StatusEffect statusEffect in worker.StatusEffects)
        {
            StatusEffectData data = statusEffect.GetData();
            StatusEffectIcon icon = StatusEffectIconManager.Instance.GetStatusEffectIcon(data.ID);
            icon.transform.SetParent(statusEffectsRect);
            statusEffectIcons.Add(icon);
        }
    }

    private void UpdateAttributesText(Worker worker)
    {
        if (worker == null) return;

        int heart = worker.BaseAttributes.Heart + worker.AllocAttributes.Heart + worker.TempAttributes.Heart;
        int ERM = worker.BaseAttributes.ERM + worker.AllocAttributes.ERM + worker.TempAttributes.ERM;
        int accuracy = worker.BaseAttributes.Accuracy + worker.AllocAttributes.Accuracy + worker.TempAttributes.Accuracy;
        int latency = worker.BaseAttributes.Latency + worker.AllocAttributes.Latency + worker.TempAttributes.Latency;

        heartText.text = FormatAttributeText(heart, worker.AllocAttributes.Heart, worker.TempAttributes.Heart);
        ERMText.text = FormatAttributeText(ERM, worker.AllocAttributes.ERM, worker.TempAttributes.ERM);
        accuracyText.text = FormatAttributeText(accuracy, worker.AllocAttributes.Accuracy, worker.TempAttributes.Accuracy);
        latencyText.text = FormatAttributeText(latency, worker.AllocAttributes.Latency, worker.TempAttributes.Latency);
    }

    private string FormatAttributeText(int totalVal, int allocVal, int tempVal)
    {
        sb.Clear();
        string tier = NumberToTier(totalVal);
        if (allocVal > 0)
        {
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").Append(tier).Append("</color>");
        }
        else if (tempVal > 0)
        {
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(modifyColor)).Append(">").Append(tier).Append("</color>");
        }
        else
        {
            sb.Append(tier);
        }
        return sb.ToString();
    }


    private string NumberToTier(int num)
    {
        switch(num)
        {
            case 1:
                return "E";
            case 2:
                return "D";
            case 3:
                return "C";
            case 4: 
                return "B";
            case 5:      
                return "A";
            case 6:
                return "S";
            case >6:
                return "S+";
            default:
                return "F";
        }
    }

    public void ClearAttributesText()
    {
        heartText.text = "";
        latencyText.text = "";
        accuracyText.text = "";
        ERMText.text = "";
    }

    public void ShowButtons()
    {
        NewButton.gameObject.SetActive(true);
        SideButtonsRect.gameObject.SetActive(true);
        AddHeartButton.gameObject.SetActive(true);
        SubtractHeartButton.gameObject.SetActive(true);
        AddERMButton.gameObject.SetActive(true);
        SubtractERMButton.gameObject.SetActive(true);
        AddAccuracyButton.gameObject.SetActive(true);
        SubtractAccuracyButton.gameObject.SetActive(true);
        AddLatencyButton.gameObject.SetActive(true);
        SubtractLatencyButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        NewButton.gameObject.SetActive(false);
        SideButtonsRect.gameObject.SetActive(false);
        AddHeartButton.gameObject.SetActive(false);
        SubtractHeartButton.gameObject.SetActive(false);
        AddERMButton.gameObject.SetActive(false);
        SubtractERMButton.gameObject.SetActive(false);
        AddAccuracyButton.gameObject.SetActive(false);
        SubtractAccuracyButton.gameObject.SetActive(false);
        AddLatencyButton.gameObject.SetActive(false);
        SubtractLatencyButton.gameObject.SetActive(false);
    }
    

}
