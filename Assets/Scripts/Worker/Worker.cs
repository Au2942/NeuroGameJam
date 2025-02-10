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
    [SerializeField] public UIEventHandler WorkerClickDetector;
    [SerializeField] public Rigidbody2D Rigidbody;
    [SerializeField] public Collider2D Collider;
    [SerializeField] public WorkerIcon Icon;
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
    public float AttackDamage { get => workerData.TotalStats.RestoreAmount; set => workerData.TotalStats.RestoreAmount = value;}
    public float AttackRate { get => workerData.TotalStats.TaskTime; set => workerData.TotalStats.TaskTime = value;}
    List<ICombatant> ICombatant.CombatTargets { get => CombatTargets; set => CombatTargets = value; }
    List<StatusEffect> IStatusEffectable.StatusEffects { get => StatusEffects; set => StatusEffects = value; }
    public event System.Action<Worker> OnSelectEvent; 
    public event System.Action<Worker> OnDieEvent; 
    public event System.Action OnDetailsChanged;
    private System.Action<PointerEventData> OnWorkerClickEventHandler;
    private System.Action<PointerEventData> OnWorkerMouseEnterEventHandler;
    private System.Action<PointerEventData> OnWorkerMouseExitEventHandler;
    private System.Action<PointerEventData> OnIconClickEventHandler;



    void Awake()
    {
        OnWorkerMouseEnterEventHandler = (eventData) => WorkerAppearance.ShowOutline(true);
        OnWorkerMouseExitEventHandler = (eventData) => {if(WorkerManager.Instance.SelectedWorker != this) WorkerAppearance.ShowOutline(false); };
        OnIconClickEventHandler = (eventData) => Select();
        Icon.ClickDetector.OnLeftClickEvent += OnIconClickEventHandler;

    }
    
    void OnEnable()
    {
        workerData.UpdateTotalAttribute();
        WorkerClickDetector.OnLeftClickEvent += OnWorkerClickEventHandler;
        WorkerClickDetector.OnPointerEnterEvent += OnWorkerMouseEnterEventHandler;
        WorkerClickDetector.OnPointerExitEvent += OnWorkerMouseExitEventHandler;

    }
    void OnDisable()
    {
        WorkerClickDetector.OnLeftClickEvent -= OnWorkerClickEventHandler;
        WorkerClickDetector.OnPointerEnterEvent -= OnWorkerMouseEnterEventHandler;
        WorkerClickDetector.OnPointerExitEvent -= OnWorkerMouseExitEventHandler;
        Icon.ClickDetector.OnLeftClickEvent -= OnIconClickEventHandler;
    }

    void Start()
    {
        workerData.Health = workerData.TotalStats.MaxHealth;
        StartCoroutine(RegenHealth());
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
            value = workerData.TotalStats.MaxHealth - workerData.Health;
        }

        if(WorkerAppearance.gameObject.activeSelf)
        {
            PopupTextSpawner.Instance.SpawnPopupText(WorkerAppearance.transform, WorkerAppearance.transform.position, value.ToString(), 0.5f, value > 0 ? Color.green : Color.red);
        }

        Icon.SetDamageBar(1 - workerData.Health / workerData.TotalStats.MaxHealth);

        if(workerData.Health <= 0)
        {
            Die();
        }
        OnDetailsChanged?.Invoke();
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
        OnDetailsChanged?.Invoke();
    }

    public void AddTempStats(WorkerStats stats)
    {
        workerData.AddTempStats(stats);
        OnDetailsChanged?.Invoke();
    }

    public void ApplyAllocAttributes()
    {
        workerData.ApplyAllocAttributes();
        OnDetailsChanged?.Invoke();
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, IStatusEffectSource source = null)
    {
        StatusEffects.Add(statusEffect);
        OnDetailsChanged?.Invoke();
    }

    public void ChangeStatusEffectStack(StatusEffect statusEffect, int stack)
    {
        OnDetailsChanged?.Invoke();
    }
    
    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        StatusEffects.Remove(statusEffect);
        OnDetailsChanged?.Invoke();
    }

    public void Select()
    {
        if(PlayerManager.Instance.State == PlayerManager.PlayerState.command) {return;}

        if(IsAvailable && !GameManager.Instance.isPause)
        {
            PlayerManager.Instance.SetState(PlayerManager.PlayerState.command);
        }
        WorkerAppearance.ShowOutline(true);
        Icon.ShowSelectBorder(true);
        OnSelectEvent?.Invoke(this);
    }

    public void Deselect()
    {
        WorkerAppearance.ShowOutline(false);
        Icon.ShowSelectBorder(false);
    }

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
        Icon.DisplayCooldownOverlay(!IsAvailable);
    }
    public void DoMaintenance(MemoryEntity entity)
    {
        StartCoroutine(WorkHandler.StartMaintaining(entity, this));
    }

    public void DoRepair(MemoryEntity entity)
    {
        StartCoroutine(WorkHandler.StartRepairing(entity, this));
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

    public bool TakeDamage(float value, ICombatant attacker)
    {
        if(Random.Range(0,100) < workerData.TotalStats.DamageIgnoreChance)
        {
            if(WorkerAppearance.gameObject.activeSelf)
            {
                PopupTextSpawner.Instance.SpawnPopupText(WorkerAppearance.transform, WorkerAppearance.transform.position, "Dodged", 0.5f, Color.white);
            }
            return false;
        }
        AddHealth(-value);
        return true;
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
    }



}

