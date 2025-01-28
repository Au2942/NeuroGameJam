using UnityEngine;
using System.Collections.Generic;


public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    [SerializeField] public WorkerDetailsUI WorkerStatUI;
    [SerializeField] private WorkerScroller WorkerScroller;
    [SerializeField] private RectTransform WorkerLayout;
    [SerializeField] public List<Worker> Workers = new List<Worker>();
    [SerializeField] public int MaxLevel = 5;
    public Worker selectedWorker {get; private set;}
    [SerializeField] private Worker WorkerPrefab; 

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
        foreach(Worker worker in Workers)
        {
            if(worker != null)
            {
                worker.OnSelected += SelectWorker;
            }
        }
    }

    public Worker AddWorker(Worker newWorkerPrefab)
    {
        if(newWorkerPrefab == null) return null;

        Worker newWorker = Instantiate(newWorkerPrefab, WorkerLayout);
        Workers.Add(newWorker);
        newWorker.OnSelected += SelectWorker;
        newWorker.Select();

        return newWorker;
    }
    public Worker AddWorker()
    {
        return AddWorker(WorkerPrefab);
    }

    public void RemoveWorker(Worker worker)
    {
        if(worker == null) return;
        int index = Workers.IndexOf(worker);
        Workers.Remove(worker);
        worker.OnSelected -= SelectWorker;
        Destroy(worker.gameObject);
        Worker nextWorker = Workers.Count > 0 ? Workers[Mathf.Clamp(index, 0, Workers.Count - 1)] : null;
        if(nextWorker != null)
        {
            nextWorker.Select();
        }

    }

    public void SelectWorker(Worker worker)
    {
        DeselectWorker();
        selectedWorker = worker;
        WorkerStatUI.UpdateAttributesText(selectedWorker);
        OnWorkerSelected?.Invoke(selectedWorker);
    }

    public void DeselectWorker()
    {
        if(selectedWorker == null) return;
        WorkerStatUI.ResetAttributesText();
        selectedWorker.Deselect();
        selectedWorker = null;
        OnWorkerDeselected?.Invoke();
    }

    public bool TryDoMaintainWork(MemoryEntity entity)
    {
        if(selectedWorker != null)
        {
            selectedWorker.DoMaintenance(entity);
            selectedWorker = null;
            return true;
        }
        return false;
    }
    public bool TryDoRepairWork(MemoryEntity entity)
    {
        if(selectedWorker != null)
        {
            selectedWorker.DoRepair(entity);
            selectedWorker = null;
            return true;
        }
        return false;
    }

    void OnDestroy()
    {
        foreach(Worker worker in Workers)
        {
            if(worker == null) continue;
            worker.OnSelected -= SelectWorker;
        }
    }
}
