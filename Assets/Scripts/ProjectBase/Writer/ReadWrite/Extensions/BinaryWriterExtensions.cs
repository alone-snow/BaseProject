#define OnUnity
using System;
using System.Collections.Generic;
#if OnUnity
using UnityEngine;
#endif

namespace BinaryReadWrite
{
    public static class BinaryWriterExtensions
    {
        public static void WriteByte(this Writer writer, byte value) => writer.WriteBlittable(value);
        public static void WriteByteNullable(this Writer writer, byte? value) => writer.WriteBlittableNullable(value);

        public static void WriteSByte(this Writer writer, sbyte value) => writer.WriteBlittable(value);
        public static void WriteSByteNullable(this Writer writer, sbyte? value) => writer.WriteBlittableNullable(value);

        // char is not blittable. convert to ushort.
        public static void WriteChar(this Writer writer, char value) => writer.WriteBlittable((ushort)value);
        public static void WriteCharNullable(this Writer writer, char? value) => writer.WriteBlittableNullable((ushort?)value);

        // bool is not blittable. convert to byte.
        public static void WriteBool(this Writer writer, bool value) => writer.WriteBlittable((byte)(value ? 1 : 0));
        public static void WriteBoolNullable(this Writer writer, bool? value) => writer.WriteBlittableNullable(value.HasValue ? ((byte)(value.Value ? 1 : 0)) : new byte?());

        public static void WriteShort(this Writer writer, short value) => writer.WriteBlittable(value);
        public static void WriteShortNullable(this Writer writer, short? value) => writer.WriteBlittableNullable(value);

        public static void WriteUShort(this Writer writer, ushort value) => writer.WriteBlittable(value);
        public static void WriteUShortNullable(this Writer writer, ushort? value) => writer.WriteBlittableNullable(value);

        public static void WriteInt(this Writer writer, int value) => writer.WriteBlittable(value);
        public static void WriteIntNullable(this Writer writer, int? value) => writer.WriteBlittableNullable(value);

        public static void WriteUInt(this Writer writer, uint value) => writer.WriteBlittable(value);
        public static void WriteUIntNullable(this Writer writer, uint? value) => writer.WriteBlittableNullable(value);

        public static void WriteLong(this Writer writer, long value) => writer.WriteBlittable(value);
        public static void WriteLongNullable(this Writer writer, long? value) => writer.WriteBlittableNullable(value);

        public static void WriteULong(this Writer writer, ulong value) => writer.WriteBlittable(value);
        public static void WriteULongNullable(this Writer writer, ulong? value) => writer.WriteBlittableNullable(value);

        public static void WriteFloat(this Writer writer, float value) => writer.WriteBlittable(value);
        public static void WriteFloatNullable(this Writer writer, float? value) => writer.WriteBlittableNullable(value);

        public static void WriteDouble(this Writer writer, double value) => writer.WriteBlittable(value);
        public static void WriteDoubleNullable(this Writer writer, double? value) => writer.WriteBlittableNullable(value);

        public static void WriteDecimal(this Writer writer, decimal value) => writer.WriteBlittable(value);
        public static void WriteDecimalNullable(this Writer writer, decimal? value) => writer.WriteBlittableNullable(value);

        public static void WriteString(this Writer writer, string value)
        {
            // write 0 for null support, increment real size by 1
            // (note: original HLAPI would write "" for null strings, but if a
            //        string is null on the server then it should also be null
            //        on the client)
            if (value == null)
            {
                writer.WriteUShort(0);
                return;
            }

            // WriteString copies into the buffer manually.
            // need to ensure capacity here first, manually.
            int maxSize = writer.encoding.GetMaxByteCount(value.Length);
            writer.EnsureCapacity(writer.Position + 2 + maxSize); // 2 bytes position + N bytes encoding

            // encode it into the buffer first.
            // reserve 2 bytes for header after we know how much was written.
            int written = writer.encoding.GetBytes(value, 0, value.Length, writer.buffer, writer.Position + 2);

            // check if within max size, otherwise Reader can't read it.
            if (written > Writer.MaxStringLength)
                throw new IndexOutOfRangeException($"NetworkWriter.WriteString - Value too long: {written} bytes. Limit: {Writer.MaxStringLength} bytes");

            // .Position is unchanged, so fill in the size header now.
            // we already ensured that max size fits into ushort.max-1.
            writer.WriteUShort(checked((ushort)(written + 1))); // Position += 2

            // now update position by what was written above
            writer.Position += written;
        }

        public static void WriteBytesAndSizeSegment(this Writer writer, ArraySegment<byte> buffer)
        {
            writer.WriteBytesAndSize(buffer.Array, buffer.Offset, buffer.Count);
        }

        // Weaver needs a write function with just one byte[] parameter
        // (we don't name it .Write(byte[]) because it's really a WriteBytesAndSize since we write size / null info too)

        public static void WriteBytesAndSize(this Writer writer, byte[] buffer)
        {
            // buffer might be null, so we can't use .Length in that case
            writer.WriteBytesAndSize(buffer, 0, buffer != null ? buffer.Length : 0);
        }

        // for byte arrays with dynamic size, where the reader doesn't know how many will come
        // (like an inventory with different items etc.)

        public static void WriteBytesAndSize(this Writer writer, byte[] buffer, int offset, int count)
        {
            // null is supported because [SyncVar]s might be structs with null byte[] arrays
            // write 0 for null array, increment normal size by 1 to save bandwidth
            // (using size=-1 for null would limit max size to 32kb instead of 64kb)
            if (buffer == null)
            {
                writer.WriteUInt(0u);
                return;
            }
            writer.WriteUInt(checked((uint)count) + 1u);
            writer.WriteBytes(buffer, offset, count);
        }

        public static void WriteArraySegment<T>(this Writer writer, ArraySegment<T> segment)
        {
            int length = segment.Count;
            writer.WriteInt(length);
            for (int i = 0; i < length; i++)
            {
                writer.Write(segment.Array[segment.Offset + i]);
            }
        }
        // while SyncList<T> is recommended for NetworkBehaviours,
        // structs may have .List<T> members which weaver needs to be able to
        // fully serialize for NetworkMessages etc.
        // note that Weaver/Writers/GenerateWriter() handles this manually.
        public static void WriteList<T>(this Writer writer, List<T> list)
        {
            // 'null' is encoded as '-1'
            if (list is null)
            {
                writer.WriteInt(-1);
                return;
            }

            // check if within max size, otherwise Reader can't read it.
            if (list.Count > Reader.AllocationLimit)
                throw new IndexOutOfRangeException($"Writer.WriteList - List<{typeof(T)}> too big: {list.Count} elements. Limit: {Reader.AllocationLimit}");

            writer.WriteInt(list.Count);
            for (int i = 0; i < list.Count; i++)
                writer.Write(list[i]);
        }
        public static void WriteArray<T>(this Writer writer, T[] array)
        {
            // 'null' is encoded as '-1'
            if (array is null)
            {
                writer.WriteInt(-1);
                return;
            }

            // check if within max size, otherwise Reader can't read it.
            if (array.Length > Reader.AllocationLimit)
                throw new IndexOutOfRangeException($"Writer.WriteArray - Array<{typeof(T)}> too big: {array.Length} elements. Limit: {Reader.AllocationLimit}");

            writer.WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
                writer.Write(array[i]);
        }

        public static void WriteUri(this Writer writer, Uri uri)
        {
            writer.WriteString(uri?.ToString());
        }

        public static void WriteDateTime(this Writer writer, DateTime dateTime)
        {
            writer.WriteDouble(dateTime.ToOADate());
        }

        public static void WriteDateTimeNullable(this Writer writer, DateTime? dateTime)
        {
            writer.WriteBool(dateTime.HasValue);
            if (dateTime.HasValue)
                writer.WriteDouble(dateTime.Value.ToOADate());
        }
#if OnUnity
        public static void WriteVector2(this Writer writer, Vector2 value) => writer.WriteBlittable(value);
        public static void WriteVector2Nullable(this Writer writer, Vector2? value) => writer.WriteBlittableNullable(value);

        public static void WriteVector3(this Writer writer, Vector3 value) => writer.WriteBlittable(value);
        public static void WriteVector3Nullable(this Writer writer, Vector3? value) => writer.WriteBlittableNullable(value);

        public static void WriteVector4(this Writer writer, Vector4 value) => writer.WriteBlittable(value);
        public static void WriteVector4Nullable(this Writer writer, Vector4? value) => writer.WriteBlittableNullable(value);

        public static void WriteVector2Int(this Writer writer, Vector2Int value) => writer.WriteBlittable(value);
        public static void WriteVector2IntNullable(this Writer writer, Vector2Int? value) => writer.WriteBlittableNullable(value);

        public static void WriteVector3Int(this Writer writer, Vector3Int value) => writer.WriteBlittable(value);
        public static void WriteVector3IntNullable(this Writer writer, Vector3Int? value) => writer.WriteBlittableNullable(value);

        public static void WriteColor(this Writer writer, Color value) => writer.WriteBlittable(value);
        public static void WriteColorNullable(this Writer writer, Color? value) => writer.WriteBlittableNullable(value);

        public static void WriteColor32(this Writer writer, Color32 value) => writer.WriteBlittable(value);
        public static void WriteColor32Nullable(this Writer writer, Color32? value) => writer.WriteBlittableNullable(value);

        public static void WriteQuaternion(this Writer writer, Quaternion value) => writer.WriteBlittable(value);
        public static void WriteQuaternionNullable(this Writer writer, Quaternion? value) => writer.WriteBlittableNullable(value);

        // Rect is a struct with properties instead of fields
        public static void WriteRect(this Writer writer, Rect value)
        {
            writer.WriteVector2(value.position);
            writer.WriteVector2(value.size);
        }
        public static void WriteRectNullable(this Writer writer, Rect? value)
        {
            writer.WriteBool(value.HasValue);
            if (value.HasValue)
                writer.WriteRect(value.Value);
        }

        // Plane is a struct with properties instead of fields
        public static void WritePlane(this Writer writer, Plane value)
        {
            writer.WriteVector3(value.normal);
            writer.WriteFloat(value.distance);
        }
        public static void WritePlaneNullable(this Writer writer, Plane? value)
        {
            writer.WriteBool(value.HasValue);
            if (value.HasValue)
                writer.WritePlane(value.Value);
        }

        // Ray is a struct with properties instead of fields
        public static void WriteRay(this Writer writer, Ray value)
        {
            writer.WriteVector3(value.origin);
            writer.WriteVector3(value.direction);
        }
        public static void WriteRayNullable(this Writer writer, Ray? value)
        {
            writer.WriteBool(value.HasValue);
            if (value.HasValue)
                writer.WriteRay(value.Value);
        }

        public static void WriteMatrix4x4(this Writer writer, Matrix4x4 value) => writer.WriteBlittable(value);
        public static void WriteMatrix4x4Nullable(this Writer writer, Matrix4x4? value) => writer.WriteBlittableNullable(value);

        public static void WriteGuid(this Writer writer, Guid value)
        {
#if !UNITY_2021_3_OR_NEWER
            // Unity 2019 doesn't have Span yet
            byte[] data = value.ToByteArray();
            writer.WriteBytes(data, 0, data.Length);
#else
            // WriteBlittable(Guid) isn't safe. see WriteBlittable comments.
            // Guid is Sequential, but we can't guarantee packing.
            // TryWriteBytes is safe and allocation free.
            writer.EnsureCapacity(writer.Position + 16);
            value.TryWriteBytes(new Span<byte>(writer.buffer, writer.Position, 16));
            writer.Position += 16;
#endif
        }
        public static void WriteGuidNullable(this Writer writer, Guid? value)
        {
            writer.WriteBool(value.HasValue);
            if (value.HasValue)
                writer.WriteGuid(value.Value);
        }


        // while SyncSet<T> is recommended for NetworkBehaviours,
        // structs may have .Set<T> members which weaver needs to be able to
        // fully serialize for NetworkMessages etc.
        // note that Weaver/Writers/GenerateWriter() handles this manually.
        // TODO writer not found. need to adjust weaver first. see tests.
        /*
        public static void WriteHashSet<T>(this NetworkWriter writer, HashSet<T> hashSet)
        {
            if (hashSet is null)
            {
                writer.WriteInt(-1);
                return;
            }
            writer.WriteInt(hashSet.Count);
            foreach (T item in hashSet)
                writer.Write(item);
        }
        */



        public static void WriteTexture2D(this Writer writer, Texture2D texture2D)
        {
            // TODO allocation protection when sending textures to server.
            //      currently can allocate 32k x 32k x 4 byte = 3.8 GB

            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            // simply send -1 for width.
            if (texture2D == null)
            {
                writer.WriteShort(-1);
                return;
            }

            // check if within max size, otherwise Reader can't read it.
            int totalSize = texture2D.width * texture2D.height;
            if (totalSize > Reader.AllocationLimit)
                throw new IndexOutOfRangeException($"Writer.WriteTexture2D - Texture2D total size (width*height) too big: {totalSize}. Limit: {Reader.AllocationLimit}");

            // write dimensions first so reader can create the texture with size
            // 32k x 32k short is more than enough
            writer.WriteShort((short)texture2D.width);
            writer.WriteShort((short)texture2D.height);
            writer.WriteArray(texture2D.GetPixels32());
        }

        public static void WriteSprite(this Writer writer, Sprite sprite)
        {
            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            // simply send a 'null' for texture content.
            if (sprite == null)
            {
                writer.WriteTexture2D(null);
                return;
            }

            writer.WriteTexture2D(sprite.texture);
            writer.WriteRect(sprite.rect);
            writer.WriteVector2(sprite.pivot);
        }
#endif
    }
}