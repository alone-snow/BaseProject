using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ABCPlayerReadWrite
	{
		public static void WriteABCPlayer(this Writer writer, ABCPlayer value)
		{
			writer.Write(value.abc);
			writer.Write(value.id);
			writer.Write(value.name);

		}
		public static ABCPlayer ReadABCPlayer(this Reader reader)
		{
			ABCPlayer value = new ABCPlayer();
			value.abc = reader.ReadInt();
			value.id = reader.ReadLong();
			value.name = reader.ReadString();

			return value;
		}
	}
}
