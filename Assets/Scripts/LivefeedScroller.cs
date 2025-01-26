using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LivefeedScroller : MonoBehaviour
{
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] private Camera livefeedCamera;
    [SerializeField] public RectTransform contentUI;
    [SerializeField] private Livefeed livefeedPrefab;
    [SerializeField] private GameObject selectBorderPrefab;
    [SerializeField] private List<RawImage> renderImages = new List<RawImage>();
    [SerializeField] private Vector2Int resolution = new Vector2Int(320, 180);
    [SerializeField] float refreshRate = 0.1f;

    private List<RenderTexture> renderTextures = new List<RenderTexture>();
    private GameObject selectBorderInstance;
    private Vector3 initialPosition = new Vector3();
    private Coroutine smoothScrollCoroutine;

    public event System.Action OnScrolling;

    void Start()
    {
        initialPosition = livefeedCamera.transform.localPosition;
        ChannelNavigationManager.Instance.OnNewStream += AddLivefeed;
        ChannelNavigationManager.Instance.OnChangeChannelIndex += t => SelectFeed(t); 

        selectBorderInstance = Instantiate(selectBorderPrefab, contentUI);

        StartCoroutine(RenderLivefeedsRoutine());
    }

    public void AddLivefeed()
    {
        Livefeed livefeed = Instantiate(livefeedPrefab, contentUI);
        livefeed.transform.SetSiblingIndex(2 + renderImages.Count); //after 2 buffers
        LivefeedManager.Instance.AddLivefeed(livefeed);
        scrollRect.onValueChanged.AddListener(delegate {livefeed.OnScroll();});
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

    public void SelectFeed(int index)
    {
        if(smoothScrollCoroutine != null) StopCoroutine(smoothScrollCoroutine);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        RectTransform renderImage = renderImages[index].GetComponent<RectTransform>();
        
        selectBorderInstance.transform.SetParent(renderImage.parent, false);
        selectBorderInstance.transform.SetAsFirstSibling();
        selectBorderInstance.GetComponent<RectTransform>().sizeDelta = renderImage.sizeDelta;

        smoothScrollCoroutine = StartCoroutine(SmoothScrollTo(renderImage.parent.GetComponent<RectTransform>()));
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
    
    IEnumerator SmoothScrollTo(RectTransform rect)
    {
        float rectX = rect.anchoredPosition.x;
        float duration = 0.3f; // Duration of the scroll animation
        float elapsedTime = 0f;

        RectTransform content = scrollRect.content;
        float contentWidth = scrollRect.content.rect.width; // Total width of content
        float contentOffset = scrollRect.content.anchoredPosition.x; // Offset of content
        float targetX = contentWidth/2 - rectX;
    

        while (elapsedTime < duration)
        {
            // Calculate normalized position

            elapsedTime += Time.unscaledDeltaTime;
            targetX = scrollRect.content.rect.width/2 - rectX;
            float newPosition = Mathf.Lerp(contentOffset, targetX, elapsedTime / duration);
            content.localPosition = new Vector2(newPosition, content.anchoredPosition.y);
            yield return null;
        }

        content.localPosition = new Vector2(targetX, content.anchoredPosition.y);
    }
}
