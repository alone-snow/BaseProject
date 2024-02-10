using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class EventModReadWrite
	{
		public static void WriteEventMod(this Writer writer, EventMod value)
		{
			writer.Write(value.modId);
			writer.Write(value.name);
			writer.Write((short)value.eventDatas.Count);
			for (int i = 0; i < value.eventDatas.Count; ++i)
				writer.WriteEventData(value.eventDatas[i]);
			writer.Write((short)value.eventPools.Count);
			for (int i = 0; i < value.eventPools.Count; ++i)
				writer.WriteEventPool(value.eventPools[i]);
		}
		public static EventMod ReadEventMod(this Reader reader)
		{
			EventMod value = new EventMod();
			value.modId = reader.ReadInt();
			value.name = reader.ReadString();
			value.eventDatas = new List<EventData>();
			short eventDatasCount = reader.ReadShort();
			for (int i = 0; i < eventDatasCount; ++i)
				value.eventDatas.Add(reader.ReadEventData());
			value.eventPools = new List<EventPool>();
			short eventPoolsCount = reader.ReadShort();
			for (int i = 0; i < eventPoolsCount; ++i)
				value.eventPools.Add(reader.ReadEventPool());
			return value;
		}
	}
}