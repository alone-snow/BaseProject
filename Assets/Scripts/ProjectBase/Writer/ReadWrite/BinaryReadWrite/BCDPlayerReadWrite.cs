using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class BCDPlayerReadWrite
	{
		public static void WriteBCDPlayer(this Writer writer, BCDPlayer value)
		{
			writer.Write(value.bcd);
			writer.Write(value.id);
			writer.Write(value.name);

		}
		public static BCDPlayer ReadBCDPlayer(this Reader reader)
		{
			BCDPlayer value = new BCDPlayer();
			value.bcd = reader.ReadInt();
			value.id = reader.ReadLong();
			value.name = reader.ReadString();

			return value;
		}
	}
}
