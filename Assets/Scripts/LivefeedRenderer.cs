using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LivefeedRenderer : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Camera livefeedCamera;
    [SerializeField] private RectTransform contentUI;
    [SerializeField] private Livefeed livefeedPrefab;
    [SerializeField] private GameObject selectBorderPrefab;
    [SerializeField] private List<RawImage> renderImages = new List<RawImage>();
    [SerializeField] private Vector2Int resolution = new Vector2Int(320, 180);
    [SerializeField] float refreshRate = 0.1f;

    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    private GameObject selectBorderInstance;
    private Vector3 initialPosition = new Vector3();
    private Coroutine smoothScrollCoroutine;

    void Start()
    {
        initialPosition = livefeedCamera.transform.localPosition;
        ChannelNavigationManager.Instance.OnNewStream += AddLivefeed;
        ChannelNavigationManager.Instance.OnChangeChannelIndex += t => SetSelectedFeed(t); 

        selectBorderInstance = Instantiate(selectBorderPrefab, contentUI);

        StartCoroutine(RenderLivefeedsRoutine());
    }

    public void AddLivefeed()
    {
        Livefeed livefeed = Instantiate(livefeedPrefab, contentUI);
        LivefeedManager.Instance.AddLivefeed(livefeed);
        RawImage rawImage = livefeed.GetComponentInChildren<RawImage>();
        if(rawImage != null)
        {
            int livefeedIndex = renderImages.Count;
            renderImages.Add(rawImage);
            RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, 16);
            renderTextures.Add(renderTexture);
            rawImage.texture = renderTexture;
            livefeed.SetLivefeed(GameManager.Instance.ChannelData.GetChannelName(livefeedIndex) ,livefeedIndex); 
        }
    }
    public void AddLivefeed(string name)
    {
        Livefeed livefeed = Instantiate(livefeedPrefab, contentUI);
        LivefeedManager.Instance.AddLivefeed(livefeed);
        RawImage rawImage = livefeed.GetComponentInChildren<RawImage>();
        if(rawImage != null)
        {
            int livefeedIndex = renderImages.Count;
            renderImages.Add(rawImage);
            RenderTexture renderTexture = new RenderTexture(320, 180, 16);
            renderTextures.Add(renderTexture);
            rawImage.texture = renderTexture;
            livefeed.SetLivefeed(name ,livefeedIndex); 
        }
    }

    public void SetSelectedFeed(int index)
    {
        if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
        
        RectTransform renderImage = renderImages[index].GetComponent<RectTransform>();
        
        selectBorderInstance.transform.SetParent(renderImage.parent, false);
        selectBorderInstance.transform.SetAsFirstSibling();

        smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(renderImage.parent.GetComponent<RectTransform>().anchoredPosition.x));
    }

    IEnumerator RenderLivefeedsRoutine()
    {
        while(true)
        {
            RenderLivefeeds();
            yield return new WaitForSeconds(refreshRate);
        }
    }

    public void RenderLivefeeds()
    {
        livefeedCamera.transform.localPosition = initialPosition;
        for(int i = 0; i < renderTextures.Count; i++)
        {
            livefeedCamera.targetTexture = renderTextures[i];
            livefeedCamera.Render();
            livefeedCamera.targetTexture = null;
            livefeedCamera.transform.localPosition -= new Vector3(ChannelNavigationManager.Instance.spacing, 0, 0);
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
