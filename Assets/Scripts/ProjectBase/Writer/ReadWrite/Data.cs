using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
[Serializable]
public class AttackData
{
    public int id;
    public string name;
    public string description;
    public Sprite img;
    public GameObject gameObject;
}

public class FriendRoom
{
    public string Name;
    //public ServerResponse localIP;
    public ulong steamIP;
}
[Serializable]
public class SpriteData
{
    public string name;
    public List<Sprite> sprites;
}
[Serializable]
public class EffectData
{
    public int id;
    public string name;
    public bool ifContinue;
    public EEffectType type;
    public GameObject gb;
}
public enum EEffectType
{
    fore,
    back,
    free
}
[Serializable]
public class UserData
{
    public string account;
    public string password;
    public string language = "Chinese";

    public List<ModState> mods = new List<ModState>();

    public float volume = 0.3f;
    public float SoundValue = 0.3f;
}

public class ModState
{
    public string name;
    public string description;
    public string modID;
    public bool isActive;
    public ModState() { }
    public ModState(string name, string description, string modID, bool isActive)
    {
        this.name = name;
        this.description = description;
        this.modID = modID;
        this.isActive = isActive;
    }
}

public class PlayerData
{
    public long id;
    public string name;
    public List<float> numValue;
    public float searchStrength;    //搜索强度
    public float maxSearchAmount;   //最大搜索数量
    public float searchCost;        //搜索消耗
    public float jumpCost;          //跳跃消耗
    public float transitionCost;    //跃迁消耗

    public List<EventData> eventDatas = new List<EventData>();
    public Dictionary<string, EventPool> pools = new Dictionary<string, EventPool>();
    public List<InventoryItem> box = new List<InventoryItem>();
    public List<EventData> eventDataList = new List<EventData>();
    public EventData activeEvent;
    public bool ifHaveActiveEvent;
    public Dictionary<string,float> state = new Dictionary<string,float>();
}
[Serializable]
public class EventData
{
    public string id;
    public string name;
    public string content;
    public E_EventTouchType touchType;
    public E_EventQuality quality;
    public List<ResourceDetile> resources;
    public List<InventoryItem> itemDetails;

    public List<ChoiceDetile> choiceDetile;
    public List<string> unlockEvent;
    public EventData()
    {
        id = "";
        name = "";
        content = "";
        resources = new List<ResourceDetile>();
        itemDetails = new List<InventoryItem>();
        choiceDetile = new List<ChoiceDetile>();
        unlockEvent = new List<string>();
    }
    public EventData (string id, string name, string content, E_EventTouchType touchType)
    {
        this.id = id;
        this.name = name;
        this.content = content;
        this.touchType = touchType;

        resources = new List<ResourceDetile>();
        itemDetails = new List<InventoryItem>();
        choiceDetile = new List<ChoiceDetile>();
        unlockEvent = new List<string>();

    }

    public EventData(EventData eventData)
    {
        if(eventData == null)
        {
            return;
        }
        id = eventData.id;
        name = eventData.name;
        content = eventData.content;
        touchType = eventData.touchType;
        quality = eventData.quality;
        resources = new List<ResourceDetile>(eventData.resources);
        itemDetails = new List<InventoryItem>(eventData.itemDetails);
        choiceDetile = new List<ChoiceDetile>();
        foreach(ChoiceDetile cd in eventData.choiceDetile)
        {
            choiceDetile.Add(new ChoiceDetile(cd));
        }
        unlockEvent = new List<string>(eventData.unlockEvent);
    }
 }
[Serializable]
public class EventPool
{
    public string poolName;
    public List<EventPoolData> eventPoolData = new List<EventPoolData>();

    public EventPool() { }
    public EventPool(string poolName)
    {
        this.poolName = poolName;
    }
    public EventPool(EventPool ep)
    {
        poolName = ep.poolName;
        foreach(EventPoolData data in ep.eventPoolData)
        {
            eventPoolData.Add(new EventPoolData(data));
        }
    }

    public void Reset()
    {
        int index = 0;
        for (int i = 0; i < eventPoolData.Count; i++)
        {
            eventPoolData[i].weightIndex = index;
            index += eventPoolData[i].weight;
        }
    }

    //public void AddEventPoolData(EventPoolData epd)
    //{
    //    EventPoolData lastData = eventPoolData[eventPoolData.Count - 1];
    //    eventPoolData.Add(epd);
    //    epd.weightIndex = lastData.weightIndex + lastData.weight;
    //}

    public void AddEventPoolData(string id, int weight)
    {
        for (int i = 0; i < eventPoolData.Count; i++)
        {
            if (eventPoolData[i].eventId == id)
            {
                int index = eventPoolData[i].weightIndex;
                eventPoolData[i].weight += weight;
                for (int j = i; j < eventPoolData.Count; j++)
                {
                    eventPoolData[j].weightIndex = index;
                    index += eventPoolData[j].weight;
                }
                return;
            }
        }

        EventPoolData lastData = eventPoolData[eventPoolData.Count - 1];
        EventPoolData epd = new EventPoolData(id, weight);
        eventPoolData.Add(epd);
        epd.weightIndex = lastData.weightIndex + lastData.weight;

    }

    public void RemoveEventPoolData(string id)
    {
        for(int i = 0; i < eventPoolData.Count; i++)
        {
            if(eventPoolData[i].eventId == id)
            {
                int index = eventPoolData[i].weightIndex;
                eventPoolData.RemoveAt(i);
                for (int j = i; j < eventPoolData.Count; j++)
                {
                    eventPoolData[j].weightIndex = index;
                    index += eventPoolData[j].weight;
                }
                break;
            }
        }
    }

    public void ChangeWeight(string id,int weight)
    {
        for (int i = 0; i < eventPoolData.Count; i++)
        {
            if (eventPoolData[i].eventId == id)
            {
                int index = eventPoolData[i].weightIndex;
                eventPoolData[i].weight = weight;
                for (int j = i; j < eventPoolData.Count; j++)
                {
                    eventPoolData[j].weightIndex = index;
                    index += eventPoolData[j].weight;
                }
                break;
            }
        }
    }

    public EventPoolData GetEventPoolDataWithWeight(int weight)
    {
        EventPoolData lastData = eventPoolData[eventPoolData.Count - 1];
        weight %= lastData.weight + lastData.weightIndex;
        Debug.Log(weight < 10 ? 1 : weight < 20 ? 2 : 3);
        //二分查找
        int start = 0;
        int end = eventPoolData.Count - 1;
        int mid;
        while (start != end)
        {
            mid = (start + end) >> 1;
            if (eventPoolData[mid].weightIndex > weight)
            {
                end = mid;
                continue;

            }else if(eventPoolData[mid].weightIndex + eventPoolData[mid].weight > weight)
            {
                return eventPoolData[mid];
            }
            else
            {
                start = mid + 1;
            }

        }
        return eventPoolData[start];
    }
}
[Serializable]
public class EventPoolData
{
    public string eventId;
    public int weight;
    [HideInInspector] public int weightIndex;
    public EventData eventData;
    public EventPoolData(EventPoolData epd)
    {
        eventId = epd.eventId;
        weight = epd.weight;
        weightIndex = epd.weightIndex;
    }
    public EventPoolData(string eventId, int weight)
    {
        this.eventId = eventId;
        this.weight = weight;
    }
    public EventPoolData()
    {

    }
}

[Serializable]
public struct ResourceDetile
{
    public E_ResourceType resourceType;
    public float amount;
}
[Serializable]
public class ChoiceDetile
{
    public string name;
    public string eventId;
    public string trigerContent;
    public string LimitContent;
    public bool limit ;
    public float[] floats;
    public List<ResourceDetile> resources;
    public List<InventoryItem> itemDetails;

    public ChoiceDetile() { }
    public ChoiceDetile(ChoiceDetile choiceDetile)
    {
        name = choiceDetile.name;
        eventId = choiceDetile.eventId;
        trigerContent = choiceDetile.trigerContent;
        LimitContent = choiceDetile.LimitContent;
        limit = choiceDetile.limit;
        resources = new List<ResourceDetile>(choiceDetile.resources);
        itemDetails = new List<InventoryItem>(choiceDetile.itemDetails);
    }

    public static ChoiceDetile NULL()
    {
        return new ChoiceDetile()
        {
            name = "退出",
            eventId = "",
            trigerContent = "",
            LimitContent = "",
            resources = new List<ResourceDetile>(),
            itemDetails = new List<InventoryItem>()
        };
    }
    public void SetLimit(bool limit)
    {
        this.limit = limit;
    }
    public void SetName(string name)
    {
        this.name = name;
    }
    public void SetEventId(string eventId)
    {
        this.eventId = eventId;
    }
    public void SetTrigerContent(string trigerContent)
    {
        this.trigerContent = trigerContent;
    }
    public void SetLimitContent(string LimitContent)
    {
        this.LimitContent = LimitContent;
    }
}

public class EventMod
{
    public string name;
    public int modId;
    public List<EventData> eventDatas = new List<EventData>();
    public List<EventPool> eventPools = new List<EventPool>();
}

[Serializable]
public class EnemyData
{
    public int id;
    public string name;
    public GameObject obj;
}
[Serializable]
public class UIData
{
    public Dictionary<string, string> value;
}
[Serializable]
public class SettingData
{
    public string Language;
}
[System.Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    public string itemDescription;
    public int itemUseRadius;
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    public int itemPrice;
    [Range(0, 1)]
    public float sellPercentage;
    public int maxStack;
}
[System.Serializable]

public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
    public ItemDetails itemDetails;
    public InventoryItem(int id,int amount) 
    {
        itemID = id;
        itemAmount = amount;
        itemDetails = null;
    }
}