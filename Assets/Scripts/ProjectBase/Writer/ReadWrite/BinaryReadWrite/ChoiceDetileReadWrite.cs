using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ChoiceDetileReadWrite
	{
		public static void WriteChoiceDetile(this Writer writer, ChoiceDetile value)
		{
			writer.Write(value.name);
			writer.Write(value.eventId);
			writer.Write(value.trigerContent);
			writer.Write(value.LimitContent);
			writer.Write((short)value.floats.Length);
			for (int i = 0; i < value.floats.Length; ++i)
				writer.Write(value.floats[i]);
			writer.Write((short)value.resources.Count);
			for (int i = 0; i < value.resources.Count; ++i)
				writer.WriteResourceDetile(value.resources[i]);
			writer.Write((short)value.itemDetails.Count);
			for (int i = 0; i < value.itemDetails.Count; ++i)
				writer.WriteInventoryItem(value.itemDetails[i]);
		}
		public static ChoiceDetile ReadChoiceDetile(this Reader reader)
		{
			ChoiceDetile value = new ChoiceDetile();
			value.name = reader.ReadString();
			value.eventId = reader.ReadString();
			value.trigerContent = reader.ReadString();
			value.LimitContent = reader.ReadString();
			short floatsLength = reader.ReadShort();
			value.floats = new float[floatsLength];
			for (int i = 0; i < value.floats.Length; ++i)
				value.floats[i] = reader.ReadFloat();
			value.resources = new List<ResourceDetile>();
			short resourcesCount = reader.ReadShort();
			for (int i = 0; i < resourcesCount; ++i)
				value.resources.Add(reader.ReadResourceDetile());
			value.itemDetails = new List<InventoryItem>();
			short itemDetailsCount = reader.ReadShort();
			for (int i = 0; i < itemDetailsCount; ++i)
				value.itemDetails.Add(reader.ReadInventoryItem());
			return value;
		}
	}
}