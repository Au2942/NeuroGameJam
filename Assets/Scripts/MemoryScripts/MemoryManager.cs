using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour 
{
    public static MemoryManager Instance;
    public MemoryData MemoryData = new MemoryData();
    public MemoryNavigator MemoryNavigator;
    public int MemoryCount => MemoryData.MemoryInfos.Count;
    public int CurrentMemoryIndex => MemoryNavigator.CurrentMemoryIndex;
    public Dictionary<string, int> MemoryTypesCount {get; set;} = new Dictionary<string, int>();



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

    public void CorruptRandomMemory(float corruptionAmount)
    {
        if(MemoryCount == 0)
        {
            return;
        }
        List<MemoryEntity> targets = new();
        foreach(var memoryInfo in MemoryData.MemoryInfos)
        {
            if(memoryInfo.Entity.CorruptionCooldownTimer <= 0 && !memoryInfo.Entity.IsBeingWorkedOn)
            {
                targets.Add(memoryInfo.Entity);
            }
        }
        if(targets.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, targets.Count);
        targets[randomIndex].CorruptPlayback(corruptionAmount);
    }


    public void UpdateMemoryManager()
    {
        MemoryNavigator.CheckNavigationInput();
    }


    public GameObject AddMemoryFromStream(StreamSO stream)
    {
        if(!MemoryTypesCount.TryAdd(stream.name, 1))
        {
            MemoryTypesCount[stream.name]++;
        }

        MemoryEntity memory = Instantiate(stream.memory);
        memory.PlaybackTimeline.SetupPlaybackTimeline(PlayerManager.Instance.CurrentStreamTimer);
        MemoryInfo memoryInfo = MemoryData.AddMemory(memory);
        
        MemoryNavigator.SetupMemoryBlock("Memory of " + stream.name + "stream #" + MemoryTypesCount[stream.name], memoryInfo);
        MemoryNavigator.SetCurrentMemoryIndex(CurrentMemoryIndex, false);

        return memory.gameObject;
    }

    
}
