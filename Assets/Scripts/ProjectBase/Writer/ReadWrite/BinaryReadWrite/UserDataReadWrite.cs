using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class UserDataReadWrite
	{
		public static void WriteUserData(this Writer writer, UserData value)
		{
			writer.Write(value.account);
			writer.Write(value.password);
			writer.Write(value.language);
			writer.Write((short)value.mods.Count);
			for (int i = 0; i < value.mods.Count; ++i)
				writer.WriteModState(value.mods[i]);
			writer.Write(value.volume);
			writer.Write(value.SoundValue);
		}
		public static UserData ReadUserData(this Reader reader)
		{
			UserData value = new UserData();
			value.account = reader.ReadString();
			value.password = reader.ReadString();
			value.language = reader.ReadString();
			value.mods = new List<ModState>();
			short modsCount = reader.ReadShort();
			for (int i = 0; i < modsCount; ++i)
				value.mods.Add(reader.ReadModState());
			value.volume = reader.ReadFloat();
			value.SoundValue = reader.ReadFloat();
			return value;
		}
	}
}