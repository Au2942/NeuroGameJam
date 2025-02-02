using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class Worker : MonoBehaviour, ICombatUnit
{
    //seperate into "actual worker" and "icon worker" later
    [SerializeField] public string Identifier;
    [SerializeField] public WorkerAppearance WorkerAppearance;
    [SerializeField] public WorkerAppearance IconAppearance;
    [SerializeField] public UIEventHandler iconClickDetector; 
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public Image DamageBar;
    [SerializeField] public Image CooldownOverlay;
    [SerializeField] public Image AddedOverlay;
    [SerializeField] public WorkerData workerData = new WorkerData();
    public WorkerAttributes BaseAttributes => workerData.BaseAttributes;
    public WorkerAttributes TempAttributes => workerData.TempAttributes;
    public WorkerAttributes AllocAttributes => workerData.AllocAttributes;
    public WorkerAttributes TotalAttributes => workerData.TotalAttributes;  
    public WorkerStats BaseStats => workerData.BaseStats;
    public WorkerStats TempStats => workerData.TempStats;
    public WorkerStats TotalStats => workerData.TotalStats;
    [SerializeField] public List<WorkerStatusEffect> WorkerStatusEffects = new List<WorkerStatusEffect>();
    [SerializeField] public bool IsAvailable = true;
    [SerializeField] public WorkHandler WorkHandler = new WorkHandler();

    public MemoryEntity assignedEntity => WorkHandler.entity;
    public List<ICombatUnit> CombatTargets = new List<ICombatUnit>();

    float ICombatUnit.Health { get => workerData.Health; set => workerData.Health = value; }
    float ICombatUnit.MaxHealth { get => workerData.TotalStats.MaxHealth; set => workerData.TotalStats.MaxHealth = value; }
    public float AttackDamage { get => workerData.TotalStats.WorkAmount; set => workerData.TotalStats.WorkAmount = value;}
    public float AttackRate { get => workerData.TotalStats.WorkTime; set => workerData.TotalStats.WorkTime = value;}

    List<ICombatUnit> ICombatUnit.CombatTargets { get => CombatTargets; set => CombatTargets = value; }
    public event System.Action<Worker> OnSelectedEvent; 
    private System.Action<PointerEventData> OnClickEventHandler;

    void Awake()
    {
        OnClickEventHandler = (eventData) => Select();
    }
    
    void OnEnable()
    {
        workerData.UpdateTotalAttribute();
        iconClickDetector.OnLeftClickEvent += OnClickEventHandler;
    }

    void Update()
    {
        foreach(WorkerStatusEffect statusEffect in WorkerStatusEffects)
        {
            statusEffect.OnUpdate(Time.deltaTime);
            if(statusEffect.ShouldExpire())
            {
                statusEffect.Remove();
                WorkerStatusEffects.Remove(statusEffect);
            }
        }
    }

    public void AddHealth(float value)
    {
        workerData.Health += value;
        if(workerData.Health > workerData.TotalStats.MaxHealth)
        {
            workerData.Health = workerData.TotalStats.MaxHealth;
        }
        DamageBar.fillAmount = 1 - workerData.Health / workerData.TotalStats.MaxHealth;
        if(workerData.Health <= 0)
        {
            Die();
        }
    }

    public int AllocateAttributes(WorkerAttributes attributes)
    {
        return workerData.AllocateAttributes(attributes);
    }

    public int ResetAllocAttributes()
    {
        return workerData.ResetAllocAttributes();
    }

    public void AddTempAttributes(WorkerAttributes attributes)
    {
        workerData.AddTempAttributes(attributes);
    }

    public void AddTempStats(WorkerStats stats)
    {
        workerData.AddTempStats(stats);
    }

    public void ApplyAllocAttributes()
    {
        workerData.ApplyAllocAttributes();
    }

    public void ApplyStatusEffect(WorkerStatusEffect statusEffect, Entity source = null)
    {
        WorkerStatusEffects.Add(statusEffect);
        statusEffect.Apply(source, this);
    }
    
    public void RemoveStatusEffect(WorkerStatusEffect statusEffect)
    {
        WorkerStatusEffects.Remove(statusEffect);
    }

    public void Select()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.command) {return;}

        if(!GameManager.Instance.isPause)
        {
            if(IsAvailable)
            {
                PlayerManager.Instance.SetState(PlayerManager.PlayerState.command);
            }
            SetAvailability(false);
            OnSelectedEvent?.Invoke(this);
        }
    }


    public void Deselect()
    {
        if(WorkHandler.workState == WorkHandler.WorkState.None)
        {
            SetAvailability(true);
        }
    }

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
        if(IsAvailable)
        {
            CooldownOverlay.gameObject.SetActive(false);
        }
        else
        {
            CooldownOverlay.gameObject.SetActive(true);
        }
    }
    public void DoMaintenance(MemoryEntity entity)
    {
        WorkHandler.StartMaintaining(entity, this);
    }

    public void DoRepair(MemoryEntity entity)
    {
        WorkHandler.StartRepairing(entity, this);
    }

    void OnDestroy()
    {
        iconClickDetector.OnLeftClickEvent -= (eventData) => Select();
    }

    public void Attack()
    {
        if(assignedEntity == null) return; 
        WorkHandler.RollRepairSuccessChance();
    }

    public bool IsCombatReady()
    {
        if(WorkHandler.workState == WorkHandler.WorkState.Repairing && CombatTargets.Count > 0) 
        {
            return true;
        }
        return false;
    }
    public void DealDamage(ICombatUnit target)
    {
        target.TakeDamage(AttackDamage, this);
    }

    public void TakeDamage(float value, ICombatUnit attacker)
    {
        Debug.Log(name + " health is " + workerData.Health);
        AddHealth(-value);
    }

    public void AddCombatTarget(ICombatUnit target)
    {
        CombatTargets.Add(target);
    }

    public void RemoveCombatTarget(ICombatUnit target)
    {
        CombatTargets.Remove(target);
    }
    public void Die()
    {
        WorkerManager.Instance.RemoveWorker(this);
        Destroy(gameObject);
    }

    void OnDisable()
    {
        iconClickDetector.OnLeftClickEvent -= OnClickEventHandler;
    }

}

