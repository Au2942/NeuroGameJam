using UnityEngine;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;
    
    [SerializeField] public List<Worker> RepairWorkers = new List<Worker>();
    private Worker selectedRepairWorker;
    [SerializeField] private Worker RepairWorkerPrefab; 


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

    public void AddWorker(Worker newWorkerPrefab)
    {
        Worker newWorker = Instantiate(newWorkerPrefab, transform);
        if(newWorker == null)
        {
            Destroy(newWorker);
            return;
        }

        RepairWorkers.Add(newWorker);
        newWorker.OnSelected += SelectRepairWorker;
    }

    public void SelectRepairWorker(Worker repairWorker)
    {
        selectedRepairWorker = repairWorker;
    }

    public void DeselectRepairWorker()
    {
        selectedRepairWorker.SetAvailability(true);
        selectedRepairWorker = null;
    }

    public bool TryUseRepairWorker(Entity entity)
    {
        if(selectedRepairWorker != null)
        {
            entity.StartRepairing(selectedRepairWorker);
            selectedRepairWorker = null;
            return true;
        }
        return false;
    }

    public void ReturnRepairWorker(Worker repairWorker)
    {
        repairWorker.IsAvailable = true;
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
