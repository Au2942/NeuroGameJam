using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public struct WorkerAttributes
{
    public int Robustness; //health, health regen
    public float RobustnessScore;
    public int Latency; //repair speed, repair cooldown
    public float LatencyScore;
    public int Accuracy; //repair success chance
    public float AccuracyScore;
    public int Fitness; //repair amount
    public float FitnessScore;

    public WorkerAttributes(int robustness, int latency, int accuracy, int fitness)
    {
        Robustness = robustness;
        Latency = latency;
        Accuracy = accuracy;
        Fitness = fitness;
        RobustnessScore = 0;
        LatencyScore = 0;
        AccuracyScore = 0;
        FitnessScore = 0;
    }
    public float AverageAttributes()
    {
        return (Robustness + Latency + Accuracy + Fitness) / 4;
    }

}

public class Worker : MonoBehaviour
{

    [SerializeField] public string Identifier;
    [SerializeField] public Image WorkerIcon;
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public UIEventHandler ClickHandler; //can be put into its own class later maybe?
    [SerializeField] public Image CooldownIcon;
    [SerializeField] public WorkerAttributes Attributes = new WorkerAttributes(1,1,1,1);
    [SerializeField] public WorkerAttributes AllocAttributes = new WorkerAttributes(0,0,0,0);
    [SerializeField] public int MaxAttribute = 5;
    [SerializeField] public int Level = 1;
    [SerializeField] public int health = 5;
    [SerializeField] public float RepairSpeed = 5f;
    [SerializeField] public float RepairCooldown = 5f;
    [SerializeField] public float RepairSuccessChance = 60f;
    [SerializeField] public float RepairAmount = 30f;
    [SerializeField] public bool IsAvailable = true;

    public event System.Action<Worker> OnSelected; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Level = Mathf.RoundToInt(Attributes.AverageAttributes());
        UpdateStats();
        ClickHandler.OnLeftClickEvent += (eventData) => Select();
    }

    public int AddAllocAttributes(WorkerAttributes addedAttributes)
    {
        int allocedAttributes = 0;
        if(addedAttributes.Robustness > 0)
        {
            int allocedRobustness = Mathf.Min(addedAttributes.Robustness, MaxAttribute - Attributes.Robustness - AllocAttributes.Robustness);
            AllocAttributes.Robustness += allocedRobustness;
            allocedAttributes += allocedRobustness;
        }
        else
        {
            int unallocedRobustness = Mathf.Min(-addedAttributes.Robustness, AllocAttributes.Robustness);
            AllocAttributes.Robustness -= unallocedRobustness;
            allocedAttributes -= unallocedRobustness;
        }
        if(addedAttributes.Latency > 0)
        {
            int allocedLatency = Mathf.Min(addedAttributes.Latency, MaxAttribute - Attributes.Latency - AllocAttributes.Latency);
            AllocAttributes.Latency += allocedLatency;
            allocedAttributes += allocedLatency;
        }
        else
        {
            int unallocedLatency = Mathf.Min(-addedAttributes.Latency, AllocAttributes.Latency);
            AllocAttributes.Latency -= unallocedLatency;
            allocedAttributes -= unallocedLatency;
        }
        if(addedAttributes.Accuracy > 0)
        {
            int allocedAccuracy = Mathf.Min(addedAttributes.Accuracy, MaxAttribute - Attributes.Accuracy - AllocAttributes.Accuracy);
            AllocAttributes.Accuracy += allocedAccuracy;
            allocedAttributes += allocedAccuracy;
        }
        else
        {
            int unallocedAccuracy = Mathf.Min(-addedAttributes.Accuracy, AllocAttributes.Accuracy);
            AllocAttributes.Accuracy -= unallocedAccuracy;
            allocedAttributes -= unallocedAccuracy;
        }
        if(addedAttributes.Fitness > 0)
        {
            int allocedFitness = Mathf.Min(addedAttributes.Fitness, MaxAttribute - Attributes.Fitness - AllocAttributes.Fitness);
            AllocAttributes.Fitness += allocedFitness;
            allocedAttributes += allocedFitness;
        }
        else
        {
            int unallocedFitness = Mathf.Min(-addedAttributes.Fitness, AllocAttributes.Fitness);
            AllocAttributes.Fitness -= unallocedFitness;
            allocedAttributes -= unallocedFitness;
        }
        return allocedAttributes;
    }
    public int ResetAllocAttributes()
    {
        int resetAttributes = AllocAttributes.Robustness + AllocAttributes.Latency + AllocAttributes.Accuracy + AllocAttributes.Fitness;
        AllocAttributes.Robustness = 0;
        AllocAttributes.Latency = 0;
        AllocAttributes.Accuracy = 0;
        AllocAttributes.Fitness = 0;
        return resetAttributes;
    }

    public void ApplyAllocAttributes()
    {
        Attributes.Robustness += AllocAttributes.Robustness;
        Attributes.Latency += AllocAttributes.Latency;
        Attributes.Accuracy += AllocAttributes.Accuracy;
        Attributes.Fitness += AllocAttributes.Fitness;
        AllocAttributes = new WorkerAttributes(0,0,0,0);
        Level = Mathf.RoundToInt(Attributes.AverageAttributes());
        UpdateStats();
    }

    public void Select()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair || !IsAvailable) return;
        if(!GameManager.Instance.isPause)
        {
            PlayerManager.Instance.SetState(PlayerManager.PlayerState.repair);
        }
        SetAvailability(false);
        OnSelected?.Invoke(this);
    }

    public void Deselect()
    {
        SetAvailability(true);
    }

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
        if(IsAvailable)
        {
            CooldownIcon.gameObject.SetActive(false);
        }
        else
        {
            CooldownIcon.gameObject.SetActive(true);
        }
    }

    public virtual void StartRepairing(MemoryEntity entity)
    {
        entity.IsBeingRepaired = true;
        entity.Interactable = false;
        StartCoroutine(Repairing(entity));
    }

    private IEnumerator Repairing(MemoryEntity entity)
    {
        yield return new WaitForSeconds(RepairSpeed);
        RollRepairSuccessChance(entity);
        FinishRepairing(entity);
    }

    private void RollRepairSuccessChance(MemoryEntity entity)
    {
        float roll = Random.Range(0, 100);
        if(roll < RepairSuccessChance)
        {
            entity.OnRepairSuccess(this);
        }
        else
        {
            entity.OnRepairFail(this);
        }
    } 

    public virtual void FinishRepairing(MemoryEntity entity)
    {
        entity.IsBeingRepaired = false;
        entity.Interactable = true;
        IsAvailable = true;
    }


    public void FinishWork()
    {
        StartCoroutine(StartCooldown());
    }

    public IEnumerator StartCooldown()
    {
        float elapsedTime = 0f;;
        while(elapsedTime < RepairCooldown)
        {
            CooldownIcon.fillAmount = 1-(elapsedTime / RepairCooldown);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetAvailability(true);
    }
    
    private void UpdateStats()
    {
        RepairSpeed = 5f - (Attributes.Latency * 0.5f);
        RepairCooldown = 5f - (Attributes.Latency * 0.5f);
        RepairSuccessChance = 60f + (Attributes.Accuracy * 5f);
        RepairAmount = 30f + (Attributes.Fitness * 5f);
    }
    
    void OnDestroy()
    {
        ClickHandler.OnLeftClickEvent -= (eventData) => Select();
    }
}
