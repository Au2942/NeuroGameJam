using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RenderLiveFeed : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Camera livefeedCamera;
    [SerializeField] private RectTransform contentUI;
    [SerializeField] private GameObject livefeedPrefab;
    [SerializeField] private GameObject selectBorderPrefab;
    [SerializeField] float refreshRate = 0.1f;

    private List<RawImage> renderImages = new List<RawImage>();
    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    private GameObject selectBorderInstance;
    private Vector3 initialPosition = new Vector3();
    private Coroutine smoothScrollCoroutine;

    void Start()
    {
        initialPosition = livefeedCamera.transform.localPosition;
        AddLivefeed(); //add stream livefeed
        TimelineManager.Instance.OnMemoryAdded += AddLivefeed;
        TimelineManager.Instance.OnChangeMemoryIndex += t => SetSelectedFeed(t, true); //reverse order

        selectBorderInstance = Instantiate(selectBorderPrefab, contentUI);
        SetSelectedFeed(0);

        StartCoroutine(RenderLiveFeedRoutine());
    }

    public void AddLivefeed()
    {
        GameObject livefeed = Instantiate(livefeedPrefab, contentUI);
        SelectLivefeed selectLivefeed = livefeed.GetComponent<SelectLivefeed>();
        RawImage rawImage = livefeed.GetComponentInChildren<RawImage>();
        if(rawImage != null)
        {
            renderImages.Add(rawImage);
            RenderTexture renderTexture = new RenderTexture(320, 180, 16);
            renderTextures.Add(renderTexture);
            rawImage.texture = renderTexture;
            selectLivefeed.SetIndex(renderImages.Count-1);
        }
    }

    public void SetSelectedFeed(int index, bool reverseOrder = false)
    {
        if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
        
        Transform renderImage;
        if(!reverseOrder)
            renderImage = renderImages[index].transform;
        else
            renderImage = renderImages[^(index+1)].transform;

        selectBorderInstance.transform.SetParent(renderImage.parent, false);
        selectBorderInstance.transform.SetAsFirstSibling();

        RectTransform rectTransform = renderImage.parent.GetComponent<RectTransform>();
        smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(rectTransform.localPosition.x));
    }

    IEnumerator RenderLiveFeedRoutine()
    {
        while(true)
        {
            livefeedCamera.transform.localPosition = initialPosition;
            for(int i = 0; i < renderTextures.Count; i++)
            {
                livefeedCamera.targetTexture = renderTextures[i];
                livefeedCamera.Render();
                livefeedCamera.targetTexture = null;
                livefeedCamera.transform.localPosition += new Vector3(1152, 0, 0);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
    
    IEnumerator SmoothScrollTo(float targetLocalX)
    {
        float duration = 0.3f; // Duration of the scroll animation
        float elapsedTime = 0f;
        float startNormalizedPosition = scrollRect.horizontalNormalizedPosition;

        float contentWidth = scrollRect.content.rect.width; // Total width of content
        float viewportWidth = scrollRect.viewport.rect.width; // Visible width of viewport

        // Calculate normalized position
        float targetNormalizedPosition = Mathf.Clamp01(
            (targetLocalX - (viewportWidth / 2)) / (contentWidth - viewportWidth)
        );

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newPosition = Mathf.Lerp(startNormalizedPosition, targetNormalizedPosition, elapsedTime / duration);
            scrollRect.horizontalNormalizedPosition = newPosition;
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = targetNormalizedPosition;
    }
}
