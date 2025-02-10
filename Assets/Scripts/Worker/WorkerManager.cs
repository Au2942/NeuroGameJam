using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    [SerializeField] public WorkerDetails WorkerDetails;
    [SerializeField] public WorkerScroller WorkerScroller;
    [SerializeField] public WorkerAppearanceGenerator WorkerAppearanceGenerator;
    [SerializeField] public RectTransform WorkerLayout;
    [SerializeField] public List<Worker> Workers = new List<Worker>();
    [SerializeField] public int MaxLevel = 5;
    [SerializeField] private Worker WorkerPrefab; 
    [SerializeField] public Worker SelectedWorker;

    public event System.Action<Worker> OnWorkerSelectEvent;
    public event System.Action OnWorkerDeselectedEvent;
    public System.Action OnSelectedWorkerDetailsChangedEventHandler;

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
                worker.OnSelectEvent += SelectWorker;
            }
        }
        AddWorker();
    }

    public Worker AddWorker(Worker newWorkerPrefab)
    {
        if(newWorkerPrefab == null) return null;

        Worker newWorker = Instantiate(newWorkerPrefab, WorkerLayout);
        Workers.Add(newWorker);

        WorkerAppearanceGenerator.GenerateAppearance(newWorker.WorkerAppearance);
        newWorker.Icon.Appearance.CopyData(newWorker.WorkerAppearance.WorkerAppearanceData);

        newWorker.OnSelectEvent += SelectWorker;

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
        worker.OnSelectEvent -= SelectWorker;
        Destroy(worker.gameObject, 0);
        Worker nextWorker = Workers.Count > 0 ? Workers[Mathf.Clamp(index, 0, Workers.Count - 1)] : null;
        if(nextWorker != null)
        {
            nextWorker.Select();
        }
    }

    public void UpdateSelectedWorkerDetails()
    {
        WorkerDetails.UpdateDisplayDetails(SelectedWorker);
    }

    public void SelectWorker(Worker worker)
    {
        if(SelectedWorker == worker) 
        {
            DeselectWorker();
            return;
        }
        
        DeselectWorker();
        SelectedWorker = worker;
        WorkerDetails.UpdateDisplayDetails(SelectedWorker);

        worker.OnDetailsChanged += UpdateSelectedWorkerDetails;

        OnWorkerSelectEvent?.Invoke(SelectedWorker);
    }

    public void DeselectWorker()
    {
        if(SelectedWorker == null) return;

        SelectedWorker.OnDetailsChanged -= UpdateSelectedWorkerDetails;
        
        WorkerDetails.ClearDisplayDetails();
        SelectedWorker.Deselect();
        SelectedWorker = null;
        OnWorkerDeselectedEvent?.Invoke();
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
            worker.OnSelectEvent -= SelectWorker;
        }
    }
}
