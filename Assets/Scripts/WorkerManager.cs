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

    public void AddRepairWorker(RepairWorker newRepairWorker)
    {
        RepairWorkers.Add(newRepairWorker);
        newRepairWorker.OnSelected += SelectRepairWorker;
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

    public RepairWorker UseRepairWorker(Entity entity)
    {
        if(selectedRepairWorker != null)
        {
            entity.StartRepairing(selectedRepairWorker);
            selectedRepairWorker = null;
            return selectedRepairWorker;
        }
        return null;
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
