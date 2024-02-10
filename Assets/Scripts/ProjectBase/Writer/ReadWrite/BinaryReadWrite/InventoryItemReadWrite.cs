using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class InventoryItemReadWrite
	{
		public static void WriteInventoryItem(this Writer writer, InventoryItem value)
		{
			writer.Write(value.itemID);
			writer.Write(value.itemAmount);
		}
		public static InventoryItem ReadInventoryItem(this Reader reader)
		{
			InventoryItem value = new InventoryItem();
			value.itemID = reader.ReadInt();
			value.itemAmount = reader.ReadInt();
			return value;
		}
	}
}