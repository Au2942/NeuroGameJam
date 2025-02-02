using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance;

    [SerializeField] public WorkerDetailsUI WorkerStatUI;
    [SerializeField] public WorkerScroller WorkerScroller;
    [SerializeField] public WorkerAppearanceGenerator WorkerAppearanceGenerator;
    [SerializeField] public RectTransform WorkerLayout;
    [SerializeField] public List<Worker> Workers = new List<Worker>();
    [SerializeField] public int MaxLevel = 5;
    [SerializeField] private Worker WorkerPrefab; 
    [SerializeField] public Worker SelectedWorker;

    public event System.Action<Worker> OnWorkerSelectedEvent;
    public event System.Action OnWorkerDeselectedEvent;


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
                worker.OnSelectedEvent += SelectWorker;
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
        newWorker.IconAppearance.SetApperance(newWorker.WorkerAppearance.WorkerAppearanceData);

        newWorker.OnSelectedEvent += SelectWorker;

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
        worker.OnSelectedEvent -= SelectWorker;
        Destroy(worker.gameObject);
        Worker nextWorker = Workers.Count > 0 ? Workers[Mathf.Clamp(index, 0, Workers.Count - 1)] : null;
        if(nextWorker != null)
        {
            nextWorker.Select();
        }

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
        WorkerStatUI.UpdateAttributesText(SelectedWorker);
        OnWorkerSelectedEvent?.Invoke(SelectedWorker);
    }

    public void DeselectWorker()
    {
        if(SelectedWorker == null) return;
        WorkerStatUI.ResetAttributesText();
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
            worker.OnSelectedEvent -= SelectWorker;
        }
    }
}
