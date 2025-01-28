using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerDetailsUI : MonoBehaviour
{

    [SerializeField] private RectTransform workerDetailsUI;
    [SerializeField] private RectTransform SideButtonsUI;
    [SerializeField] public Button ResetButton;
    [SerializeField] public Button DeleteButton;
    [SerializeField] public Button NewButton;
    [SerializeField] public Button AddRobustnessButton;
    [SerializeField] private TextMeshProUGUI robustnessText;
    [SerializeField] public Button SubtractRobustnessButton;
    [SerializeField] public Button AddLatencyButton;
    [SerializeField] private TextMeshProUGUI latencyText;
    [SerializeField] public Button SubtractLatencyButton;
    [SerializeField] public Button AddAccuracyButton;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] public Button SubtractAccuracyButton;
    [SerializeField] public Button AddFitnessButton;
    [SerializeField] private TextMeshProUGUI fitnessText;
    [SerializeField] public Button SubtractFitnessButton;
    [SerializeField] private Color allocColor;
    [SerializeField] private Color modifyColor;


    void Start()
    {
        ResetAttributesText();
        HideButtons();
    }


    public void UpdateAttributesText(Worker worker)
    {
        if(worker != null)
        {

            int robustness = worker.Attributes.Robustness + worker.AllocAttributes.Robustness + worker.TempAttributes.Robustness;
            int latency = worker.Attributes.Latency + worker.AllocAttributes.Latency + worker.TempAttributes.Latency;
            int accuracy = worker.Attributes.Accuracy + worker.AllocAttributes.Accuracy + worker.TempAttributes.Accuracy;
            int fitness = worker.Attributes.Fitness + worker.AllocAttributes.Fitness + worker.TempAttributes.Fitness;

            if(worker.AllocAttributes.Robustness > 0)
            {
                robustnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(allocColor) + ">" + NumberToTier(robustness) + "</color>";
            }
            else
            {
                if(worker.TempAttributes.Robustness > 0)
                {
                    robustnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(modifyColor) + ">" + NumberToTier(robustness) + "</color>";
                }
                else
                {
                    robustnessText.text = NumberToTier(robustness);
                }
            }

            if(worker.AllocAttributes.Latency > 0)
            {
                latencyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(allocColor) + ">" + NumberToTier(latency) + "</color>";
            }
            else
            {
                if(worker.TempAttributes.Latency > 0)
                {
                    latencyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(modifyColor) + ">" + NumberToTier(latency) + "</color>";
                }
                else
                {
                    latencyText.text = NumberToTier(latency);
                }
            }

            if(worker.AllocAttributes.Accuracy > 0)
            {
                accuracyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(allocColor) + ">" + NumberToTier(accuracy) + "</color>";
            }
            else
            {
                if(worker.TempAttributes.Accuracy > 0)
                {
                    accuracyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(modifyColor) + ">" + NumberToTier(accuracy) + "</color>";
                }
                else
                {
                    accuracyText.text = NumberToTier(accuracy);
                }
            }

            if(worker.AllocAttributes.Fitness > 0)
            {
                fitnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(allocColor) + ">" + NumberToTier(fitness) + "</color>";
            }
            else
            {
                if(worker.TempAttributes.Fitness > 0)
                {
                    fitnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(modifyColor) + ">" + NumberToTier(fitness) + "</color>";
                }
                else
                {
                    fitnessText.text = NumberToTier(fitness);
                }
            }
        }
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

    public void ResetAttributesText()
    {
        robustnessText.text = "";
        latencyText.text = "";
        accuracyText.text = "";
        fitnessText.text = "";
    }

    public void ShowButtons()
    {
        NewButton.gameObject.SetActive(true);
        SideButtonsUI.gameObject.SetActive(true);
        AddRobustnessButton.gameObject.SetActive(true);
        SubtractRobustnessButton.gameObject.SetActive(true);
        AddLatencyButton.gameObject.SetActive(true);
        SubtractLatencyButton.gameObject.SetActive(true);
        AddAccuracyButton.gameObject.SetActive(true);
        SubtractAccuracyButton.gameObject.SetActive(true);
        AddFitnessButton.gameObject.SetActive(true);
        SubtractFitnessButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        NewButton.gameObject.SetActive(false);
        SideButtonsUI.gameObject.SetActive(false);
        AddRobustnessButton.gameObject.SetActive(false);
        SubtractRobustnessButton.gameObject.SetActive(false);
        AddLatencyButton.gameObject.SetActive(false);
        SubtractLatencyButton.gameObject.SetActive(false);
        AddAccuracyButton.gameObject.SetActive(false);
        SubtractAccuracyButton.gameObject.SetActive(false);
        AddFitnessButton.gameObject.SetActive(false);
        SubtractFitnessButton.gameObject.SetActive(false);
    }
    

}
