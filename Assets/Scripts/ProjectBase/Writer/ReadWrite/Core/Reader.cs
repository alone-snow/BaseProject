using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BinaryReadWrite
{
    public class Reader
    {
        public ArraySegment<byte> buffer;

        public int Position;

        /// <summary>Remaining bytes that can be read, for convenience.</summary>
        public int Remaining => buffer.Count - Position;

        /// <summary>Total buffer capacity, independent of reader position.</summary>
        public int Capacity => buffer.Count;

        internal readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        public const int AllocationLimit = 1024 * 1024 * 16; // 16 MB * sizeof(T)

        public Reader(ArraySegment<byte> segment)
        {
            buffer = segment;
        }

#if !UNITY_2021_3_OR_NEWER
        // Unity 2019 doesn't have the implicit byte[] to segment conversion yet
        public Reader(byte[] bytes)
        {
            buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBuffer(ArraySegment<byte> segment)
        {
            buffer = segment;
            Position = 0;
        }

#if !UNITY_2021_3_OR_NEWER
        // Unity 2019 doesn't have the implicit byte[] to segment conversion yet
        public void SetBuffer(byte[] bytes)
        {
            buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
            Position = 0;
        }
#endif
        internal unsafe T ReadBlittable<T>() where T : unmanaged
        {
            int size = sizeof(T);

            if (Remaining < size)
            {
                throw new EndOfStreamException($"ReadBlittable<{typeof(T)}> not enough data in buffer to read {size} bytes: {ToString()}");
            }

            T value;
            fixed (byte* ptr = &buffer.Array[buffer.Offset + Position])
            {
#if UNITY_ANDROID
                T* valueBuffer = stackalloc T[1];
                //UnsafeUtility.MemCpy(valueBuffer, ptr, size);
                Utility.MemCpy(valueBuffer, ptr, size);
                value = valueBuffer[0];
#else
                value = *(T*)ptr;
#endif
            }
            Position += size;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T? ReadBlittableNullable<T>()
            where T : unmanaged =>
            ReadByte() != 0 ? ReadBlittable<T>() : default(T?);

        public byte ReadByte() => ReadBlittable<byte>();

        /// <summary>Read 'count' bytes into the bytes array</summary>
        public byte[] ReadBytes(byte[] bytes, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("ReadBytes requires count >= 0");

            if (count > bytes.Length)
            {
                throw new EndOfStreamException($"ReadBytes can't read {count} + bytes because the passed byte[] only has length {bytes.Length}");
            }
            if (Remaining < count)
            {
                throw new EndOfStreamException($"ReadBytesSegment can't read {count} bytes because it would read past the end of the stream. {ToString()}");
            }

            Array.Copy(buffer.Array, buffer.Offset + Position, bytes, 0, count);
            Position += count;
            return bytes;
        }

        /// <summary>Read 'count' bytes allocation-free as ArraySegment that points to the internal array.</summary>
        public ArraySegment<byte> ReadBytesSegment(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("ReadBytesSegment requires count >= 0");

            if (Remaining < count)
            {
                throw new EndOfStreamException($"ReadBytesSegment can't read {count} bytes because it would read past the end of the stream. {ToString()}");
            }

            ArraySegment<byte> result = new ArraySegment<byte>(buffer.Array, buffer.Offset + Position, count);
            Position += count;
            return result;
        }

        /// <summary>Reads any data type that mirror supports. Uses weaver populated Reader(T).read</summary>
        public T Read<T>()
        {
            Func<Reader, T> readerDelegate = Reader<T>.read;
            if (readerDelegate == null)
            {
                //Debug.LogError($"No reader found for {typeof(T)}. Use a type supported by Mirror or define a custom reader extension for {typeof(T)}.");
                return default;
            }
            return readerDelegate(this);
        }

        public override string ToString() =>
            $"[{buffer.ToHexString()} @ {Position}/{Capacity}]";
    }

    public static class Reader<T>
    {
        public static Func<Reader, T> read;
    }
}


