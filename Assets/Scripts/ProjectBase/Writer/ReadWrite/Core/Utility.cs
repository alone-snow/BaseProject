using System;

namespace BinaryReadWrite
{
    public static class Utility
    {
        public unsafe static void MemCpy(byte* destination, byte* source, long size)
        {
            for (int i = 0; i < size; i++)
            {
                *destination = *source;
                destination++;
                source++;
            }
        }
        public static string ToHexString(this ArraySegment<byte> segment) =>
        BitConverter.ToString(segment.Array, segment.Offset, segment.Count);
    }
}

