using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using PrimeTween;

public class PopupTextSpawner : MonoBehaviour
{
    public static PopupTextSpawner Instance;
    [SerializeField] private GameObject popupTextPrefab;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private float defaultDuration = 0.5f;
    [SerializeField] private Ease defaultScaleEase = Ease.OutSine;
    [SerializeField] private Ease defaultAlphaEase = Ease.InExpo;


    private Queue<GameObject> popupTextPool = new Queue<GameObject>();
    private Queue<GameObject> activePopupTextPool = new Queue<GameObject>();
    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newPopupText = Instantiate(popupTextPrefab, transform);
            newPopupText.SetActive(false);
            popupTextPool.Enqueue(newPopupText);
        }
    }

    public void SpawnPopupText(Transform parent, Vector3 position, string text, float duration, bool reuseActive = true)
    {
        GameObject popupText;
        RectTransform popupRectTransform;
        if (popupTextPool.Count == 0)
        {
            if(!reuseActive || activePopupTextPool.Count == 0)
            {
                return;
            }
            popupText = activePopupTextPool.Dequeue();
        }
        else 
        {
            popupText = popupTextPool.Dequeue();
        }
        popupRectTransform = popupText.GetComponent<RectTransform>();
        popupRectTransform.SetParent(parent);
        popupRectTransform.position = position;
        popupText.GetComponent<TextMeshProUGUI>().text = text;
        popupText.SetActive(true);

        Tween.Scale(popupText.transform, Vector3.one*scaleMultiplier, duration, ease: defaultScaleEase);
        Tween.Alpha(popupText.GetComponent<TextMeshProUGUI>(), 1, 0, duration, ease: defaultAlphaEase);

        StartCoroutine(ReturnToPool(popupText, duration));
    }

    public void SpawnPopupText(Transform parent, Vector3 position, string text, bool reuseActive = true)
    {
        SpawnPopupText(parent, position, text, defaultDuration, reuseActive);
    }

    private IEnumerator ReturnToPool(GameObject popupText, float duration)
    {
        yield return new WaitForSeconds(duration);

        popupText.transform.localScale = Vector3.one;
        popupText.GetComponent<TextMeshProUGUI>().alpha = 1;

        popupText.SetActive(false);
        popupTextPool.Enqueue(popupText);
    }



}