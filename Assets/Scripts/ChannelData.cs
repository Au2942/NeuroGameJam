using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ChannelInfo
{
    public string name;
    public Entity entity;

    public ChannelInfo(string channelName, Entity channelEntity)
    {
        name = channelName;
        entity = channelEntity;
    }
}

public class ChannelData : MonoBehaviour
{
    [SerializeField] public List<ChannelInfo> ChannelInfos = new List<ChannelInfo>();

    public void AddChannel(string channelName, Entity channelEntity)
    {
        ChannelInfo newChannelInfo = new ChannelInfo
        {
            name = channelName,
            entity = channelEntity
        };
        ChannelInfos.Add(newChannelInfo);
    }

    public void AddChannel(ChannelInfo newChannelInfo)
    {
        ChannelInfos.Add(newChannelInfo);
    }


    public void RemoveChannel(ChannelInfo channelInfoToRemove)
    {
        ChannelInfos.Remove(channelInfoToRemove);
    }
    
    public void RemoveChannel(int index)
    {
        ChannelInfos.RemoveAt(index);
    }

    public string GetChannelName(int index)
    {
        if(index < 0 || index >= ChannelInfos.Count)
        {
            return null;
        }
        return ChannelInfos[index].name;
    }

    public string GetChannelName(Entity channelEntity)
    {
        string channelName = ChannelInfos.Find(x => x.entity == channelEntity).name;
        return channelName;
    }

    public int GetChannelIndex(string channelName)
    {
        int index = ChannelInfos.FindIndex(x => x.name == channelName);
        return index;
    }

    public int GetChannelIndex(Entity channelEntity)
    {
        int index = ChannelInfos.FindIndex(x => x.entity == channelEntity);
        return index;
    }

    public Entity GetChannelEntity(int index)
    {
        if(index < 0 || index >= ChannelInfos.Count)
        {
            return null;
        }
        return ChannelInfos[index].entity;
    }

    public Entity GetChannelEntity(string channelName)
    {
        Entity channelEntity = ChannelInfos.Find(x => x.name == channelName).entity;
        return channelEntity;
    }

    public List<Entity> GetChannelEntities()
    {
        List<Entity> entities = new List<Entity>();
        foreach(ChannelInfo channelInfo in ChannelInfos)
        {
            entities.Add(channelInfo.entity);
        }
        return entities;
    }

    public void ReplaceChannel(int index, string newName, Entity newEntity)
    {
        ChannelInfos[index] = new ChannelInfo(newName, newEntity);
    }

    public ChannelInfo GetChannelInfo(int index)
    {
        if(index < 0 || index >= ChannelInfos.Count)
        {
            return new ChannelInfo();
        }
        return ChannelInfos[index];
    }

    public ChannelInfo GetChannelInfo(string channelName)
    {
        ChannelInfo channelInfo = ChannelInfos.Find(x => x.name == channelName);
        return channelInfo;
    }

    public ChannelInfo GetChannelInfo(Entity channelEntity)
    {
        ChannelInfo channelInfo = ChannelInfos.Find(x => x.entity == channelEntity);
        return channelInfo;
    }

    public int GetChannelCount()
    {
        return ChannelInfos.Count;
    }

    public float GetChannelEntityIntegrityPercentage(int index)
    {
        if(index < 0 || index >= ChannelInfos.Count)
        {
            return 0;
        }
        ChannelInfo channelInfo = ChannelInfos[index];
        float result = channelInfo.entity.Health / channelInfo.entity.MaxHealth * 100;
        return result;
    }

    public float GetChannelEntityIntegrityPercentage(string channelName)
    {
        ChannelInfo channelInfo = ChannelInfos.Find(x => x.name == channelName);
        float result = channelInfo.entity.Health / channelInfo.entity.MaxHealth * 100;
        return result;
    }
}
