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
    [Header("Attributes")]
    [SerializeField] public Button HeartAddButton;
    [SerializeField] public Button HeartSubtractButton;
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] public Button AddErrorRecoveryButton;
    [SerializeField] public Button SubtractErrorRecoveryButton;
    [SerializeField] private TextMeshProUGUI errorRecoveryText;
    [SerializeField] public Button AccuracyAddButton;
    [SerializeField] public Button AccuracySubtractButton;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] public Button LatencyAddButton;
    [SerializeField] public Button LatencySubtractButton;
    [SerializeField] private TextMeshProUGUI latencyText;
    [Header("Tooltip")]
    [SerializeField] public TooltipTrigger HeartTooltip;
    [SerializeField][TextArea(3,5)] public string HeartBaseTooltip = "Robustness of the worker.";
    [SerializeField] public TooltipTrigger ErrorRecoveryTooltip;
    [SerializeField][TextArea(3,5)] public string ErrorRecoveryBaseTooltip = "Error Restoration Metric. Memory restoration capability of the worker.";
    [SerializeField] public TooltipTrigger AccuracyTooltip;
    [SerializeField][TextArea(3,5)] public string AccuracyBaseTooltip = "Problem identifying accuracy and prediction reliability of the worker.";
    [SerializeField] public TooltipTrigger LatencyTooltip;
    [SerializeField][TextArea(3,5)] public string LatencyBaseTooltip = "Task Update, Transmission and Execution Latency. Responsiveness of the worker.";
    [Header("Colors")]
    [SerializeField] private Color allocColor = new Color(1,1,1);
    [SerializeField] private Color buffColor = new Color(1,1,1);
    [SerializeField] private Color debuffColor = new Color(1,1,1);
    public List<StatusEffectIcon> statusEffectIcons = new List<StatusEffectIcon>();


    private string allocColorHex;
    private string buffColorHex;
    private string debuffColorHex;
    void Start()
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();
        allocColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").ToString();
        sb.Clear();
        buffColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(buffColor)).Append(">").ToString();
        sb.Clear();
        debuffColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(debuffColor)).Append(">").ToString();
        ClearAttributesAndStatsText();
        HideButtons();
    }

    public void UpdateDisplayDetails(Worker worker)
    {
        if(worker != null)
        {
            UpdateInfos(worker);
            UpdateAttributesAndStatsText(worker);
        }
        else
        {
            ClearDisplayDetails();
        }
    }

    private void UpdateInfos(Worker worker)
    {
        ClearInfos();

        StringBuilder sb = new StringBuilder();
        if(worker == null) return;
        sb.Clear();
        sb = sb.Append("ID: ").Append(worker.Name);
        workerNameText.text = sb.ToString();

        foreach(StatusEffect statusEffect in worker.StatusEffects)
        {
            StatusEffectData data = statusEffect.GetData();
            StatusEffectIcon icon = StatusEffectIconManager.Instance.GetStatusEffectIcon(data.ID);
            icon.transform.SetParent(statusEffectsRect);
            statusEffectIcons.Add(icon);
        }
    }

    private void ClearInfos()
    {
        workerNameText.text = "ID :";
        for(int i = 0; i < statusEffectIcons.Count; i++)
        {
            StatusEffectIcon icon = statusEffectIcons[i];
            StatusEffectIconManager.Instance.ReturnStatusEffectIcon(icon);
            statusEffectIcons.Remove(icon);
        }
    }

    private void UpdateAttributesAndStatsText(Worker worker)
    {
        if (worker == null) return;

        WorkerAttributes baseAttribute = worker.BaseAttributes;
        WorkerAttributes allocAttributes = worker.AllocAttributes;
        WorkerAttributes tempAttributes = worker.TempAttributes;
        //WorkerAttributes allocedTotalAttributes = totalAttributes + allocAttributes;


        heartText.text = FormatAttributeText(baseAttribute.Heart, allocAttributes.Heart, tempAttributes.Heart);
        errorRecoveryText.text = FormatAttributeText(baseAttribute.ErrorRecovery, allocAttributes.ErrorRecovery, tempAttributes.ErrorRecovery);
        accuracyText.text = FormatAttributeText(baseAttribute.Accuracy, allocAttributes.Accuracy, tempAttributes.Accuracy);
        latencyText.text = FormatAttributeText(baseAttribute.Latency, allocAttributes.Latency, tempAttributes.Latency);

        UpdateTooltips(worker, worker.BaseStats, 
            worker.workerData.CalculateStatsFromAttributes(allocAttributes), 
            worker.TempStats + worker.workerData.CalculateStatsFromAttributes(tempAttributes));
    }

    private string FormatAttributeText(int baseVal, int allocVal, int tempVal)
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();
        string tier = NumberToTier(baseVal);
        sb.Append(tier);
        if (allocVal > 0)
        {
            sb.Append(allocColorHex).Append("(").Append(NumberToTier(baseVal+allocVal)).Append(")</color>");
        }
        if (tempVal > 0)
        {
            sb.Append(buffColorHex).Append("(").Append(NumberToTier(baseVal+allocVal+tempVal)).Append(")</color>");
        }
        else if(tempVal < 0)
        {
            sb.Append(debuffColorHex).Append("(").Append(NumberToTier(baseVal+allocVal+tempVal)).Append(")</color>");
        }
        return sb.ToString();
    }

    private void UpdateTooltips(Worker worker, WorkerStats baseStats, WorkerStats allocStats, WorkerStats tempStats)
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();
        sb.Append(HeartBaseTooltip).Append("\nHealth: ").Append(worker.workerData.Health).Append("/");
        sb.Append(FormatStatText(baseStats.MaxHealth, allocStats.MaxHealth, tempStats.MaxHealth));
        sb.Append("\nHealth Regen: 1/").Append(FormatStatText(baseStats.RegenTime, allocStats.RegenTime, tempStats.RegenTime, false)).Append("s");
        HeartTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(ErrorRecoveryBaseTooltip).Append("\nRestoration Amount: ");
        sb.Append(FormatStatText(baseStats.WorkAmount, allocStats.WorkAmount, tempStats.WorkAmount));
        ErrorRecoveryTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(AccuracyBaseTooltip).Append("\nWork Success Chance: ");
        sb.Append(FormatStatText(baseStats.WorkSuccessChance, allocStats.WorkSuccessChance, tempStats.WorkSuccessChance)).Append("%");
        sb.Append("\nDamage Avoidance Chance: ");
        sb.Append(FormatStatText(baseStats.DamageAvoidanceChance, allocStats.DamageAvoidanceChance, tempStats.DamageAvoidanceChance)).Append("%");
        AccuracyTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(LatencyBaseTooltip).Append("\nWork Speed: ");
        sb.Append(FormatStatText(baseStats.WorkTime, allocStats.WorkTime, tempStats.WorkTime, false)).Append("s");
        sb.Append("\nRecall Cooldown: ").Append(FormatStatText(baseStats.RecallTime, allocStats.RecallTime, tempStats.RecallTime, false)).Append("s");
        LatencyTooltip.SetTooltipContent(sb.ToString());
    }

    private string FormatStatText(float baseVal, float allocVal, float tempVal, bool moreIsBetter = true)
    {

        StringBuilder sb = new StringBuilder();
        sb.Clear();
        sb.Append(baseVal);
        if (allocVal > 0)
        {
            sb.Append(allocColorHex).Append("(").Append("+").Append(allocVal).Append(")</color>");
        }
        else if(allocVal < 0)
        {
            sb.Append(allocColorHex).Append("(").Append(allocVal).Append(")</color>");
        }
        if (tempVal > 0)
        {
            sb.Append(moreIsBetter? buffColorHex : debuffColorHex).Append("(").Append("+").Append(tempVal).Append(")</color>");
        }
        else if(tempVal < 0)
        {
            sb.Append(moreIsBetter? debuffColorHex : buffColorHex).Append("(").Append(tempVal).Append(")</color>");
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

    private void ClearAttributesAndStatsText()
    {
        heartText.text = "";
        latencyText.text = "";
        accuracyText.text = "";
        errorRecoveryText.text = "";

        HeartTooltip.SetTooltipContent(HeartBaseTooltip);
        ErrorRecoveryTooltip.SetTooltipContent(ErrorRecoveryBaseTooltip);
        AccuracyTooltip.SetTooltipContent(AccuracyBaseTooltip);
        LatencyTooltip.SetTooltipContent(LatencyBaseTooltip);
    }

    public void ClearDisplayDetails()
    {
        ClearInfos();
        ClearAttributesAndStatsText();
    }

    public void ShowButtons()
    {
        NewButton.gameObject.SetActive(true);
        SideButtonsRect.gameObject.SetActive(true);
        HeartAddButton.gameObject.SetActive(true);
        HeartSubtractButton.gameObject.SetActive(true);
        AddErrorRecoveryButton.gameObject.SetActive(true);
        SubtractErrorRecoveryButton.gameObject.SetActive(true);
        AccuracyAddButton.gameObject.SetActive(true);
        AccuracySubtractButton.gameObject.SetActive(true);
        LatencyAddButton.gameObject.SetActive(true);
        LatencySubtractButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        NewButton.gameObject.SetActive(false);
        SideButtonsRect.gameObject.SetActive(false);
        HeartAddButton.gameObject.SetActive(false);
        HeartSubtractButton.gameObject.SetActive(false);
        AddErrorRecoveryButton.gameObject.SetActive(false);
        SubtractErrorRecoveryButton.gameObject.SetActive(false);
        AccuracyAddButton.gameObject.SetActive(false);
        AccuracySubtractButton.gameObject.SetActive(false);
        LatencyAddButton.gameObject.SetActive(false);
        LatencySubtractButton.gameObject.SetActive(false);
    }
    

}
