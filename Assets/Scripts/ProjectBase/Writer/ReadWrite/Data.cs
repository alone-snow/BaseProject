using BinaryReadWrite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
[AutoWrite]
public class UserData
{
    public string account;
    public string password;
    public string language = "Chinese";

    public long playerId = -1;

    public float volume = 0.3f;
    public float SoundValue = 0.3f;
}
[AutoWrite]
public class PlayerData
{
    public long id;
    public string name;
}
public class ABCPlayer : PlayerData
{
    public int abc;
}

public class BCDPlayer : PlayerData
{
    public int bcd;
}

[Serializable]
[AutoWrite]
public struct ResourceDetile
{
    public E_ResourceType resourceType;
    public float amount;

    public ResourceDetile(E_ResourceType resourceType, float amount)
    {
        this.resourceType = resourceType;
        this.amount = amount;
    }
    public ResourceDetile(int resourceType, int amount)
    {
        this.resourceType = (E_ResourceType)resourceType;
        this.amount = amount;
    }
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
    public string itemDescription;
    public int itemPrice;
    [Range(0, 1)]
    public float sellPercentage;
    public int maxStack;
}
[System.Serializable]
[AutoWrite]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
    [NotWrite]
    public ItemDetails itemDetails;

    public InventoryItem(int id,int amount) 
    {
        itemID = id;
        itemAmount = amount;
        itemDetails = null;
    }
}
[System.Serializable]
[AutoWrite]
public class ShopPool
{
    public Dictionary<string, ItemPool> pools = new Dictionary<string, ItemPool>();
}
public struct ShopItem
{
    public InventoryItem item;
    public SalesType type;
    public int price;
    public ShopItem(int id,int amount,SalesType type,int price)
    {
        item = new InventoryItem(id,amount);
        this.type = type;
        this.price = price;
    }
    public ShopItem(InventoryItem item, SalesType type, int price)
    {
        this.item = item;
        this.type = type;
        this.price = price;
    }
}
public enum SalesType
{
    Gold, Gem
}
[System.Serializable]
[AutoWrite]
public class ItemPool : ICloneable
{
    public string poolName;
    public List<ItemPoolData> itemPoolData = new List<ItemPoolData>();

    public ItemPool() { }
    public ItemPool(string poolName)
    {
        this.poolName = poolName;
    }

    public void Reset()
    {
        int index = 0;
        for (int i = 0; i < itemPoolData.Count; i++)
        {
            itemPoolData[i].weightIndex = index;
            index += itemPoolData[i].weight;
        }
    }

    public void AddEventPoolData(int id,int amount, int weight)
    {
        for (int i = 0; i < itemPoolData.Count; i++)
        {
            if (itemPoolData[i].item.itemID == id && itemPoolData[i].item.itemAmount == amount)
            {
                int index = itemPoolData[i].weightIndex;
                itemPoolData[i].weight += weight;
                for (int j = i; j < itemPoolData.Count; j++)
                {
                    itemPoolData[j].weightIndex = index;
                    index += itemPoolData[j].weight;
                }
                return;
            }
        }

        ItemPoolData lastData = itemPoolData[itemPoolData.Count - 1];
        ItemPoolData epd = new ItemPoolData(id, amount, weight);
        itemPoolData.Add(epd);
        epd.weightIndex = lastData.weightIndex + lastData.weight;

    }

    public void RemoveEventPoolData(int id)
    {
        for (int i = 0; i < itemPoolData.Count; i++)
        {
            if (itemPoolData[i].item.itemID == id)
            {
                int index = itemPoolData[i].weightIndex;
                itemPoolData.RemoveAt(i);
                for (int j = i; j < itemPoolData.Count; j++)
                {
                    itemPoolData[j].weightIndex = index;
                    index += itemPoolData[j].weight;
                }
                break;
            }
        }
    }

    public void ChangeWeight(int id, int weight)
    {
        for (int i = 0; i < itemPoolData.Count; i++)
        {
            if (itemPoolData[i].item.itemID == id)
            {
                int index = itemPoolData[i].weightIndex;
                itemPoolData[i].weight = weight;
                for (int j = i; j < itemPoolData.Count; j++)
                {
                    itemPoolData[j].weightIndex = index;
                    index += itemPoolData[j].weight;
                }
                break;
            }
        }
    }

    public ItemPoolData GetEventPoolDataWithWeight(int weight)
    {
        if (itemPoolData.Count == 0) return null;
        ItemPoolData lastData = itemPoolData[itemPoolData.Count - 1];
        weight %= lastData.weight + lastData.weightIndex;
        //¶þ·Ö²éÕÒ
        int start = 0;
        int end = itemPoolData.Count - 1;
        int mid;
        while (start != end)
        {
            mid = (start + end) >> 1;
            if (itemPoolData[mid].weightIndex > weight)
            {
                end = mid;
                continue;

            }
            else if (itemPoolData[mid].weightIndex + itemPoolData[mid].weight > weight)
            {
                return itemPoolData[mid];
            }
            else
            {
                start = mid + 1;
            }

        }
        return itemPoolData[start];
    }

    public object Clone()
    {
        return Utilities.Clone(this);
    }
}
[System.Serializable]
[AutoWrite]
public class ItemPoolData : ICloneable
{
    public InventoryItem item;
    public int weight;
    [HideInInspector] public int weightIndex;
    public ItemPoolData() { }
    public ItemPoolData(int id, int amount,int weight) 
    {
        item = new InventoryItem(id, amount);
        this.weight = weight;
    }

    public object Clone()
    {
        return Utilities.Clone(this);
    }
}


