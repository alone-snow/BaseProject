#define OnUnity
using System;
using System.Collections.Generic;
using System.IO;
#if OnUnity
using UnityEngine;
#endif

namespace BinaryReadWrite
{
    public static class BinaryReaderExtensions
    {
        public static byte ReadByte(this Reader reader) => reader.ReadBlittable<byte>();
        public static byte? ReadByteNullable(this Reader reader) => reader.ReadBlittableNullable<byte>();

        public static sbyte ReadSByte(this Reader reader) => reader.ReadBlittable<sbyte>();
        public static sbyte? ReadSByteNullable(this Reader reader) => reader.ReadBlittableNullable<sbyte>();

        // bool is not blittable. read as ushort.
        public static char ReadChar(this Reader reader) => (char)reader.ReadBlittable<ushort>();
        public static char? ReadCharNullable(this Reader reader) => (char?)reader.ReadBlittableNullable<ushort>();

        // bool is not blittable. read as byte.
        public static bool ReadBool(this Reader reader) => reader.ReadBlittable<byte>() != 0;
        public static bool? ReadBoolNullable(this Reader reader)
        {
            byte? value = reader.ReadBlittableNullable<byte>();
            return value.HasValue ? (value.Value != 0) : default(bool?);
        }

        public static short ReadShort(this Reader reader) => reader.ReadBlittable<short>();
        public static short? ReadShortNullable(this Reader reader) => reader.ReadBlittableNullable<short>();

        public static ushort ReadUShort(this Reader reader) => reader.ReadBlittable<ushort>();
        public static ushort? ReadUShortNullable(this Reader reader) => reader.ReadBlittableNullable<ushort>();

        public static int ReadInt(this Reader reader) => reader.ReadBlittable<int>();
        public static int? ReadIntNullable(this Reader reader) => reader.ReadBlittableNullable<int>();

        public static uint ReadUInt(this Reader reader) => reader.ReadBlittable<uint>();
        public static uint? ReadUIntNullable(this Reader reader) => reader.ReadBlittableNullable<uint>();

        public static long ReadLong(this Reader reader) => reader.ReadBlittable<long>();
        public static long? ReadLongNullable(this Reader reader) => reader.ReadBlittableNullable<long>();

        public static ulong ReadULong(this Reader reader) => reader.ReadBlittable<ulong>();
        public static ulong? ReadULongNullable(this Reader reader) => reader.ReadBlittableNullable<ulong>();

        public static float ReadFloat(this Reader reader) => reader.ReadBlittable<float>();
        public static float? ReadFloatNullable(this Reader reader) => reader.ReadBlittableNullable<float>();

        public static double ReadDouble(this Reader reader) => reader.ReadBlittable<double>();
        public static double? ReadDoubleNullable(this Reader reader) => reader.ReadBlittableNullable<double>();

        public static decimal ReadDecimal(this Reader reader) => reader.ReadBlittable<decimal>();
        public static decimal? ReadDecimalNullable(this Reader reader) => reader.ReadBlittableNullable<decimal>();

        /// <exception cref="T:System.ArgumentException">if an invalid utf8 string is sent</exception>
        public static string ReadString(this Reader reader)
        {
            // read number of bytes
            ushort size = reader.ReadUShort();

            // null support, see NetworkWriter
            if (size == 0)
                return null;

            ushort realSize = (ushort)(size - 1);

            // make sure it's within limits to avoid allocation attacks etc.
            if (realSize > Writer.MaxStringLength)
                throw new EndOfStreamException($"Reader.ReadString - Value too long: {realSize} bytes. Limit is: {Writer.MaxStringLength} bytes");

            ArraySegment<byte> data = reader.ReadBytesSegment(realSize);

            // convert directly from buffer to string via encoding
            // throws in case of invalid utf8.
            // see test: ReadString_InvalidUTF8()
            return reader.encoding.GetString(data.Array, data.Offset, data.Count);
        }

        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static byte[] ReadBytesAndSize(this Reader reader)
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? null : reader.ReadBytes(checked((int)(count - 1u)));
        }

        public static byte[] ReadBytes(this Reader reader, int count)
        {
            if (count > Reader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"Reader attempted to allocate {count} bytes, which is larger than the allowed limit of {Reader.AllocationLimit} bytes.");
            }

            byte[] bytes = new byte[count];
            reader.ReadBytes(bytes, count);
            return bytes;
        }

        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static ArraySegment<byte> ReadBytesAndSizeSegment(this Reader reader)
        {
            // count = 0 means the array was null
            // otherwise count - 1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? default : reader.ReadBytesSegment(checked((int)(count - 1u)));
        }

        public static List<T> ReadList<T>(this Reader reader)
        {
            int length = reader.ReadInt();

            // 'null' is encoded as '-1'
            if (length < 0) return null;

            if (length > Reader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"Reader attempted to allocate a List<{typeof(T)}> {length} elements, which is larger than the allowed limit of {Reader.AllocationLimit}.");
            }

            List<T> result = new List<T>(length);
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }

        public static T[] ReadArray<T>(this Reader reader)
        {
            int length = reader.ReadInt();

            // 'null' is encoded as '-1'
            if (length < 0) return null;

            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            if (length > Reader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"Reader attempted to allocate an Array<{typeof(T)}> with {length} elements, which is larger than the allowed limit of {Reader.AllocationLimit}.");
            }

            // we can't check if reader.Remaining < length,
            // because we don't know sizeof(T) since it's a managed type.
            // if (length > reader.Remaining) throw new EndOfStreamException($"Received array that is too large: {length}");

            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.Read<T>();
            }
            return result;
        }

        public static Uri ReadUri(this Reader reader)
        {
            string uriString = reader.ReadString();
            return (string.IsNullOrWhiteSpace(uriString) ? null : new Uri(uriString));
        }

        public static DateTime ReadDateTime(this Reader reader) => DateTime.FromOADate(reader.ReadDouble());
        public static DateTime? ReadDateTimeNullable(this Reader reader) => reader.ReadBool() ? ReadDateTime(reader) : default(DateTime?);
#if OnUnity
        public static Vector2 ReadVector2(this Reader reader) => reader.ReadBlittable<Vector2>();
        public static Vector2? ReadVector2Nullable(this Reader reader) => reader.ReadBlittableNullable<Vector2>();

        public static Vector3 ReadVector3(this Reader reader) => reader.ReadBlittable<Vector3>();
        public static Vector3? ReadVector3Nullable(this Reader reader) => reader.ReadBlittableNullable<Vector3>();

        public static Vector4 ReadVector4(this Reader reader) => reader.ReadBlittable<Vector4>();
        public static Vector4? ReadVector4Nullable(this Reader reader) => reader.ReadBlittableNullable<Vector4>();

        public static Vector2Int ReadVector2Int(this Reader reader) => reader.ReadBlittable<Vector2Int>();
        public static Vector2Int? ReadVector2IntNullable(this Reader reader) => reader.ReadBlittableNullable<Vector2Int>();

        public static Vector3Int ReadVector3Int(this Reader reader) => reader.ReadBlittable<Vector3Int>();
        public static Vector3Int? ReadVector3IntNullable(this Reader reader) => reader.ReadBlittableNullable<Vector3Int>();

        public static Color ReadColor(this Reader reader) => reader.ReadBlittable<Color>();
        public static Color? ReadColorNullable(this Reader reader) => reader.ReadBlittableNullable<Color>();

        public static Color32 ReadColor32(this Reader reader) => reader.ReadBlittable<Color32>();
        public static Color32? ReadColor32Nullable(this Reader reader) => reader.ReadBlittableNullable<Color32>();

        public static Quaternion ReadQuaternion(this Reader reader) => reader.ReadBlittable<Quaternion>();
        public static Quaternion? ReadQuaternionNullable(this Reader reader) => reader.ReadBlittableNullable<Quaternion>();

        // Rect is a struct with properties instead of fields
        public static Rect ReadRect(this Reader reader) => new Rect(reader.ReadVector2(), reader.ReadVector2());
        public static Rect? ReadRectNullable(this Reader reader) => reader.ReadBool() ? ReadRect(reader) : default(Rect?);

        // Plane is a struct with properties instead of fields
        public static Plane ReadPlane(this Reader reader) => new Plane(reader.ReadVector3(), reader.ReadFloat());
        public static Plane? ReadPlaneNullable(this Reader reader) => reader.ReadBool() ? ReadPlane(reader) : default(Plane?);

        // Ray is a struct with properties instead of fields
        public static Ray ReadRay(this Reader reader) => new Ray(reader.ReadVector3(), reader.ReadVector3());
        public static Ray? ReadRayNullable(this Reader reader) => reader.ReadBool() ? ReadRay(reader) : default(Ray?);

        public static Matrix4x4 ReadMatrix4x4(this Reader reader) => reader.ReadBlittable<Matrix4x4>();
        public static Matrix4x4? ReadMatrix4x4Nullable(this Reader reader) => reader.ReadBlittableNullable<Matrix4x4>();

        public static Guid ReadGuid(this Reader reader)
        {
#if !UNITY_2021_3_OR_NEWER
            // Unity 2019 doesn't have Span yet
            return new Guid(reader.ReadBytes(16));
#else
            // ReadBlittable(Guid) isn't safe. see ReadBlittable comments.
            // Guid is Sequential, but we can't guarantee packing.
            if (reader.Remaining >= 16)
            {
                ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(reader.buffer.Array, reader.buffer.Offset + reader.Position, 16);
                reader.Position += 16;
                return new Guid(span);
            }
            throw new EndOfStreamException($"ReadGuid out of range: {reader}");
#endif
        }
        public static Guid? ReadGuidNullable(this Reader reader) => reader.ReadBool() ? ReadGuid(reader) : default(Guid?);



        // while SyncSet<T> is recommended for NetworkBehaviours,
        // structs may have .Set<T> members which weaver needs to be able to
        // fully serialize for NetworkMessages etc.
        // note that Weaver/Readers/GenerateReader() handles this manually.
        // TODO writer not found. need to adjust weaver first. see tests.
        /*
        public static HashSet<T> ReadHashSet<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();
            if (length < 0)
                return null;
            HashSet<T> result = new HashSet<T>();
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }
        */


        public static Texture2D ReadTexture2D(this Reader reader)
        {
            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            short width = reader.ReadShort();
            if (width == -1) return null;

            // read height
            short height = reader.ReadShort();

            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            // log an error and return default.
            // we don't want attackers to be able to trigger exceptions.
            int totalSize = width * height;
            if (totalSize > Reader.AllocationLimit)
            {
                Debug.LogWarning($"Reader attempted to allocate a Texture2D with total size (width * height) of {totalSize}, which is larger than the allowed limit of {Reader.AllocationLimit}.");
                return null;
            }

            Texture2D texture2D = new Texture2D(width, height);

            // read pixel content
            Color32[] pixels = reader.ReadArray<Color32>();
            texture2D.SetPixels32(pixels);
            texture2D.Apply();
            return texture2D;
        }

        public static Sprite ReadSprite(this Reader reader)
        {
            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            Texture2D texture = reader.ReadTexture2D();
            if (texture == null) return null;

            // otherwise create a valid sprite
            return Sprite.Create(texture, reader.ReadRect(), reader.ReadVector2());
        }

#endif
    }
}