using UnityEngine;
using System.Collections.Generic;

public class StatusEffectIconManager : MonoBehaviour
{
    public static StatusEffectIconManager Instance;
    [SerializeField] private StatusEffectIcon statusEffectIconPrefab;
    [SerializeField] public List<WorkerStatusEffectSO> statusEffectsSO;
    public int poolSize = 30;
    public int expandStep = 10;
    //this dictionary should be in StatusEffectManager maybe?
    private Dictionary<string, WorkerStatusEffect> statusEffectByID = new Dictionary<string, WorkerStatusEffect>();
    private Queue<StatusEffectIcon> statusEffectIconPool = new Queue<StatusEffectIcon>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        foreach(WorkerStatusEffectSO statusEffectSO in statusEffectsSO)
        {
            WorkerStatusEffect statusEffect = statusEffectSO.CreateWorkerStatusEffect();
            WorkerStatusEffectData data = statusEffect.GetData();
            statusEffectByID.TryAdd(data.ID, statusEffect);
        }
        ExpandPool(poolSize);
    }

    private void ExpandPool(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            StatusEffectIcon statusEffectIcon = Instantiate(statusEffectIconPrefab, transform);
            statusEffectIcon.gameObject.SetActive(false);
            statusEffectIconPool.Enqueue(statusEffectIcon);
        }
    }

    private void SetStatusEffectIcon(string id, StatusEffectIcon icon)
    {
        WorkerStatusEffect statusEffect = statusEffectByID[id];
        WorkerStatusEffectData data = statusEffect.GetData();
        icon.SetIcon(data.Icon);
        icon.SetTooltip(data.Name, data.Description);
    }
    public StatusEffectIcon GetStatusEffectIcon(string id)
    {
        if(statusEffectIconPool.Count == 0)
        {
            ExpandPool(10);
        }
        StatusEffectIcon statusEffectIcon = statusEffectIconPool.Dequeue();
        SetStatusEffectIcon(id, statusEffectIcon);
        statusEffectIcon.gameObject.SetActive(true);
        return statusEffectIcon;
    }

    public void ReturnStatusEffectIcon(StatusEffectIcon statusEffectIcon)
    {
        statusEffectIconPool.Enqueue(statusEffectIcon);
        statusEffectIcon.transform.SetParent(transform); //ensure wont be destroyed
        statusEffectIcon.gameObject.SetActive(false);
    }

}