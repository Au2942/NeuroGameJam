using UnityEngine;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    [SerializeField] public WorkerStatsUI WorkerStatUI;
    [SerializeField] public List<Worker> RepairWorkers = new List<Worker>();
    [SerializeField] public int MaxLevel = 5;
    public Worker selectedRepairWorker {get; private set;}
    [SerializeField] private Worker RepairWorkerPrefab; 

    public event System.Action<Worker> OnWorkerSelected;
    public event System.Action OnWorkerDeselected;


    void Awake()
    {
        if (Instance == null)
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
        foreach(Worker repairWorker in RepairWorkers)
        {
            if(repairWorker != null)
            {
                repairWorker.OnSelected += SelectRepairWorker;
            }
        }
    }

    public Worker AddWorker(Worker newWorkerPrefab)
    {
        Worker newWorker = Instantiate(newWorkerPrefab, transform);
        if(newWorker == null)
        {
            Destroy(newWorker);
            return null;
        }

        RepairWorkers.Add(newWorker);
        newWorker.OnSelected += SelectRepairWorker;
        return newWorker;
    }
    public Worker AddWorker()
    {
        return AddWorker(RepairWorkerPrefab);
    }

    public void RemoveWorker(Worker worker)
    {
        RepairWorkers.Remove(worker);
        worker.OnSelected -= SelectRepairWorker;
        Destroy(worker.gameObject);
    }

    public void SelectRepairWorker(Worker repairWorker)
    {
        selectedRepairWorker = repairWorker;
        WorkerStatUI.UpdateStatText(selectedRepairWorker);
        OnWorkerSelected?.Invoke(selectedRepairWorker);
    }

    public void DeselectRepairWorker()
    {
        WorkerStatUI.ResetStatText();
        selectedRepairWorker.SetAvailability(true);
        selectedRepairWorker = null;
        OnWorkerDeselected?.Invoke();
    }

    public bool TryUseRepairWorker(MemoryEntity entity)
    {
        if(selectedRepairWorker != null)
        {
            selectedRepairWorker.StartRepairing(entity);
            selectedRepairWorker = null;
            return true;
        }
        return false;
    }

    void OnDestroy()
    {
        foreach(Worker repairWorker in RepairWorkers)
        {
            if(repairWorker == null) continue;
            repairWorker.OnSelected -= SelectRepairWorker;
        }
    }
}
