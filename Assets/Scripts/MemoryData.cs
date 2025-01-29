using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MemoryInfo
{
    public string name;
    public MemoryEntity entity;

    public MemoryInfo(string memoryName, MemoryEntity memoryEntity)
    {
        name = memoryName;
        entity = memoryEntity;
    }

    public void SetMemoryName(string newName)
    {
        name = newName;
    }

    public void SetMemoryEntity(MemoryEntity newEntity)
    {
        entity = newEntity;
    }
}
[System.Serializable]
public class MemoryData
{
    [SerializeField] public List<MemoryInfo> MemoryInfos = new List<MemoryInfo>();

    public MemoryInfo AddMemory(string memoryName, MemoryEntity memoryEntity)
    {
        MemoryInfo newMemoryInfo = new MemoryInfo
        {
            name = memoryName,
            entity = memoryEntity
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

    public void SetMemoryName(int index, string newName)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return;
        }
        MemoryInfos[index].SetMemoryName(newName);
    }

    public string GetMemoryName(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return null;
        }
        return MemoryInfos[index].name;
    }

    public string GetMemoryName(Entity memoryEntity)
    {
        string memoryName = MemoryInfos.Find(x => x.entity == memoryEntity).name;
        return memoryName;
    }

    public int GetMemoryIndex(string memoryName)
    {
        int index = MemoryInfos.FindIndex(x => x.name == memoryName);
        return index;
    }

    public int GetMemoryIndex(Entity memoryEntity)
    {
        int index = MemoryInfos.FindIndex(x => x.entity == memoryEntity);
        return index;
    }

    public MemoryEntity GetMemoryEntity(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return null;
        }
        return MemoryInfos[index].entity;
    }

    public MemoryEntity GetMemoryEntity(string memoryName)
    {
        MemoryEntity memoryEntity = MemoryInfos.Find(x => x.name == memoryName).entity;
        return memoryEntity;
    }

    public List<MemoryEntity> GetMemoryEntities()
    {
        List<MemoryEntity> entities = new List<MemoryEntity>();
        foreach(MemoryInfo memoryInfo in MemoryInfos)
        {
            entities.Add(memoryInfo.entity);
        }
        return entities;
    }

    public void ReplaceMemory(int index, string newName, MemoryEntity newEntity)
    {
        MemoryInfos[index] = new MemoryInfo(newName, newEntity);
    }

    public MemoryInfo GetMemoryInfo(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return new MemoryInfo();
        }
        return MemoryInfos[index];
    }

    public MemoryInfo GetMemoryInfo(string memoryName)
    {
        MemoryInfo memoryInfo = MemoryInfos.Find(x => x.name == memoryName);
        return memoryInfo;
    }

    public MemoryInfo GetMemoryInfo(Entity memoryEntity)
    {
        MemoryInfo memoryInfo = MemoryInfos.Find(x => x.entity == memoryEntity);
        return memoryInfo;
    }

    public int GetMemoryCount()
    {
        return MemoryInfos.Count;
    }

    public float GetMemoryEntityIntegrityPercentage(int index)
    {
        if(index < 0 || index >= MemoryInfos.Count)
        {
            return 0;
        }
        MemoryInfo memoryInfo = MemoryInfos[index];
        float result = memoryInfo.entity.Health / memoryInfo.entity.MaxHealth * 100;
        return result;
    }

    public float GetMemoryEntityIntegrityPercentage(string memoryName)
    {
        MemoryInfo memoryInfo = MemoryInfos.Find(x => x.name == memoryName);
        float result = memoryInfo.entity.Health / memoryInfo.entity.MaxHealth * 100;
        return result;
    }
}
