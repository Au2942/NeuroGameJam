using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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
    [SerializeField] private Color allocColor = new Color(1,1,1);
    [SerializeField] private Color modifyColor = new Color(1,1,1);

    private StringBuilder sb = new StringBuilder();
    void Start()
    {
        ResetAttributesText();
        HideButtons();
    }


    public void UpdateAttributesText(Worker worker)
    {
        if (worker != null)
        {
            int robustness = worker.BaseAttributes.Robustness + worker.AllocAttributes.Robustness + worker.TempAttributes.Robustness;
            int latency = worker.BaseAttributes.Latency + worker.AllocAttributes.Latency + worker.TempAttributes.Latency;
            int accuracy = worker.BaseAttributes.Accuracy + worker.AllocAttributes.Accuracy + worker.TempAttributes.Accuracy;
            int fitness = worker.BaseAttributes.Fitness + worker.AllocAttributes.Fitness + worker.TempAttributes.Fitness;

            sb.Clear();
            if (worker.AllocAttributes.Robustness > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").Append(NumberToTier(robustness)).Append("</color>");
            }
            else if (worker.TempAttributes.Robustness > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(modifyColor)).Append(">").Append(NumberToTier(robustness)).Append("</color>");
            }
            else
            {
                sb.Append(NumberToTier(robustness));
            }
            robustnessText.text = sb.ToString();

            sb.Clear();
            if (worker.AllocAttributes.Latency > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").Append(NumberToTier(latency)).Append("</color>");
            }
            else if (worker.TempAttributes.Latency > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(modifyColor)).Append(">").Append(NumberToTier(latency)).Append("</color>");
            }
            else
            {
                sb.Append(NumberToTier(latency));
            }
            latencyText.text = sb.ToString();

            sb.Clear();
            if (worker.AllocAttributes.Accuracy > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").Append(NumberToTier(accuracy)).Append("</color>");
            }
            else if (worker.TempAttributes.Accuracy > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(modifyColor)).Append(">").Append(NumberToTier(accuracy)).Append("</color>");
            }
            else
            {
                sb.Append(NumberToTier(accuracy));
            }
            accuracyText.text = sb.ToString();

            sb.Clear();
            if (worker.AllocAttributes.Fitness > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(allocColor)).Append(">").Append(NumberToTier(fitness)).Append("</color>");
            }
            else if (worker.TempAttributes.Fitness > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(modifyColor)).Append(">").Append(NumberToTier(fitness)).Append("</color>");
            }
            else
            {
                sb.Append(NumberToTier(fitness));
            }
            fitnessText.text = sb.ToString();
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
