using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class EventDataReadWrite
	{
		public static void WriteEventData(this Writer writer, EventData value)
		{
			writer.Write(value.id);
			writer.Write(value.name);
			writer.Write(value.content);
			writer.Write((int)value.touchType);
			writer.Write((int)value.quality);
			writer.Write((short)value.resources.Count);
			for (int i = 0; i < value.resources.Count; ++i)
				writer.WriteResourceDetile(value.resources[i]);
			writer.Write((short)value.itemDetails.Count);
			for (int i = 0; i < value.itemDetails.Count; ++i)
				writer.WriteInventoryItem(value.itemDetails[i]);
			writer.Write((short)value.choiceDetile.Count);
			for (int i = 0; i < value.choiceDetile.Count; ++i)
				writer.WriteChoiceDetile(value.choiceDetile[i]);
			writer.Write((short)value.unlockEvent.Count);
			for (int i = 0; i < value.unlockEvent.Count; ++i)
				writer.Write(value.unlockEvent[i]);
		}
		public static EventData ReadEventData(this Reader reader)
		{
			EventData value = new EventData();
			value.id = reader.ReadString();
			value.name = reader.ReadString();
			value.content = reader.ReadString();
			value.touchType = (E_EventTouchType)reader.ReadInt();
			value.quality = (E_EventQuality)reader.ReadInt();
			value.resources = new List<ResourceDetile>();
			short resourcesCount = reader.ReadShort();
			for (int i = 0; i < resourcesCount; ++i)
				value.resources.Add(reader.ReadResourceDetile());
			value.itemDetails = new List<InventoryItem>();
			short itemDetailsCount = reader.ReadShort();
			for (int i = 0; i < itemDetailsCount; ++i)
				value.itemDetails.Add(reader.ReadInventoryItem());
			value.choiceDetile = new List<ChoiceDetile>();
			short choiceDetileCount = reader.ReadShort();
			for (int i = 0; i < choiceDetileCount; ++i)
				value.choiceDetile.Add(reader.ReadChoiceDetile());
			value.unlockEvent = new List<string>();
			short unlockEventCount = reader.ReadShort();
			for (int i = 0; i < unlockEventCount; ++i)
				value.unlockEvent.Add(reader.ReadString());
			return value;
		}
	}
}