using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class ResourceDetileReadWrite
	{
		public static void WriteResourceDetile(this Writer writer, ResourceDetile value)
		{
			writer.Write((int)value.resourceType);
			writer.Write(value.amount);

		}
		public static ResourceDetile ReadResourceDetile(this Reader reader)
		{
			ResourceDetile value = new ResourceDetile();
			value.resourceType= (E_ResourceType)reader.ReadInt();
			value.amount = reader.ReadFloat();

			return value;
		}
	}
}
