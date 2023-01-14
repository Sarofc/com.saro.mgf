using System.IO;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Saro.IO
{
    public static class BinaryReaderWriterExtension
    {
        //[ThreadStatic]
        static byte[] s_buffer = new byte[2048]; // TODO 大小多少合适

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureBufferCapacity(int capacity)
        {
            if (s_buffer.Length < capacity)
                s_buffer = new byte[capacity];
        }

        public unsafe static void WriteUnmanaged<T>(this BinaryWriter writer, ref T obj) where T : unmanaged
        {
            var size = sizeof(T);
            fixed (T* ptr = &obj)
            {
                Span<byte> buffer = new((byte*)ptr, size);
                writer.Write(buffer);
            }
        }

        public unsafe static void ReadUnmanaged<T>(this BinaryReader reader, ref T obj) where T : unmanaged
        {
            var size = sizeof(T);

            EnsureBufferCapacity(size);

            //Span<byte> buffer = stackalloc byte[size]; // 大小限制
            //Span<byte> buffer = new(s_buffer, 0, size);
            //var count = reader.Read(buffer);
            var count = reader.Read(s_buffer, 0, size);
            Log.Assert(size == count, $"read bytes error: size != count: {size} != {count}");
            fixed (T* ptr = &obj)
            fixed (byte* pSrc = &s_buffer[0])
            {
                byte* pDst = (byte*)ptr;

                //UnsafeUtility.MemCpy(pSrc, pDst, count);
                Buffer.MemoryCopy(pSrc, pDst, size, size);
                //Unsafe.CopyBlock(, pDst, (uint)count);
            }
        }

        public static void WriteArrayUnmanaged<T>(this BinaryWriter writer, ref T[] array, int length) where T : unmanaged
        {
            if (length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            writer.Write(length);
            if (length > 0)
            {
                unsafe
                {
                    var size = sizeof(T) * length;
                    fixed (T* ptr = &array[0])
                    {
                        Span<byte> buffer = new((byte*)ptr, size);
                        writer.Write(buffer);
                    }
                }
            }
        }

        public unsafe static int ReadArrayUnmanaged<T>(this BinaryReader reader, ref T[] array) where T : unmanaged
        {
            var arrayLength = reader.ReadInt32();
            if (array.Length < arrayLength)
                array = new T[arrayLength];

            if (arrayLength > 0)
            {
                var size = sizeof(T) * arrayLength;

                EnsureBufferCapacity(size);

                //Span<byte> buffer = stackalloc byte[size]; // TODO 大小限制
                var count = reader.Read(s_buffer, 0, size);
                Log.Assert(size == count, $"read bytes error: size != count: {size} != {count}");
                fixed (T* ptr = &array[0])
                fixed (byte* pSrc = &s_buffer[0])
                {
                    byte* pDst = (byte*)ptr;

                    //UnsafeUtility.MemCpy(pSrc, pDst, count);
                    Buffer.MemoryCopy(pSrc, pDst, size, size);
                    //Unsafe.CopyBlock(, pDst, (uint)count);
                }
            }

            return arrayLength;
        }

        public static void WriteListUnmanaged<T>(this BinaryWriter writer, ref List<T> list) where T : unmanaged
        {
            writer.Write(list.Count);
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    writer.WriteUnmanaged(ref item);
                }
            }
        }

        public static void ReadListUnmanaged<T>(this BinaryReader reader, ref List<T> list) where T : unmanaged
        {
            var listCount = reader.ReadInt32();
            list.Clear();
            if (listCount > 0)
            {
                for (int i = 0; i < listCount; i++)
                {
                    T item = default;
                    reader.ReadUnmanaged(ref item);
                    list.Add(item);
                }
            }
        }
    }
}
