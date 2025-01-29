using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class SleepSettingsScreen : MonoBehaviour
{
    public static SleepSettingsScreen Instance;
    [SerializeField] private RectTransform sleepSettingsUI;
    [SerializeField] private RectTransform sleepSettingsLayout;
    [SerializeField] private RectTransform actionsPanel;
    [SerializeField] private RectTransform workerPanel;
    [SerializeField] private float healthPerChunk = 20f;
    [SerializeField] private float hourPerRestoreHealthChunk = 1f;
    [SerializeField] private float hourPerTrainedAttribute = 0.5f;
    [SerializeField] private Color addedWorkerColor;
    [SerializeField] private float hourPerAddedWorker = 1f;
    [SerializeField] private Slider restoreHealthSlider;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI sleepHoursText; 

    private Worker selectedWorker => WorkerManager.Instance.SelectedWorker;
    private List<Worker> addedWorkers = new List<Worker>();
    private HashSet<Worker> trainedWorkers = new HashSet<Worker>();
    private WorkerDetailsUI DetailsUI => WorkerManager.Instance.WorkerStatUI;
    private float DisplayTimeMultiplier => TimescaleManager.Instance.displayTimeMultiplier;
    private int allocedAttributes = 0;
    private int restoredHealthChunk = 0;
    private float sleepHours = 0;
    public bool IsOpen {get; private set;} = false;

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
        restoreHealthSlider.onValueChanged.AddListener(delegate {SetRestoreHealthChunk((int)restoreHealthSlider.value);});
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
        sleepSettingsUI.gameObject.SetActive(true);
    }

    public void Update()
    {
       UpdateSleepHoursText(); 
    }

    private void UpdateSleepHoursText()
    {
        CalculateSleepHours();
        string hours;
        string remainingHrs = " (remaining: " + (PlayerManager.Instance.RemainingStreamTime*DisplayTimeMultiplier/3600 - Mathf.Max(4,sleepHours)).ToString("0.0")  + " hrs)";
        if(sleepHours < 4)
        {
            hours = "4 hrs (" + sleepHours + " hrs)"; 
        }
        else
        {
            hours = sleepHours + " hrs";
        }
        if(!ValidSleepHours())
        {
            sleepHoursText.text = "<color=red> Sleep Hours: " + hours + remainingHrs + "</color>";
        }
        else 
        {
            sleepHoursText.text = "Sleep Hours: " + hours + remainingHrs;
        }
    }

    public void AddRobustness(int value)
    {
        AddSelectedAllocAttributes(new WorkerAttributes(value, 0, 0, 0));
    }

    public void AddLatency(int value)
    {
        AddSelectedAllocAttributes(new WorkerAttributes(0, value, 0, 0));
    }

    public void AddAccuracy(int value)
    {
        AddSelectedAllocAttributes(new WorkerAttributes(0, 0, value, 0));
    }

    public void AddFitness(int value)
    {
        AddSelectedAllocAttributes(new WorkerAttributes(0, 0, 0, value));
    }

    private void AddSelectedAllocAttributes(WorkerAttributes attributes)
    {
        if(selectedWorker == null) return;
        allocedAttributes += selectedWorker.AddAllocAttributes(attributes);
        trainedWorkers.Add(selectedWorker);
        DetailsUI.UpdateAttributesText(selectedWorker);
    }

    public void ResetSelectedAllocAttributes()
    {
        if(selectedWorker == null) return;
        if(!trainedWorkers.Contains(selectedWorker)) return;
        allocedAttributes -= selectedWorker.ResetAllocAttributes();
        trainedWorkers.Remove(selectedWorker);
        DetailsUI.UpdateAttributesText(selectedWorker);
    }

    public void AddWorker()
    {
        Worker worker = WorkerManager.Instance.AddWorker();
        worker.WorkerIcon.color = addedWorkerColor;
        addedWorkers.Add(worker);
        DetailsUI.NewButton.transform.SetAsLastSibling();
    }

    public void DeleteAddedWorker()
    {
        if(addedWorkers.Contains(selectedWorker))
        {
            addedWorkers.Remove(selectedWorker);
            trainedWorkers.Remove(selectedWorker);
            WorkerManager.Instance.RemoveWorker(selectedWorker);
        }
    }

    public void SetRestoreHealthChunk(int value)
    {
        restoredHealthChunk = value;
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
        PlayerManager.Instance.HealHealth(restoredHealthChunk*healthPerChunk);
        CloseUI();
        PlayerManager.Instance.Sleep(Mathf.Max(4,sleepHours) * (3600/DisplayTimeMultiplier)); //this also change timescale so must be last
    }

    private void CalculateSleepHours()
    {
        sleepHours = 0;
        sleepHours += restoredHealthChunk * hourPerRestoreHealthChunk;
        sleepHours += allocedAttributes * hourPerTrainedAttribute;
        sleepHours += addedWorkers.Count * hourPerAddedWorker;
    }

    private bool ValidSleepHours()
    {
        CalculateSleepHours();
        return PlayerManager.Instance.RemainingStreamTime*DisplayTimeMultiplier/3600 - Mathf.Max(4,sleepHours) >= 1;
    }

    public void CloseUI()
    {
        allocedAttributes = 0;
        restoredHealthChunk = 0;
        sleepHours = 0;
        addedWorkers.Clear();
        sleepSettingsUI.gameObject.SetActive(false);
        workerPanel.SetParent(actionsPanel);
        WorkerManager.Instance.DeselectWorker();
        WorkerManager.Instance.WorkerStatUI.HideButtons();
        TimescaleManager.Instance.SetTimescale(1);
        GameManager.Instance.ContinueStream();
        GameManager.Instance.isPause = false;
        IsOpen = false;
    }


}

