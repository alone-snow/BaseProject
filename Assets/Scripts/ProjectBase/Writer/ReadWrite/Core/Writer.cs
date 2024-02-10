using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace BinaryReadWrite
{
    public class Writer
    {
        public const ushort MaxStringLength = ushort.MaxValue - 1;

        public const int DefaultCapacity = 1500;
        public byte[] buffer = new byte[DefaultCapacity];

        /// <summary>Next position to write to the buffer</summary>
        public int Position;

        /// <summary>Current capacity. Automatically resized if necessary.</summary>
        public int Capacity => buffer.Length;

        internal readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        /// <summary>Reset both the position and length of the stream</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureCapacity(int value)
        {
            if (buffer.Length < value)
            {
                int capacity = Math.Max(value, buffer.Length * 2);
                Array.Resize(ref buffer, capacity);
            }
        }
        /// <summary>Copies buffer until 'Position' to a new array.</summary>
        public byte[] ToArray()
        {
            byte[] data = new byte[Position];
            Array.ConstrainedCopy(buffer, 0, data, 0, Position);
            return data;
        }
        /// <summary>Returns allocation-free ArraySegment until 'Position'.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<byte> ToArraySegment() =>
            new ArraySegment<byte>(buffer, 0, Position);

        // implicit conversion for convenience
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<byte>(Writer w) =>
            w.ToArraySegment();

        internal unsafe void WriteBlittable<T>(T value) where T : unmanaged
        {
            int size = sizeof(T);

            EnsureCapacity(Position + size);

            // write blittable
            fixed (byte* ptr = &buffer[Position])
            {
#if UNITY_ANDROID
                T* valueBuffer = stackalloc T[1]{value};
                //UnsafeUtility.MemCpy(ptr, valueBuffer, size);
                Utility.MemCpy(ptr, valueBuffer, size);
#else
                *(T*)ptr = value;
#endif
            }
            Position += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteBlittableNullable<T>(T? value) where T : unmanaged
        {
            WriteByte((byte)(value.HasValue ? 0x01 : 0x00));

            if (value.HasValue)
                WriteBlittable(value.Value);
        }

        public void WriteByte(byte value) => WriteBlittable(value);

        public void WriteBytes(byte[] array, int offset, int count)
        {
            EnsureCapacity(Position + count);
            Array.ConstrainedCopy(array, offset, this.buffer, Position, count);
            Position += count;
        }

        public unsafe bool WriteBytes(byte* ptr, int offset, int size)
        {
            EnsureCapacity(Position + size);

            fixed (byte* destination = &buffer[Position])
            {
                // 10 mio writes: 868ms
                //   Array.Copy(value.Array, value.Offset, buffer, Position, value.Count);
                // 10 mio writes: 775ms
                //   Buffer.BlockCopy(destination, offset, buffer, Position, size);
                // 10 mio writes: 637ms
                // UnsafeUtility.MemCpy(destination, ptr + offset, size);
                Utility.MemCpy(destination, ptr + offset, size);
            }

            Position += size;
            return true;
        }

        public void Write<T>(T value)
        {
            Action<Writer, T> writeDelegate = Writer<T>.write;
            if (writeDelegate == null)
            {
                Debug.LogError($"No writer found for {typeof(T)}. This happens either if you are missing a NetworkWriter extension for your custom type, or if weaving failed. Try to reimport a script to weave again.");
            }
            else
            {
                writeDelegate(this, value);
            }
        }
        public override string ToString() =>
            $"[{ToArraySegment().ToHexString()} @ {Position}/{Capacity}]";
    }

    public static class Writer<T>
    {
        public static Action<Writer, T> write;
    }
}


