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
    [SerializeField][TextArea(3,5)] public string HeartBaseTooltip = "Health Assessment and Recovery Time.";
    [SerializeField] public TooltipTrigger ErrorRecoveryTooltip;
    [SerializeField][TextArea(3,5)] public string ErrorRecoveryBaseTooltip = "Error Restoration Metric. Memory restoration capability of the worker.";
    [SerializeField] public TooltipTrigger AccuracyTooltip;
    [SerializeField][TextArea(3,5)] public string AccuracyBaseTooltip = "Accuracy & Reliability. Task success rate and operation reliability.";
    [SerializeField] public TooltipTrigger LatencyTooltip;
    [SerializeField][TextArea(3,5)] public string LatencyBaseTooltip = "Task Update, Transmission and Execution Latency. Responsiveness of the worker.";
    [Header("Colors")]
    [SerializeField] public Color AllocColor = new Color(1,1,1);
    [SerializeField] public Color BuffColor = new Color(1,1,1);
    [SerializeField] public Color DebuffColor = new Color(1,1,1);
    public List<StatusEffectIcon> statusEffectIcons = new List<StatusEffectIcon>();


    public string AllocColorHex;
    public string BuffColorHex;
    public string DebuffColorHex;
    void Start()
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();
        AllocColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(AllocColor)).Append(">").ToString();
        sb.Clear();
        BuffColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(BuffColor)).Append(">").ToString();
        sb.Clear();
        DebuffColorHex = sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(DebuffColor)).Append(">").ToString();
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
        workerNameText.text = "ID:";
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
            sb.Append(AllocColorHex).Append("(").Append(NumberToTier(baseVal+allocVal)).Append(")</color>");
        }
        if (tempVal > 0)
        {
            sb.Append(BuffColorHex).Append("(").Append(NumberToTier(tempVal)).Append(")</color>");
        }
        else if(tempVal < 0)
        {
            sb.Append(DebuffColorHex).Append("(").Append(NumberToTier(tempVal)).Append(")</color>");
        }
        return sb.ToString();
    }

    private void UpdateTooltips(Worker worker, WorkerStats baseStats, WorkerStats allocStats, WorkerStats tempStats)
    {
        StringBuilder sb = new StringBuilder();
        sb.Clear();
        sb.Append(HeartBaseTooltip).Append("\nHeart Capacity: ").Append(worker.workerData.Health).Append("/");
        sb.Append(FormatStatText(baseStats.MaxHealth, allocStats.MaxHealth, tempStats.MaxHealth));
        sb.Append("\nMend Rate: ").Append(FormatStatText(baseStats.RegenTime, allocStats.RegenTime, tempStats.RegenTime, 100,false)).Append("ms");
        HeartTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(ErrorRecoveryBaseTooltip).Append("\nRestoration Yield: ");
        sb.Append(FormatStatText(baseStats.RestoreAmount, allocStats.RestoreAmount, tempStats.RestoreAmount));
        sb.Append("\nExecution Iterations: ").Append(FormatStatText(baseStats.TaskExecutionCount, allocStats.TaskExecutionCount, tempStats.TaskExecutionCount));
        ErrorRecoveryTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(AccuracyBaseTooltip).Append("\nExecution Accuracy: ");
        sb.Append(FormatStatText(baseStats.TaskSuccessChance, allocStats.TaskSuccessChance, tempStats.TaskSuccessChance)).Append("%");
        sb.Append("\nOperation Reliability: ");
        sb.Append(FormatStatText(baseStats.OperationReliability, allocStats.OperationReliability, tempStats.OperationReliability)).Append("%");
        AccuracyTooltip.SetTooltipContent(sb.ToString());

        sb.Clear();
        sb.Append(LatencyBaseTooltip).Append("\nExecution Time: ");
        sb.Append(FormatStatText(baseStats.TaskTime, allocStats.TaskTime, tempStats.TaskTime, 100, false)).Append("ms");
        sb.Append("\nResponse Latency: ").Append(FormatStatText(baseStats.ResponseTime, allocStats.ResponseTime, tempStats.ResponseTime, 100, false)).Append("ms");
        LatencyTooltip.SetTooltipContent(sb.ToString());
    }

    private string FormatStatText(float baseVal, float allocVal, float tempVal, float multiplier = 1, bool moreIsBetter = true)
    {

        StringBuilder sb = new StringBuilder();
        sb.Clear();
        sb.Append(baseVal*multiplier);
        if (allocVal > 0)
        {
            sb.Append(AllocColorHex).Append("(").Append("+").Append(allocVal*multiplier).Append(")</color>");
        }
        else if(allocVal < 0)
        {
            sb.Append(AllocColorHex).Append("(").Append(allocVal*multiplier).Append(")</color>");
        }
        if (tempVal > 0)
        {
            sb.Append(moreIsBetter? BuffColorHex : DebuffColorHex).Append("+").Append(tempVal*multiplier).Append("</color>");
        }
        else if(tempVal < 0)
        {
            sb.Append(moreIsBetter? DebuffColorHex : BuffColorHex).Append(tempVal*multiplier).Append("</color>");
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
