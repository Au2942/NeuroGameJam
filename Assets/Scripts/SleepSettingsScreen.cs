using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class SleepSettingsScreen : MonoBehaviour
{
    [SerializeField] private RectTransform SleepSettingsUI;
    [SerializeField] private RectTransform ActionsPanel;
    [SerializeField] private RectTransform WorkerUI;
    [SerializeField] private float restoreHealthChunk = 20f;
    [SerializeField] private float hourPerRestoreHealthChunk = 1f;
    [SerializeField] private float hourPerTrainedStat = 0.5f;
    [SerializeField] private float hourPerAddedWorker = 1f;

    [SerializeField] private TextMeshProUGUI sleepHoursText; 

    private Worker selectedWorker => WorkerManager.Instance.selectedRepairWorker;
    private List<Worker> addedWorkers = new List<Worker>();
    private List<Worker> trainedWorkers = new List<Worker>();
    private WorkerStatsUI statUI => WorkerManager.Instance.WorkerStatUI;
    private float displayTimeMultiplier => TimescaleManager.Instance.displayTimeMultiplier;
    private int allocedStats = 0;
    private int restoredHealthChunk = 0;
    private float sleepHours = 0;

    public void OpenUI()
    {
        TimescaleManager.Instance.SetTimescale(0);
        SleepSettingsUI.gameObject.SetActive(true);
    }

    public void Update()
    {
       UpdateSleepHoursText(); 
    }

    private void UpdateSleepHoursText()
    {
        CalculateSleepHours();
        sleepHoursText.text = "";
        if(!ValidSleepHours())
        {
            sleepHoursText.text = "<color=red> Sleep Hours: " + sleepHours * 60 * displayTimeMultiplier;
            if(sleepHours == 1)
            {
                sleepHoursText.text += "hr";
            }
            else sleepHoursText.text += "hrs";
            sleepHoursText.text += " (minimum 4 hours) </color>";
        }
        else 
        {
            sleepHoursText.text = "Sleep Hours: " + sleepHours * 60 * displayTimeMultiplier + "hrs";
        }
    }

    public void AddSelectedAllocStats(WorkerStats stats)
    {
        if(selectedWorker == null) return;
        allocedStats += selectedWorker.IncreaseAllocStats(stats);
        trainedWorkers.Add(selectedWorker);
        statUI.UpdateStatText(selectedWorker);
    }

    public void ResetSelectedAllocStats()
    {
        if(selectedWorker == null) return;
        allocedStats -= selectedWorker.ResetAllocStats();
        trainedWorkers.Remove(selectedWorker);
        statUI.UpdateStatText(selectedWorker);
    }

    public void AddWorker()
    {
        addedWorkers.Add(WorkerManager.Instance.AddWorker());
    }

    public void RemoveAddedWorker()
    {
        if(addedWorkers.Contains(selectedWorker))
        {
            WorkerManager.Instance.RemoveWorker(selectedWorker);
            addedWorkers.Remove(selectedWorker);
            trainedWorkers.Remove(selectedWorker);
        }
    }

    public void ConfirmSleep()
    {   
        if(!ValidSleepHours()) return;
        foreach(Worker worker in trainedWorkers)
        {
            worker.ApplyAllocStats();
        }
        PlayerManager.Instance.Heal(restoredHealthChunk*restoreHealthChunk);
    }

    private void CalculateSleepHours()
    {
        sleepHours = 0;
        sleepHours += restoreHealthChunk * hourPerRestoreHealthChunk;
        sleepHours += allocedStats * hourPerTrainedStat;
        sleepHours += addedWorkers.Count * hourPerAddedWorker;
    }

    private bool ValidSleepHours()
    {
        return sleepHours >= 4 && PlayerManager.Instance.RemainingStreamTime - sleepHours*60*displayTimeMultiplier >= 60*displayTimeMultiplier;
    }

    public void CloseUI()
    {
        allocedStats = 0;
        restoreHealthChunk = 0;
        sleepHours = 0;
        addedWorkers.Clear();
        SleepSettingsUI.gameObject.SetActive(false);
        TimescaleManager.Instance.SetTimescale(1);
    }


}

