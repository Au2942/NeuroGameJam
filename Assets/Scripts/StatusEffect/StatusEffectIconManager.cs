using UnityEngine;
using System.Collections.Generic;
using System.Text;
public class StatusEffectIconManager : MonoBehaviour
{
    public static StatusEffectIconManager Instance;
    [SerializeField] private StatusEffectIcon statusEffectIconPrefab;
    //[SerializeField] public WorkerStatusEffectSO[] statusEffectsSO = new WorkerStatusEffectSO[0];
    public int poolSize = 30;
    public int expandStep = 10;
    //this dictionary should be in StatusEffectManager maybe?
    //private Dictionary<string, WorkerStatusEffect> statusEffectByID = new Dictionary<string, WorkerStatusEffect>();
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
        if(StatusEffectManager.Instance.TryGetStatusEffectDataByID(id, out StatusEffectData data))
        {
            icon.SetupStatusEffectIcon(data);
        }
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