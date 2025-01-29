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
    [SerializeField] private Worker WorkerPrefab; 
    [SerializeField] public Worker SelectedWorker;

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
        foreach(Worker worker in WorkerLayout.GetComponentsInChildren<Worker>())
        {
            Workers.Add(worker);
        }
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
        SelectedWorker = worker;
        WorkerStatUI.UpdateAttributesText(SelectedWorker);
        OnWorkerSelected?.Invoke(SelectedWorker);
    }

    public void DeselectWorker()
    {
        if(SelectedWorker == null) return;
        WorkerStatUI.ResetAttributesText();
        SelectedWorker.Deselect();
        SelectedWorker = null;
        OnWorkerDeselected?.Invoke();
    }

    public bool TryDoMaintainWork(MemoryEntity entity)
    {
        if(SelectedWorker != null)
        {
            SelectedWorker.DoMaintenance(entity);
            return true;
        }
        return false;
    }
    public bool TryDoRepairWork(MemoryEntity entity)
    {
        if(SelectedWorker != null)
        {
            SelectedWorker.DoRepair(entity);
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
