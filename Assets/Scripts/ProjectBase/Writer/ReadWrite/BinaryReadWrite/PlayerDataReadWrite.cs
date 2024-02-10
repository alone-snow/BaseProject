using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class PlayerDataReadWrite
	{
		public static void WritePlayerData(this Writer writer, PlayerData value)
		{
			writer.Write(value.id);
			writer.Write(value.name);
			writer.Write((short)value.numValue.Count);
			for (int i = 0; i < value.numValue.Count; ++i)
				writer.Write(value.numValue[i]);
			writer.Write(value.searchStrength);
			writer.Write(value.maxSearchAmount);
			writer.Write(value.searchCost);
			writer.Write(value.jumpCost);
			writer.Write(value.transitionCost);
			writer.Write((short)value.eventDatas.Count);
			for (int i = 0; i < value.eventDatas.Count; ++i)
				writer.WriteEventData(value.eventDatas[i]);
			writer.Write((short)value.pools.Count);
			foreach (string key in value.pools.Keys)
			{
				writer.Write(key);
				writer.WriteEventPool(value.pools[key]);
			}
			writer.Write((short)value.box.Count);
			for (int i = 0; i < value.box.Count; ++i)
				writer.WriteInventoryItem(value.box[i]);
			writer.Write((short)value.eventDataList.Count);
			for (int i = 0; i < value.eventDataList.Count; ++i)
				writer.WriteEventData(value.eventDataList[i]);
			writer.WriteEventData(value.activeEvent);
			writer.Write(value.ifHaveActiveEvent);
			writer.Write((short)value.state.Count);
			foreach (string key in value.state.Keys)
			{
				writer.Write(key);
				writer.Write(value.state[key]);
			}
		}
		public static PlayerData ReadPlayerData(this Reader reader)
		{
			PlayerData value = new PlayerData();
			value.id = reader.ReadLong();
			value.name = reader.ReadString();
			value.numValue = new List<float>();
			short numValueCount = reader.ReadShort();
			for (int i = 0; i < numValueCount; ++i)
				value.numValue.Add(reader.ReadFloat());
			value.searchStrength = reader.ReadFloat();
			value.maxSearchAmount = reader.ReadFloat();
			value.searchCost = reader.ReadFloat();
			value.jumpCost = reader.ReadFloat();
			value.transitionCost = reader.ReadFloat();
			value.eventDatas = new List<EventData>();
			short eventDatasCount = reader.ReadShort();
			for (int i = 0; i < eventDatasCount; ++i)
				value.eventDatas.Add(reader.ReadEventData());
			value.pools = new Dictionary<string, EventPool>();
			short poolsCount = reader.ReadShort();
			for (int i = 0; i < poolsCount; ++i)
				value.pools.Add(reader.ReadString(), reader.ReadEventPool());
			value.box = new List<InventoryItem>();
			short boxCount = reader.ReadShort();
			for (int i = 0; i < boxCount; ++i)
				value.box.Add(reader.ReadInventoryItem());
			value.eventDataList = new List<EventData>();
			short eventDataListCount = reader.ReadShort();
			for (int i = 0; i < eventDataListCount; ++i)
				value.eventDataList.Add(reader.ReadEventData());
			value.activeEvent = reader.ReadEventData();
			value.ifHaveActiveEvent = reader.ReadBool();
			value.state = new Dictionary<string, float>();
			short stateCount = reader.ReadShort();
			for (int i = 0; i < stateCount; ++i)
				value.state.Add(reader.ReadString(), reader.ReadFloat());
			return value;
		}
	}
}