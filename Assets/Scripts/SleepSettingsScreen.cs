using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;


public class SleepSettingsScreen : MonoBehaviour
{
    public static SleepSettingsScreen Instance;
    [SerializeField] private RectTransform sleepSettingsUI;
    [SerializeField] private RectTransform sleepSettingsLayout;
    [SerializeField] private RectTransform actionsPanel;
    [SerializeField] private RectTransform workerPanel;
    [SerializeField] private float healthPerHours = 20f;
    [SerializeField] private float hoursPerRestoreHealthChunk = 1f;
    [SerializeField] private float hoursPerTrainedAttribute = 0.5f;
    [SerializeField] private float hoursPerAddedWorker = 1f;
    [SerializeField] private Slider sleepHourSlider;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI sleepHoursText; 
    [SerializeField] private TextMeshProUGUI sleepPointsText; 

    private Worker selectedWorker => WorkerManager.Instance.SelectedWorker;
    private List<Worker> addedWorkers = new List<Worker>();
    private HashSet<Worker> trainedWorkers = new HashSet<Worker>();
    private WorkerDetailsUI DetailsUI => WorkerManager.Instance.WorkerStatUI;
    private float DisplayTimeMultiplier => TimescaleManager.Instance.displayTimeMultiplier;
    private int allocedAttributes = 0;
    private float sleepHours = 0;
    public bool IsOpen {get; private set;} = false;
    private StringBuilder sb1 = new StringBuilder();
    private StringBuilder sb3 = new StringBuilder();
    private StringBuilder sb2 = new StringBuilder();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        sleepHourSlider.onValueChanged.AddListener(delegate {SetSleepHours((int)sleepHourSlider.value);});
        confirmButton.onClick.AddListener(ConfirmSleep);
        DetailsUI.AddRobustnessButton.onClick.AddListener(delegate {AddRobustness(1);});
        DetailsUI.SubtractRobustnessButton.onClick.AddListener(delegate {AddRobustness(-1);});
        DetailsUI.AddLatencyButton.onClick.AddListener(delegate {AddLatency(1);});
        DetailsUI.SubtractLatencyButton.onClick.AddListener(delegate {AddLatency(-1);});
        DetailsUI.AddAccuracyButton.onClick.AddListener(delegate {AddAccuracy(1);});
        DetailsUI.SubtractAccuracyButton.onClick.AddListener(delegate {AddAccuracy(-1);});
        DetailsUI.AddFitnessButton.onClick.AddListener(delegate {AddFitness(1);});
        DetailsUI.SubtractFitnessButton.onClick.AddListener(delegate {AddFitness(-1);});
        DetailsUI.NewButton.onClick.AddListener(AddWorker);
        DetailsUI.ResetButton.onClick.AddListener(ResetSelectedAllocAttributes);
        DetailsUI.DeleteButton.onClick.AddListener(DeleteAddedWorker);
    }
    public void OpenUI()
    {
        IsOpen = true;
        GameManager.Instance.isPause = true;
        GameManager.Instance.StopStream();
        TimescaleManager.Instance.SetTimescale(0);
        workerPanel.SetParent(sleepSettingsLayout);
        WorkerManager.Instance.DeselectWorker();
        WorkerManager.Instance.WorkerStatUI.ShowButtons();
        DetailsUI.NewButton.transform.SetAsLastSibling();
        sleepSettingsUI.gameObject.SetActive(true);
        sleepHourSlider.maxValue = Mathf.FloorToInt(PlayerManager.Instance.RemainingStreamTime*DisplayTimeMultiplier/3600);
    }

    private void UpdateSleepHoursText()
    {
        sb1.Clear();
        sb2.Clear();
        if (sleepHours < 4)
        {
            sb2.Append("4 hrs (").Append(sleepHours).Append(" hrs)");
        }
        else
        {
            sb2.Append(sleepHours).Append(" hrs");
        }

        sb3.Clear();
        sb3.Append(" (remaining: ").Append((PlayerManager.Instance.RemainingStreamTime*DisplayTimeMultiplier/3600 - Mathf.Max(4,sleepHours)).ToString("0.0")).Append(" hrs)");

        if (!ValidSleepHours())
        {
            sb1.Append("<color=red> Sleep Hours: ").Append(sb2).Append(sb3).Append("</color>");
        }
        else
        {
            sb1.Append("Sleep Hours: ").Append(sb2).Append(sb3);
        }
        sleepHoursText.text = sb1.ToString();
        sb1.Clear();
        sb1.Append("\nSleep Points: ").Append(GetRemainingSleepPoints()).Append(" (unspend points will restore Neuro's integrity)");
        sleepPointsText.text = sb1.ToString();
    }

    public void AddRobustness(int value)
    {
        AllocateAttributes(new WorkerAttributes(value, 0, 0, 0));
    }

    public void AddLatency(int value)
    {
        AllocateAttributes(new WorkerAttributes(0, value, 0, 0));
    }

    public void AddAccuracy(int value)
    {
        AllocateAttributes(new WorkerAttributes(0, 0, value, 0));
    }

    public void AddFitness(int value)
    {
        AllocateAttributes(new WorkerAttributes(0, 0, 0, value));
    }

    private void AllocateAttributes(WorkerAttributes attributes)
    {
        if(selectedWorker == null) return;
        if(GetRemainingSleepPoints() < attributes.Sum()*hoursPerTrainedAttribute) return;
        allocedAttributes += selectedWorker.AllocateAttributes(attributes);
        trainedWorkers.Add(selectedWorker);
        DetailsUI.UpdateAttributesText(selectedWorker);
        UpdateSleepHoursText();
    }

    public void ResetSelectedAllocAttributes()
    {
        if(selectedWorker == null) return;
        if(!trainedWorkers.Contains(selectedWorker)) return;
        allocedAttributes -= selectedWorker.ResetAllocAttributes();
        trainedWorkers.Remove(selectedWorker);
        DetailsUI.UpdateAttributesText(selectedWorker);
        UpdateSleepHoursText();
    }

    public void AddWorker()
    {
        if(GetRemainingSleepPoints() < hoursPerAddedWorker) return;
        Worker worker = WorkerManager.Instance.AddWorker();
        worker.AddedOverlay.gameObject.SetActive(true);
        worker.Select();
        addedWorkers.Add(worker);
        DetailsUI.NewButton.transform.SetAsLastSibling();
        UpdateSleepHoursText();
    }

    public void DeleteAddedWorker()
    {
        if(addedWorkers.Contains(selectedWorker))
        {
            addedWorkers.Remove(selectedWorker);
            trainedWorkers.Remove(selectedWorker);
            WorkerManager.Instance.RemoveWorker(selectedWorker);
        }
        UpdateSleepHoursText();
    }

    public void SetSleepHours(int value)
    {
        if(GetRemainingSleepPoints() < sleepHours - value)
        {
            sleepHourSlider.value = sleepHours;
            return;
        }
        sleepHours = value;
        UpdateSleepHoursText();
    }

    public void ConfirmSleep()
    {   
        if(!ValidSleepHours())
        {
            SFXSoundBank.Instance.PlayInvalidActionSFX();
            return;
        }

        foreach(Worker worker in trainedWorkers)
        {
            worker.ApplyAllocAttributes();
        }

        foreach(Worker worker in addedWorkers)
        {
            worker.AddedOverlay.gameObject.SetActive(false);
        }

        PlayerManager.Instance.HealHealth(GetRemainingSleepPoints()*healthPerHours/hoursPerRestoreHealthChunk);
        CloseUI();
        PlayerManager.Instance.Sleep(Mathf.Max(4,sleepHours) * (3600/DisplayTimeMultiplier)); //this also change timescale so must be last
    }

    private float GetRemainingSleepPoints()
    {
        float sleepPoints = sleepHours;
        sleepPoints -= allocedAttributes * hoursPerTrainedAttribute;
        sleepPoints -= addedWorkers.Count * hoursPerAddedWorker;
        return sleepPoints;
    }

    private bool ValidSleepHours()
    {
        return PlayerManager.Instance.RemainingStreamTime*DisplayTimeMultiplier/3600 - Mathf.Max(4,sleepHours) >= 0;
    }

    public void CloseUI()
    {
        allocedAttributes = 0;
        sleepHours = 0;
        addedWorkers.Clear();
        trainedWorkers.Clear();
        workerPanel.SetParent(actionsPanel);
        WorkerManager.Instance.DeselectWorker();
        WorkerManager.Instance.WorkerStatUI.HideButtons();
        TimescaleManager.Instance.SetTimescale(1);
        GameManager.Instance.ContinueStream();
        GameManager.Instance.isPause = false;
        IsOpen = false;
        sleepSettingsUI.gameObject.SetActive(false);
    }


}

