using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LivefeedManager : MonoBehaviour
{
    public static LivefeedManager Instance;
    [SerializeField] public List<Livefeed> Livefeeds = new List<Livefeed>();
    [SerializeField] public LivefeedScroller LivefeedScroller;
    
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

    public void AddLivefeed(Livefeed newLivefeed)
    {
        Livefeeds.Add(newLivefeed);
    }

}
