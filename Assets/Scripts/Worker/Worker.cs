using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Worker : MonoBehaviour, ICombatant, IStatusEffectable
{
    [SerializeField] public string Name;
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
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();
    [SerializeField] public bool IsAvailable = true;
    [SerializeField] public bool CanRegenHealth = true;
    [SerializeField] public WorkHandler WorkHandler = new WorkHandler();

    public MemoryEntity assignedEntity => WorkHandler.entity;
    public List<ICombatant> CombatTargets = new List<ICombatant>();

    float ICombatant.Health { get => workerData.Health; set => workerData.Health = value; }
    float ICombatant.MaxHealth { get => workerData.TotalStats.MaxHealth; set => workerData.TotalStats.MaxHealth = value; }
    public float AttackDamage { get => workerData.TotalStats.WorkAmount; set => workerData.TotalStats.WorkAmount = value;}
    public float AttackRate { get => workerData.TotalStats.WorkTime; set => workerData.TotalStats.WorkTime = value;}

    List<ICombatant> ICombatant.CombatTargets { get => CombatTargets; set => CombatTargets = value; }
    List<StatusEffect> IStatusEffectable.StatusEffects { get => StatusEffects; set => StatusEffects = value; }

    public event System.Action<Worker> OnSelectEvent; 
    public event System.Action<Worker> OnDieEvent; 
    public event System.Action<StatusEffect> OnApplyStatusEffectEvent;
    public event System.Action<StatusEffect> OnRemoveStatusEffectEvent; 
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

    void Start()
    {
        workerData.Health = workerData.TotalStats.MaxHealth;
        StartCoroutine(RegenHealth());
    }

    void Update()
    {
        
    }

    IEnumerator RegenHealth()
    {
        while(true)
        {
            while(workerData.Health >= workerData.TotalStats.MaxHealth || !CanRegenHealth || WorkHandler.workState != WorkHandler.WorkState.None || GameManager.Instance.isPause)
            {
                yield return null;
            }
            yield return new WaitForSeconds(workerData.TotalStats.RegenTime);
            AddHealth(1);
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

    public void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source = null)
    {
        StatusEffects.Add(statusEffect);
        OnApplyStatusEffectEvent?.Invoke(statusEffect);
    }
    
    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        StatusEffects.Remove(statusEffect);
        OnRemoveStatusEffectEvent?.Invoke(statusEffect);
    }

    public void Select()
    {
        if(PlayerManager.Instance.State == PlayerManager.PlayerState.command) {return;}

        if(!GameManager.Instance.isPause)
        {
            if(IsAvailable)
            {
                PlayerManager.Instance.SetState(PlayerManager.PlayerState.command);
            }
            SetAvailability(false);
            OnSelectEvent?.Invoke(this);
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
    public void DealDamage(ICombatant target)
    {
        target.TakeDamage(AttackDamage, this);
    }

    public void TakeDamage(float value, ICombatant attacker)
    {
        Debug.Log(name + " health is " + workerData.Health);
        AddHealth(-value);
    }

    public void AddCombatTarget(ICombatant target)
    {
        CombatTargets.Add(target);
    }

    public void RemoveCombatTarget(ICombatant target)
    {
        CombatTargets.Remove(target);
    }
    public void Die()
    {
        OnDieEvent?.Invoke(this);
        WorkerManager.Instance.RemoveWorker(this);
        Destroy(gameObject,0);
    }

    void OnDisable()
    {
        iconClickDetector.OnLeftClickEvent -= OnClickEventHandler;
    }

}

