using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class EventPoolDataReadWrite
	{
		public static void WriteEventPoolData(this Writer writer, EventPoolData value)
		{
			writer.Write(value.eventId);
			writer.Write(value.weight);
			writer.Write(value.weightIndex);
		}
		public static EventPoolData ReadEventPoolData(this Reader reader)
		{
			EventPoolData value = new EventPoolData();
			value.eventId = reader.ReadString();
			value.weight = reader.ReadInt();
			value.weightIndex = reader.ReadInt();
			return value;
		}
	}
}