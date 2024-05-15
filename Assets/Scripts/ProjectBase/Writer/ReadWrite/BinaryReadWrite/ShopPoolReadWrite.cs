using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ShopPoolReadWrite
	{
		public static void WriteShopPool(this Writer writer, ShopPool value)
		{
			writer.WriteDictionary(value.pools);

		}
		public static ShopPool ReadShopPool(this Reader reader)
		{
			ShopPool value = new ShopPool();
			value.pools = reader.ReadDictionary<System.String,ItemPool>();

			return value;
		}
	}
}
