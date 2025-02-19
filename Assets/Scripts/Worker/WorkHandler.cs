using UnityEngine;
using System.Collections;
using PrimeTween;

[System.Serializable]
public class WorkHandler
{
    public MemoryEntity entity;
    public Worker worker;
    public WorkState workState;
    private RectTransform entityCell;
    private PlaybackTimeline entityPlaybackTimeline;
    private RectTransform workerAppearanceRect;

    public enum WorkState
    {
        None,
        Maintaining,
        Repairing
    }

    private void InitializeWork(MemoryEntity entity, Worker worker)
    {
        this.entity = entity;
        this.worker = worker;
        worker.SetAvailability(false);
        worker.WorkerAppearance.gameObject.SetActive(true);
        entityCell = entity.EntityCell != null ? entity.EntityCell : entity.transform as RectTransform;
        entityPlaybackTimeline = entity.PlaybackTL;
        entityPlaybackTimeline.PausePlayback(true);
        workerAppearanceRect = worker.WorkerAppearance.AppearanceRect;
    }

    private IEnumerator MoveToEnterCell()
    {

        Vector2 mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(InputManager.Instance.Point.ReadValue<Vector2>());
        workerAppearanceRect.position = mousePosition;
        workerAppearanceRect.SetParent(entityCell);
        workerAppearanceRect.SetAsLastSibling();
        workerAppearanceRect.localScale = Vector3.one;

        yield return worker.StartCoroutine(worker.WorkerAppearance.PlayTeleportEffect(worker.TotalStats.ResponseTime));

        entity.IsBeingWorkedOn = true;
        entity.Interactable = false;
    }

    private IEnumerator MoveToDoTask()
    {
        if(entityPlaybackTimeline != null && entityPlaybackTimeline.BasePlayback != null)
        {   
            if(!entityPlaybackTimeline.HasCorruptedSegments())
            {
                worker.StartCoroutine(ReturningWorker());
                yield break;
            } 
            if(!entityPlaybackTimeline.BasePlayback.InCorruptedPart())
            {
                //TODO: have it choose the nearest corrupted segment
                float targetTime = entityPlaybackTimeline.BasePlayback.CorruptedSegments[0].Start;
                float timeToTarget = targetTime > entityPlaybackTimeline.GetCurrentPlaybackTime() ? 
                    targetTime - entityPlaybackTimeline.GetCurrentPlaybackTime() : entityPlaybackTimeline.GetCurrentPlaybackTime() - targetTime;
                float elapsedTime = 0;
                while(elapsedTime < timeToTarget)
                {
                    float normalizedDeltaTime = entityPlaybackTimeline.BasePlayback.NormalizeTime(Time.deltaTime);
                    float nextTime = entityPlaybackTimeline.GetCurrentPlaybackTime() + normalizedDeltaTime;
                    elapsedTime += normalizedDeltaTime;
                    entityPlaybackTimeline.SetPlaybackScrollValue(nextTime);
                    yield return null;
                }

                entityPlaybackTimeline.SetPlaybackScrollValue(targetTime);

                // float timeToMove = targetTime - currentTime;
                // yield return Tween.Custom(
                //     currentTime, 
                //     targetTime,
                //     (1+timeToMove) * worker.TotalStats.TaskTime,
                //     onValueChange: newVal => entityPlaybackTimeline.SetActivePlaybackTime(newVal)
                // ).ToYieldInstruction();
            }

            entityPlaybackTimeline.LockScrollbar(true);
            Vector2 taskPosition = new Vector3(Random.Range(-entityCell.rect.width/2, entityCell.rect.width/2), Random.Range(-entityCell.rect.height/2, entityCell.rect.height/2));
            //TODO make taskPosition distance based on worker's speed
            Tween.UIAnchoredPosition(workerAppearanceRect, taskPosition, worker.TotalStats.TaskTime/2, ease: Ease.InSine);
            yield return new WaitForSeconds(worker.TotalStats.TaskTime);
            entityPlaybackTimeline.LockScrollbar(false);
        }

    }

    private void OnWorkSuccess()
    {
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnWorkSuccess();
        }
        entity.WorkSuccess(worker);
    }

    private void OnWorkFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnWorkFail();
        }
        entity.WorkFail(worker);
    }

    public IEnumerator StartMaintaining(MemoryEntity entity, Worker worker)
    {
        workState = WorkState.Maintaining;
        InitializeWork(entity, worker);

        yield return worker.StartCoroutine(MoveToEnterCell());

        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnStartMaintain();
            statusEffect.OnStartWork();
        }
        worker.StartCoroutine(Maintaining());
    }

    private IEnumerator Maintaining()
    {
        int failCount = 0;
        for(int i = 0; i < worker.TotalStats.TaskExecutionCount; i++)
        {   
            //implement a system to recall worker before they finish maintaining
            yield return worker.StartCoroutine(MoveToDoTask()); 
            if(!RollMaintainSuccessChance())
            {
                failCount++;
            }
            if(entity == null || entity.Health >= entity.MaxHealth) break;
        }

        if(failCount > worker.TotalStats.TaskExecutionCount/2)
        {
            OnWorkFail();
        }
        else
        {
            OnWorkSuccess();
        }

        FinishMaintaining();
    }


    private bool RollMaintainSuccessChance()
    {
        if(entity == null) return true;
        float successChance = worker.TotalStats.TaskSuccessChance;
        float roll = Random.Range(0, 100);

        if(successChance >= 100)
        {
            while(successChance > 100)
            {
                successChance -= 100;
                OnMaintainSuccess();
            }
            if(roll < successChance)
            {
                OnMaintainSuccess();
            }
            return true;
        }
        else
        {
            if(roll < successChance)
            {
                OnMaintainSuccess();
                return true;
            }
            else
            {
                OnMaintainFail();
                return false;
            }
        }
    }

    private void OnMaintainSuccess()
    {
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnMaintainSuccess();
        }
        entity.RestorePlayback(worker.TotalStats.RestoreAmount);
        entity.MaintainSuccess(worker);
    }

    private void OnMaintainFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnMaintainFail();
        }

        float reliabilityRollCheck = Random.Range(0, 100);

        entity.MaintainFail(worker);

    }

    public virtual void FinishMaintaining()
    {
        if(entity == null) return;
        entity.Interactable = true;
        entity.IsBeingWorkedOn = false;
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnFinishMaintain();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(ReturningWorker());
    }


    public IEnumerator StartRepairing(MemoryEntity entity, Worker worker)
    {
        workState = WorkState.Repairing;
        InitializeWork(entity, worker);

        yield return worker.StartCoroutine(MoveToEnterCell());

        entity.AddCombatTarget(worker);

        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnStartRepair();
            statusEffect.OnStartWork();
        }

        worker.StartCoroutine(Repairing());
    }

    private IEnumerator Repairing()
    {
        while(entity.ErrorIndex > 0)
        {
            for(int i = 0; i < worker.TotalStats.TaskExecutionCount; i++)
            {
                yield return worker.StartCoroutine(MoveToDoTask());
                RollRepairSuccessChance();
                if(entity == null || entity.ErrorIndex <= 0) break;
            }
            yield return new WaitForSeconds(worker.TotalStats.ResponseTime); //cooldown before next repair
        }

        OnWorkSuccess();
        FinishRepairing();
    }

    public void RollRepairSuccessChance()
    {
        if(entity == null) return;
        float successChance = worker.TotalStats.TaskSuccessChance;
        float roll = Random.Range(0, 100);

        if(successChance >= 100)
        {
            while(successChance > 100)
            {
                successChance -= 100;
                OnRepairSuccess();
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
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnRepairSuccess();
        }
        entity.RestorePlayback(worker.TotalStats.RestoreAmount);
    }

    private void OnRepairFail()
    {
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnRepairFail();
        }
    }
    private void FinishRepairing()
    {
        if(entity == null) return;
        entity.RestorePlayback(worker.TotalStats.RestoreAmount);
        foreach(WorkerStatusEffect statusEffect in worker.StatusEffects)
        {
            statusEffect.OnFinishRepair();
            statusEffect.OnFinishWork();
        }
        worker.StartCoroutine(ReturningWorker());
    }
    public IEnumerator ReturningWorker()
    {
        entity.Interactable = true;
        float elapsedTime = 0f;
        worker.StartCoroutine(worker.WorkerAppearance.PlayTeleportEffect(worker.TotalStats.ResponseTime, true));
        while(elapsedTime < worker.TotalStats.ResponseTime)
        {
            worker.Icon.SetCooldownOverlay(1-(elapsedTime / worker.TotalStats.ResponseTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ReturnWorkerAppearance();
        ReturnWorker();
    }

    public void ReturnWorkerAppearance()
    {
        workerAppearanceRect.SetParent(worker.transform);
        workerAppearanceRect.SetAsFirstSibling();
        workerAppearanceRect.position = worker.transform.position;
        workerAppearanceRect.localScale = Vector3.one;
        worker.WorkerAppearance.gameObject.SetActive(false);
    }


    public void ReturnWorker()
    {
        entityPlaybackTimeline.PausePlayback(false);
        entity = null;
        worker.SetAvailability(true);
        worker.Icon.SetCooldownOverlay(1);
        workState = WorkState.None;
        if(WorkerManager.Instance.SelectedWorker == worker)
        {
            WorkerManager.Instance.DeselectWorker();
        }
    }   




}