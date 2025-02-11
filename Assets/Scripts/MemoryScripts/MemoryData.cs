using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MemoryInfo
{
    public MemoryEntity Entity;
    public MemoryBlock Block;

    public MemoryInfo(MemoryEntity memoryEntity, MemoryBlock memoryBlock)
    {
        Entity = memoryEntity;
        Block = memoryBlock;
    }


    public void SetMemoryEntity(MemoryEntity newEntity)
    {
        Entity = newEntity;
    }
}
[System.Serializable]
public class MemoryData
{
    [SerializeField] public List<MemoryInfo> MemoryInfos = new List<MemoryInfo>();

    public MemoryInfo AddMemory(MemoryEntity memoryEntity)
    {
        MemoryInfo newMemoryInfo = new MemoryInfo
        {
            Entity = memoryEntity,
            Block = memoryEntity.MemoryBlock
        };
        MemoryInfos.Add(newMemoryInfo);
        return newMemoryInfo;
    }

    public void AddMemory(MemoryInfo newMemoryInfo)
    {
        MemoryInfos.Add(newMemoryInfo);
    }


    public void RemoveMemory(MemoryInfo memoryInfoToRemove)
    {
        MemoryInfos.Remove(memoryInfoToRemove);
    }
    
    public void RemoveMemory(int index)
    {
        MemoryInfos.RemoveAt(index);
    }

    public int GetMemoryIndex(Entity memoryEntity)
    {
        int index = MemoryInfos.FindIndex(x => x.Entity == memoryEntity);
        return index;
    }

    public MemoryEntity GetMemoryEntity(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return null;
        }
        return MemoryInfos[index].Entity;
    }


    public MemoryBlock GetMemoryBlock(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return null;
        }
        return MemoryInfos[index].Block;
    }

    public MemoryInfo GetMemoryInfo(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return new MemoryInfo();
        }
        return MemoryInfos[index];
    }


    public MemoryInfo GetMemoryInfo(Entity memoryEntity)
    {
        MemoryInfo memoryInfo = MemoryInfos.Find(x => x.Entity == memoryEntity);
        return memoryInfo;
    }

    public MemoryInfo GetMemoryInfo(MemoryBlock memoryBlock)
    {
        MemoryInfo memoryInfo = MemoryInfos.Find(x => x.Block == memoryBlock);
        return memoryInfo;
    }

    public int GetMemoryCount()
    {
        return MemoryInfos.Count;
    }

}
