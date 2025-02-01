using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorkerScroller : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    private Coroutine smoothScrollCoroutine;

    void Start()
    {
        WorkerManager.Instance.OnWorkerSelectedEvent += SetSelectedWorker;
    }
    public void SetSelectedWorker(int index)
    {
        SetSelectedWorker(WorkerManager.Instance.Workers[index]);
    }
    public void SetSelectedWorker(Worker worker)
    {
        if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        RectTransform workerRect = worker.GetComponent<RectTransform>();
        smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(workerRect.GetComponent<RectTransform>().anchoredPosition.y));
    }

    IEnumerator SmoothScrollTo(float targetLocalY)
    {
        float duration = 0.3f; // Duration of the scroll animation
        float elapsedTime = 0f;
        float startNormalizedPosition = scrollRect.verticalNormalizedPosition;

        float contentHeight = scrollRect.content.rect.height; // Total height of content
        float viewportHeight = scrollRect.viewport.rect.height; // Visible height of viewport
        
        // Calculate normalized position
        float targetNormalizedPosition = Mathf.Clamp01(
            (contentHeight+targetLocalY - (viewportHeight/2)) / (contentHeight-viewportHeight)
        );

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float newPosition = Mathf.Lerp(startNormalizedPosition, targetNormalizedPosition, elapsedTime / duration);
            scrollRect.verticalNormalizedPosition = newPosition;
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetNormalizedPosition;
    }

    void OnDestroy()
    {
        WorkerManager.Instance.OnWorkerSelectedEvent -= SetSelectedWorker;
    }
}