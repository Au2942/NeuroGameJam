using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerStatsUI : MonoBehaviour
{

    [SerializeField] private WorkerManager workerManager;
    [SerializeField] private RectTransform workerStatUI;
    [SerializeField] private TextMeshProUGUI robustnessText;
    [SerializeField] private Color robustnessColor;
    [SerializeField] private Button robustnessButton;
    [SerializeField] private TextMeshProUGUI latencyText;
    [SerializeField] private Color latencyColor;
    [SerializeField] private Button latencyButton;
    [SerializeField] private TextMeshProUGUI reliabilityText;
    [SerializeField] private Color reliabilityColor;
    [SerializeField] private Button reliabilityButton;
    [SerializeField] private TextMeshProUGUI fitnessText;
    [SerializeField] private Color fitnessColor;
    [SerializeField] private Button fitnessButton;
    [SerializeField] private Color tempColor;


    public void UpdateStatText(Worker worker)
    {
        if(worker != null)
        {
            robustnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(robustnessColor) + ">Robustness: " 
            + new string ('|', worker.Stats.Robustness) 
            + "</color><color=#" + ColorUtility.ToHtmlStringRGB(tempColor) 
            + new string('|', worker.AllocStats.Robustness) + "</color>";
            latencyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(latencyColor) + ">Latency: "
            + new string ('|', worker.Stats.Latency)
            + "</color><color=#" + ColorUtility.ToHtmlStringRGB(tempColor) 
            + new string('|', worker.AllocStats.Latency) + "</color>";
            reliabilityText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(reliabilityColor) + ">Reliability: "
            + new string ('|', worker.Stats.Reliability)
            + "</color><color=#" + ColorUtility.ToHtmlStringRGB(tempColor) 
            + new string('|', worker.AllocStats.Reliability) + "</color>";
            fitnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(fitnessColor) + ">Fitness: "
            + new string ('|', worker.Stats.Fitness)
            + "</color><color=#" + ColorUtility.ToHtmlStringRGB(tempColor)
            + new string('|', worker.AllocStats.Fitness) + "</color>";
        }
    }

    public void ResetStatText()
    {
        robustnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(robustnessColor) + ">Robustness: " + "</color>";
        latencyText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(latencyColor) + ">Latency: " + "</color>";
        reliabilityText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(reliabilityColor) + ">Reliability: " + "</color>";
        fitnessText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(fitnessColor) + ">Fitness: " + "</color>";
    }

    public void ShowButtons()
    {
        robustnessButton.gameObject.SetActive(true);
        latencyButton.gameObject.SetActive(true);
        reliabilityButton.gameObject.SetActive(true);
        fitnessButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        robustnessButton.gameObject.SetActive(false);
        latencyButton.gameObject.SetActive(false);
        reliabilityButton.gameObject.SetActive(false);
        fitnessButton.gameObject.SetActive(false);
    }
    

}
