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
			if(value is ABCPlayer childABCPlayer)
			{
				writer.Write(1054794776);
				writer.Write(childABCPlayer);
				return;
			}
			if(value is BCDPlayer childBCDPlayer)
			{
				writer.Write(1054795152);
				writer.Write(childBCDPlayer);
				return;
			}
			writer.Write(0);
			writer.Write(value.id);
			writer.Write(value.name);

		}
		public static PlayerData ReadPlayerData(this Reader reader)
		{
			switch (reader.ReadInt())
			{
				case 1054794776:
					return reader.ReadABCPlayer();
				case 1054795152:
					return reader.ReadBCDPlayer();
			}
			PlayerData value = new PlayerData();
			value.id = reader.ReadLong();
			value.name = reader.ReadString();

			return value;
		}
	}
}
