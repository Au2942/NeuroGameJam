using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class StatusEffectIcon : MonoBehaviour
{
    public TooltipTrigger TooltipTrigger;
    public Image IconImage;
    public StatusEffectData StatusEffectData;
    public WorkerDetails WorkerDetails => WorkerManager.Instance.WorkerDetails;
    private StringBuilder formattedDescriptionBuilder = new StringBuilder();
    private string formattedDescription;


    void Start()
    {
        if(TooltipTrigger == null)
        {
            TooltipTrigger = GetComponent<TooltipTrigger>();
        }
    }

    public void SetupStatusEffectIcon(StatusEffectData data)
    {
        StatusEffectData = data;
        SetSprite(StatusEffectData.Icon);
        StringBuilder descriptionSB = new StringBuilder();
        formattedDescription = GetStatusEffectDescription();
        descriptionSB.Append(formattedDescription);
        SetTooltip(StatusEffectData.Name, descriptionSB.ToString());
    }

    void Update()
    {
        if(StatusEffectData.ExpireAfterLifetime)
        {
            formattedDescriptionBuilder.Clear();
            formattedDescriptionBuilder.Append(formattedDescription).Append("\nExpires in ").Append(StatusEffectData.Lifetime).Append(" s");
            TooltipTrigger.SetTooltipContent(formattedDescriptionBuilder.ToString());
        }
    }

    private string GetStatusEffectDescription()
    {
        StringBuilder descriptionSB = new StringBuilder();
        descriptionSB.Append(StatusEffectData.Description);
        if(StatusEffectData.Stackable)
        {
            descriptionSB.Append("\nStacks: ").Append(StatusEffectData.Stack).Append(" (Max ").Append(StatusEffectData.MaxStack).Append(")\n");
        }

        if(StatusEffectData is WorkerStatusEffectData workerData)
        {
            
            descriptionSB.Append(FormatBuffText(workerData.BuffAttributes.Heart, "HART"));
            descriptionSB.Append(FormatBuffText(workerData.BuffAttributes.ErrorRecovery, "ERM"));
            descriptionSB.Append(FormatBuffText(workerData.BuffAttributes.Accuracy, "A&R"));
            descriptionSB.Append(FormatBuffText(workerData.BuffAttributes.Latency, "TUTEL"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.MaxHealth, "Heart Capacity"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.RegenTime, "Mend Rate", "ms", 100f ,false));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.RestoreAmount, "Restoration Yield"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.TaskExecutionCount, "Execution Iterations"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.TaskSuccessChance, "Execution Accuracy", "%"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.OperationReliability, "Operation Reliability", "%"));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.TaskTime, "Execution Time", "ms", 100f, false));
            descriptionSB.Append(FormatBuffText(workerData.BuffStats.ResponseTime, "Response Latency", "ms", 100f,  false));
        }

        return descriptionSB.ToString();
    }

    private string FormatBuffText(float value, string name, string suffix = "", float multiplier = 1, bool moreIsBetter = true)
    {
        if(value == 0)
        {
            return "";
        }
        
        StringBuilder sb = new StringBuilder();
        sb.Append(name).Append(":\t");
        if (value > 0)
        {
            sb.Append(moreIsBetter? WorkerDetails.BuffColorHex : WorkerDetails.DebuffColorHex).Append("+").Append(value).Append("</color>");
        }
        else if(value < 0)
        {
            sb.Append(moreIsBetter? WorkerDetails.BuffColorHex : WorkerDetails.DebuffColorHex).Append(value).Append("</color>");
        }
        sb.Append(suffix).Append("\n");
        return sb.ToString();
    }

    private void SetTooltip(string name, string description)
    {
        TooltipTrigger.SetTooltip(name, description);
    }

    private void SetSprite(Sprite sprite)
    {
        IconImage.sprite = sprite;
    }
}