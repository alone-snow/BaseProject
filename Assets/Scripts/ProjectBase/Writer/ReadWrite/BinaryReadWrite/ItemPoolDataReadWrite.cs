using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ItemPoolDataReadWrite
	{
		public static void WriteItemPoolData(this Writer writer, ItemPoolData value)
		{
			writer.Write(value.item);
			writer.Write(value.weight);
			writer.Write(value.weightIndex);

		}
		public static ItemPoolData ReadItemPoolData(this Reader reader)
		{
			ItemPoolData value = new ItemPoolData();
			value.item = reader.ReadInventoryItem();
			value.weight = reader.ReadInt();
			value.weightIndex = reader.ReadInt();

			return value;
		}
	}
}
