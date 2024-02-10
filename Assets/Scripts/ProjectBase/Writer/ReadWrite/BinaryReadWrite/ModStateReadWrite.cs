using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ModStateReadWrite
	{
		public static void WriteModState(this Writer writer, ModState value)
		{
			writer.Write(value.name);
			writer.Write(value.description);
			writer.Write(value.modID);
			writer.Write(value.isActive);
		}
		public static ModState ReadModState(this Reader reader)
		{
			ModState value = new ModState();
			value.name = reader.ReadString();
			value.description = reader.ReadString();
			value.modID = reader.ReadString();
			value.isActive = reader.ReadBool();
			return value;
		}
	}
}