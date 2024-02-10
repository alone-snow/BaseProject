using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public class AccountData
	{
		public string account;
		public string password;
		public PlayerData playerData;
	}

	public static class AccountDataReadWrite
	{
		public static void WriteAccountData(this Writer writer, AccountData value)
		{
			writer.Write(value.account);
			writer.Write(value.password);
			writer.WritePlayerData(value.playerData);
		}
		public static AccountData ReadAccountData(this Reader reader)
		{
			AccountData value = new AccountData();
			value.account = reader.ReadString();
			value.password = reader.ReadString();
			value.playerData = reader.ReadPlayerData();
			return value;
		}
	}
}