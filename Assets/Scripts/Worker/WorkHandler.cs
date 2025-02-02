using UnityEngine;
using System.Collections;
using PrimeTween;

public class WorkHandler
{
    public MemoryEntity entity;
    public Worker worker;
    public WorkState workState;
    private RectTransform entityCell;
    private RectTransform workerAppearanceRect;

    public enum WorkState
    {
        None,
        Maintaining,
        Repairing
    }

    public void MoveWorkerAppearance()
    {
        worker.WorkerAppearance.gameObject.SetActive(true);
        entityCell = entity.EntityCell != null ? entity.EntityCell : entity.transform as RectTransform;
        workerAppearanceRect = worker.WorkerAppearance.AppearanceRect;

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(InputManager.Instance.Point.ReadValue<Vector2>());
        
        workerAppearanceRect.position = targetPosition;
        workerAppearanceRect.SetParent(entityCell);
        workerAppearanceRect.SetAsFirstSibling();
        workerAppearanceRect.localScale = Vector3.one;
    }


    public void StartMaintaining(MemoryEntity entity, Worker worker)
    {
        workState = WorkState.Maintaining;
        this.entity = entity;
        this.worker = worker;
        worker.SetAvailability(false);
        MoveWorkerAppearance();
        entity.IsBeingMaintained = true;
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
        yield return new WaitForSeconds(worker.TotalStats.WorkTime); //implement a system to recall worker before they finish maintaining
        RollMaintainSuccessChance();
        FinishMaintaining();
    }

    private void RollMaintainSuccessChance()
    {
        if(entity == null) return;
        float successChance = worker.TotalStats.WorkSuccessChance;
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
        entity.Interactable = true;
        entity.IsBeingMaintained = false;
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnFinishMaintain();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(RecallCooldown());
    }


    public void StartRepairing(MemoryEntity entity, Worker worker)
    {
        workState = WorkState.Repairing;
        this.entity = entity;
        this.worker = worker;
        worker.SetAvailability(false);
        MoveWorkerAppearance();
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
        
        CombatHandler combatHandler = CombatManager.Instance.EngageInCombat(worker, entity);
        if(combatHandler == null)
        {
            Recall(); //entity is not combat ready
            yield break;
        }

        while(combatHandler.CheckIsInCombat())
        {
            yield return null;
        }
        FinishRepairing();
    }

    private void FinishRepairing()
    {
        if(entity == null) return;
        entity.RestoreHealth(worker.TotalStats.WorkAmount);
        entity.Interactable = true;
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnFinishRepair();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(RecallCooldown());
    }

    public void RollRepairSuccessChance()
    {
        if(entity == null) return;
        float successChance = worker.TotalStats.WorkSuccessChance;
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
        worker.DealDamage(worker.CombatTargets[0]);
    }

    private void OnRepairFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.WorkerStatusEffects)
        {
            statusEffect.OnRepairFail();
            statusEffect.OnWorkFail();
        }
    }

    public IEnumerator RecallCooldown()
    {
        float elapsedTime = 0f;;
        AnimateReturnWorkerAppearance(worker.TotalStats.RecallCooldown);
        while(elapsedTime < worker.TotalStats.RecallCooldown)
        {
            worker.CooldownOverlay.fillAmount = 1-(elapsedTime / worker.TotalStats.RecallCooldown);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Recall();
    }

    public void AnimateReturnWorkerAppearance(float duration)
    {
        Vector2 targetPosition = workerAppearanceRect.anchoredPosition + new Vector2(0, entityCell.rect.height + workerAppearanceRect.rect.height);
        Tween.UIAnchoredPosition(workerAppearanceRect, targetPosition, duration, ease: Ease.InSine);
    }

    public void ReturnWorkerAppearance()
    {
        workerAppearanceRect.SetParent(worker.transform);
        workerAppearanceRect.SetAsFirstSibling();
        workerAppearanceRect.position = worker.transform.position;
        workerAppearanceRect.localScale = Vector3.one;
        worker.WorkerAppearance.gameObject.SetActive(false);
    }

    public void Recall()
    {
        entity = null;
        worker.SetAvailability(true);
        ReturnWorkerAppearance();
        worker.CooldownOverlay.fillAmount = 1;
        workState = WorkState.None;
        if(WorkerManager.Instance.SelectedWorker == worker)
        {
            WorkerManager.Instance.DeselectWorker();
        }
    }   


}