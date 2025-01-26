using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerStatsUI : MonoBehaviour
{

    [SerializeField] private RectTransform workerStatUI;
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
    [SerializeField] private Color tempColor;


    void Start()
    {
        ResetStatText();
        HideButtons();
    }


    public void UpdateStatText(Worker worker)
    {
        if(worker != null)
        {
            robustnessText.text = new string ('|', worker.Attributes.Robustness) 
            + "<color=#" + ColorUtility.ToHtmlStringRGB(tempColor) + ">"
            + new string('|', worker.AllocAttributes.Robustness) + "</color>";
            latencyText.text = new string ('|', worker.Attributes.Latency)
            + "<color=#" + ColorUtility.ToHtmlStringRGB(tempColor) + ">"
            + new string('|', worker.AllocAttributes.Latency) + "</color>";
            accuracyText.text = new string ('|', worker.Attributes.Accuracy)
            + "<color=#" + ColorUtility.ToHtmlStringRGB(tempColor) + ">"
            + new string('|', worker.AllocAttributes.Accuracy) + "</color>";
            fitnessText.text = new string ('|', worker.Attributes.Fitness)
            + "<color=#" + ColorUtility.ToHtmlStringRGB(tempColor) + ">"
            + new string('|', worker.AllocAttributes.Fitness) + "</color>";
        }
    }

    public void ResetStatText()
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
