using UnityEngine;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;
    
    [SerializeField] public List<RepairWorker> RepairWorkers = new List<RepairWorker>();
    private RepairWorker selectedRepairWorker;

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
        foreach(RepairWorker repairWorker in RepairWorkers)
        {
            if(repairWorker != null)
            {
                repairWorker.OnSelected += SelectRepairWorker;
            }
        }
    }

    public void AddWorker(RepairWorker newWorkerPrefab)
    {
        RepairWorker newWorker = Instantiate(newWorkerPrefab, transform);
        if(newWorker == null)
        {
            Destroy(newWorker);
            return;
        }

        RepairWorkers.Add(newWorker);
        newWorker.OnSelected += SelectRepairWorker;
    }

    public void SelectRepairWorker(RepairWorker repairWorker)
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

    public void ReturnRepairWorker(RepairWorker repairWorker)
    {
        repairWorker.IsAvailable = true;
    }

    void OnDestroy()
    {
        foreach(RepairWorker repairWorker in RepairWorkers)
        {
            if(repairWorker == null) continue;
            repairWorker.OnSelected -= SelectRepairWorker;
        }
    }
}
