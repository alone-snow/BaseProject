using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ItemPoolReadWrite
	{
		public static void WriteItemPool(this Writer writer, ItemPool value)
		{
			writer.Write(value.poolName);
			writer.WriteList(value.itemPoolData);

		}
		public static ItemPool ReadItemPool(this Reader reader)
		{
			ItemPool value = new ItemPool();
			value.poolName = reader.ReadString();
			value.itemPoolData = reader.ReadList<ItemPoolData>();

			return value;
		}
	}
}
