using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace ExcelTool
{
	public class ItemData : AutoData<ItemData>, ICloneable
	{
		public int id;
		public string name;
		public int stack;
		public int price;
		public List<int> mod;
		public int[] attribute;
		public Dictionary<string,int> state;
		public static string Path = "Game/ItemData/ItemData.data";
		public object Clone()
		{
			return Utilities.Clone(this);
		}
		public T Clone<T>()
		{
			return (T)Utilities.Clone(this);
		}
	}
	public static class ItemDataReadWrite
	{
		public static void WriteItemData(this Writer writer, ItemData value)
		{
			writer.Write(value.id);
			writer.Write(value.name);
			writer.Write(value.stack);
			writer.Write(value.price);
			writer.Write(value.mod);
			writer.Write(value.attribute);
			writer.Write(value.state);
		}
		public static ItemData ReadItemData(this Reader reader)
		{
			ItemData value = new ItemData();
			value.id = reader.ReadInt();
			value.name = reader.ReadString();
			value.stack = reader.ReadInt();
			value.price = reader.ReadInt();
			value.mod = reader.ReadList<int>();
			value.attribute = reader.ReadArray<int>();
			value.state = reader.ReadDictionary<string,int>();
			return value;
		}
	}
}
