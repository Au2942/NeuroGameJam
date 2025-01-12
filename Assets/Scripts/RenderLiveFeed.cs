using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RenderLiveFeed : MonoBehaviour
{
    [SerializeField] private Camera livefeedCamera;
    [SerializeField] private RectTransform contentUI;
    [SerializeField] private GameObject livefeedPrefab;
    [SerializeField] float refreshRate = 0.1f;

    private List<RawImage> renderImages = new List<RawImage>();
    private List<RenderTexture> renderTextures = new List<RenderTexture>();

    private Vector3 initialPosition = new Vector3();

    void Start()
    {
        initialPosition = livefeedCamera.transform.localPosition;
        AddLivefeed(); //add stream livefeed
        TimelineManager.Instance.OnMemoryAdded += AddLivefeed;
        StartCoroutine(RenderLiveFeedRoutine());
    }

    public void AddLivefeed()
    {
        GameObject livefeed = Instantiate(livefeedPrefab, contentUI);
        RawImage rawImage = livefeed.GetComponent<RawImage>();
        if(rawImage != null)
        {
            renderImages.Add(rawImage);
            RenderTexture renderTexture = new RenderTexture(320, 180, 16);
            renderTextures.Add(renderTexture);
            rawImage.texture = renderTexture;
        }
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
                livefeedCamera.transform.localPosition += new Vector3(1440, 0, 0);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
