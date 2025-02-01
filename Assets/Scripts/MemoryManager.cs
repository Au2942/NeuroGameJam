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
            if(memoryInfo.entity.CorruptionCooldownTimer <= 0 && !memoryInfo.entity.IsBeingMaintained)
            {
                targets.Add(memoryInfo.entity);
            }
        }
        if(targets.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, targets.Count);
        targets[randomIndex].DamageHealth(corruptionAmount);
    }

    public void SetCurrentMemoryIndex(int newIndex)
    {
        MemoryNavigator.SetCurrentMemoryIndex(newIndex);
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
        MemoryInfo memoryInfo = MemoryData.AddMemory("Memory of " + stream.name + "stream #" + MemoryTypesCount[stream.name], memory);
        
        MemoryNavigator.SetupMemoryBlock(memory, memoryInfo);
        MemoryNavigator.SetCurrentMemoryIndex(CurrentMemoryIndex);

        return memory.gameObject;
    }

    
}
