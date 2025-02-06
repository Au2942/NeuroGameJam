using UnityEngine;
using System.Collections.Generic;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance;
    [SerializeField] private StatusEffectSO[] StatusEffectSO = new StatusEffectSO[0];
    private Dictionary<string, StatusEffectSO> statusEffectSOByID = new Dictionary<string, StatusEffectSO>();
    private Dictionary<string, StatusEffect> statusEffectByID = new Dictionary<string, StatusEffect>();
    public List<StatusEffect> ActiveStatusEffects = new List<StatusEffect>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        foreach(StatusEffectSO statusEffectSO in StatusEffectSO)
        {
            StatusEffect statusEffect = statusEffectSO.CreateStatusEffect();
            StatusEffectData data = statusEffect.GetData();
            statusEffectByID.TryAdd(data.ID, statusEffect);
            statusEffectSOByID.TryAdd(data.ID, statusEffectSO);
        }
    }

    public void Update()
    {
        for(int i = ActiveStatusEffects.Count - 1; i >= 0; i--) //iterate backwards to be able to remove items
        {
            StatusEffect statusEffect = ActiveStatusEffects[i];
            statusEffect.OnUpdate(Time.deltaTime);
            if(statusEffect.ShouldExpire())
            {   
                statusEffect.Expire();
                statusEffect.GetTarget().ChangeStatusEffectStack(statusEffect, statusEffect.GetData().Stack);
                if(statusEffect.GetData().Stack <= 0)
                {
                    statusEffect.GetTarget().RemoveStatusEffect(statusEffect);
                    ActiveStatusEffects.RemoveAt(i);
                }
            }
        }
    }

    public StatusEffect ApplyStatusEffect(string id, IStatusEffectable target, IStatusEffectSource source = null, int stack = 1 )
    {
        StatusEffect targetStatusEffect = target.StatusEffects.Find(x => x.GetData().ID == id);
        if(targetStatusEffect != null)
        {
            if(targetStatusEffect.GetData().Stackable && targetStatusEffect.TryAddStack(stack))
            {
                target.ChangeStatusEffectStack(targetStatusEffect, targetStatusEffect.GetData().Stack);
                return targetStatusEffect;
            }
            else return null;
        }
        else
        {
            StatusEffect statusEffect = CreateStatusEffect(id, stack);
            if(statusEffect != null)
            {
                statusEffect.Apply(target, source);
                statusEffect.GetTarget().ApplyStatusEffect(statusEffect, source);
                return statusEffect;
            }
            else return null;
        }
    }

    public StatusEffect CreateStatusEffect(string id, int stack = 1)
    {
        if(statusEffectSOByID.TryGetValue(id, out StatusEffectSO statusEffectSO))
        {
            StatusEffect statusEffect = statusEffectSO.CreateStatusEffect();
            statusEffect.GetData().Stack = stack;
            ActiveStatusEffects.Add(statusEffect);
            return statusEffect;
        }
        return null;
    }

    public T CreateStatusEffect<T> (string id, int stack = 1) where T : StatusEffect
    {
        if(statusEffectSOByID.TryGetValue(id, out StatusEffectSO statusEffectSO))
        {
            T statusEffect = statusEffectSO.CreateStatusEffect() as T;
            statusEffect.GetData().Stack = stack;
            ActiveStatusEffects.Add(statusEffect);
            return statusEffect;
        }
        return null;
    } 


    public StatusEffect CreateStatusEffect(StatusEffectSO statusEffectSO)
    {
        StatusEffect statusEffect = statusEffectSO.CreateStatusEffect();
        ActiveStatusEffects.Add(statusEffect);

        if(statusEffectByID.TryAdd(statusEffect.GetData().ID, statusEffect))
        {
            statusEffectSOByID.TryAdd(statusEffect.GetData().ID, statusEffectSO);
        }    

        return statusEffect;
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        ActiveStatusEffects.Remove(statusEffect);
    }

    public bool TryGetStatusEffectByID(string id, out StatusEffect statusEffect)
    {
        return statusEffectByID.TryGetValue(id, out statusEffect);
    }

    public bool TryGetStatusEffectDataByID(string id, out StatusEffectData data)
    {
        if(statusEffectByID.TryGetValue(id, out StatusEffect statusEffect))
        {
            data = statusEffect.GetData();
            return true;
        }
        data = null;
        return false;
    }

    public StatusEffect GetStatusEffectByID(string id)
    {
        return statusEffectByID[id];
    }

    public StatusEffectData GetStatusEffectDataByID(string id)
    {
        return statusEffectByID[id].GetData();
    }

}