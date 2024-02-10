using System;
using System.Collections.Generic;
using System.Text;
using BinaryReadWrite;
namespace BinaryReadWrite
{
	public static class WriterReadWrite
	{
		public static void WriteWriter(this Writer writer, Writer value)
		{
			writer.Write((short)value.buffer.Length);
			for (int i = 0; i < value.buffer.Length; ++i)
				writer.Write(value.buffer[i]);
			writer.Write(value.Position);
		}
		public static Writer ReadWriter(this Reader reader)
		{
			Writer value = new Writer();
			short bufferLength = reader.ReadShort();
			value.buffer = new byte[bufferLength];
			for (int i = 0; i < value.buffer.Length; ++i)
				value.buffer[i] = reader.ReadByte();
			value.Position = reader.ReadInt();
			return value;
		}
	}
}