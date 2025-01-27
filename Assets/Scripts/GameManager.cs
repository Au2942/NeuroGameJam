using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //[SerializeField] public MemoryData MemoryData;
    [SerializeField] private StreamSO defaultStream;
    [SerializeField] public ScreenEffectController ScreenEffectController;
    public int MemoryCount => MemoryManager.Instance.MemoryData.GetMemoryCount();
    public int CurrentMemoryIndex => MemoryManager.Instance.CurrentMemoryIndex;
    public List<MemoryEntity> MemoryEntities => MemoryManager.Instance.MemoryData.GetMemoryEntities();

    public bool isPause { get; set; } = false;



    public event Action OnStartStream;
    public event Action OnEndStream;

    void Awake()
    {
        if(Instance == null)
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
        StartNewStream(defaultStream);
    }


    void Update()
    {

        PlayerManager.Instance.CheckPlayerInput();
        
        if (isPause)
        { 
            return;
        }

        PlayerManager.Instance.ProgressStream();
        
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.sleep)
        {
            return;
        }

        MemoryManager.Instance.UpdateMemoryManager();

    }


    public void StartNewStream(StreamSO newStream)
    {
        PlayerManager.Instance.SetUpStream(newStream);
        MemoryManager.Instance.SetCurrentMemoryIndex(MemoryCount-1);
        OnStartStream?.Invoke();
    }

    public void ContinueStream()
    {
        MemoryEntity memoryEntity = MemoryManager.Instance.MemoryData.GetMemoryEntity(CurrentMemoryIndex);
        if(memoryEntity != null)
        {
            memoryEntity.SetInFocus(true);
        }
    }

    public void StopStream()
    {
        foreach(MemoryEntity entity in MemoryEntities)
        {
            entity.SetInFocus(false);
            entity.ShutUp();
        }
    }

    public void EndStream()
    {
        StopStream();
        MemoryManager.Instance.AddMemoryFromStream(PlayerManager.Instance.CurrentStream);
        OnEndStream?.Invoke();
    }


}
