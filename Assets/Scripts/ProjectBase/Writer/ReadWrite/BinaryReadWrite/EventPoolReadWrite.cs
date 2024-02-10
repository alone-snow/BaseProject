using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class EventPoolReadWrite
	{
		public static void WriteEventPool(this Writer writer, EventPool value)
		{
			writer.Write(value.poolName);
			writer.Write((short)value.eventPoolData.Count);
			for (int i = 0; i < value.eventPoolData.Count; ++i)
				writer.WriteEventPoolData(value.eventPoolData[i]);
		}
		public static EventPool ReadEventPool(this Reader reader)
		{
			EventPool value = new EventPool();
			value.poolName = reader.ReadString();
			value.eventPoolData = new List<EventPoolData>();
			short eventPoolDataCount = reader.ReadShort();
			for (int i = 0; i < eventPoolDataCount; ++i)
				value.eventPoolData.Add(reader.ReadEventPoolData());
			return value;
		}
	}
}