using System.IO;
using System;

namespace Saro.IO
{
    public static class BinaryReaderWriterExtension
    {
        //[ThreadStatic]
        static byte[] s_buffer = new byte[2048]; // TODO 大小多少合适

        public unsafe static void WriteUnmanaged<T>(this BinaryWriter writer, in T obj) where T : unmanaged
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
            //Span<byte> buffer = stackalloc byte[size]; // 大小限制
            Span<byte> buffer = new(s_buffer, 0, size);
            var count = reader.Read(buffer);
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
            var size = sizeof(T) * arrayLength;
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
            return arrayLength;
        }
    }
}
