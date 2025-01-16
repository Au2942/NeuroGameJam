using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PopupTextSpawner : MonoBehaviour
{
    public static PopupTextSpawner Instance;
    [SerializeField] private GameObject popupTextPrefab;
    [SerializeField] private AnimationClip popupTextAnimation;
    [SerializeField] private int poolSize = 30;


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

    public void SpawnPopupText(Transform parent, Vector3 position, string text, bool reuseActive = true)
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
        Animator animator = popupText.GetComponent<Animator>();
        if(animator != null)
        {
            animator.Play(popupTextAnimation.name);
        }
        StartCoroutine(ReturnToPool(popupText));
    }

    private IEnumerator ReturnToPool(GameObject popupText)
    {
        yield return new WaitForSeconds(popupTextAnimation.length);
        popupText.SetActive(false);
        popupTextPool.Enqueue(popupText);
    }



}