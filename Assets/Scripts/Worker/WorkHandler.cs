using UnityEngine;
using System.Collections;

public class WorkHandler
{
    public MemoryEntity entity;
    public Worker worker;
    public bool isMaintaining = false;
    public bool isRepairing = false;

    public bool IsWorking => isMaintaining || isRepairing;
    public void InitiateMaintenanceWork(MemoryEntity entity, Worker worker)
    {
        isMaintaining = true;
        this.entity = entity;
        this.worker = worker;
        StartMaintaining();
    }

    public void StartMaintaining()
    {
        if(entity == null) return;
        entity.IsBeingWorkedOn = true;
        entity.Interactable = false;

        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnStartMaintain();
            statusEffect.OnStartWork();
        }

        worker.StartCoroutine(Maintaining());
    }

    private IEnumerator Maintaining()
    {
        yield return new WaitForSeconds(worker.Stats.WorkSpeed); //implement a system to recall worker before they finish maintaining
        RollMaintainSuccessChance();
        FinishMaintaining();
    }

    private void RollMaintainSuccessChance()
    {
        if(entity == null) return;
        float successChance = worker.Stats.WorkSuccessChance;
        float roll = Random.Range(0, 100);

        if(successChance >= 100)
        {
            while(successChance > 100)
            {
                OnMaintainSuccess();
                successChance -= 100;
            }
            if(roll < successChance)
            {
                OnMaintainSuccess();
            }
        }
        else
        {
            if(roll < successChance)
            {
                OnMaintainSuccess();
            }
            else
            {
                OnMaintainFail();
            }
        }
    }

    private void OnMaintainSuccess()
    {
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnMaintainSuccess();
            statusEffect.OnWorkSuccess();
        }

        entity.MaintainSuccess(worker);
    }

    private void OnMaintainFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnMaintainFail();
            statusEffect.OnWorkFail();
        }

        entity.MaintainFail(worker);
    }

    public virtual void FinishMaintaining()
    {
        if(entity == null) return;
        entity.IsBeingWorkedOn = false;
        entity.Interactable = true;
        entity = null;
        isMaintaining = false;
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnFinishMaintain();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(worker.StartCooldown());
    }


    public void InitiateRepairWork(MemoryEntity entity, Worker worker)
    {
        isRepairing = true;
        this.entity = entity;
        this.worker = worker;
        StartRepairing();
    }

    public void StartRepairing()
    {
        if(entity == null) return;
        entity.IsBeingWorkedOn = true;
        entity.Interactable = false;

        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnStartRepair();
            statusEffect.OnStartWork();
        }

        worker.StartCoroutine(Repairing());
    }

    private IEnumerator Repairing()
    {
        //heal entity until it's no longer glitched
        while(entity.Glitched)
        {
            if(worker.health <= 0)
            {
                //deal with worker death
                yield break;
            }
            yield return new WaitForSeconds(worker.Stats.WorkSpeed); //implement a system to recall worker before they finish repairing
            RollRepairSuccessChance();
        }
        FinishRepairing();

    }

    private void RollRepairSuccessChance()
    {
        if(entity == null) return;
        float successChance = worker.Stats.WorkSuccessChance;
        float roll = Random.Range(0, 100);

        if(successChance >= 100)
        {
            while(successChance > 100)
            {
                OnRepairSuccess();
                successChance -= 100;
            }
            if(roll < successChance)
            {
                OnRepairSuccess();
            }
        }
        else
        {
            if(roll < successChance)
            {
                OnRepairSuccess();
            }
            else
            {
                OnRepairFail();
            }
        }
    }

    private void OnRepairSuccess()
    {
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnRepairSuccess();
            statusEffect.OnWorkSuccess();
        }
        entity.AddStability(worker.Stats.WorkAmount);
    }

    private void OnRepairFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnRepairFail();
            statusEffect.OnWorkFail();
        }
    }

    private void FinishRepairing()
    {
        if(entity == null) return;
        entity.IsBeingWorkedOn = false;
        entity.Interactable = true;
        entity = null;
        isRepairing = false;
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnFinishRepair();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(worker.StartCooldown());
    }

}