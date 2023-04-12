using System;
using System.Runtime.CompilerServices;

namespace Saro.Utility
{
    public static partial class ArrayUtility
    {
        public unsafe static void ClearFast<T>(T[] src) where T : unmanaged
        {
            var srcSize = (uint)sizeof(T) * (uint)src.Length;
            if (srcSize == 0) return;

            fixed (T* pSrc = &src[0])
            {
                Unsafe.InitBlock(pSrc, 0, srcSize);
            }
        }

        public unsafe static void ClearFast<T>(T[] src, int startIndex, int length) where T : unmanaged
        {
            if (length > src.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            var srcSize = (uint)sizeof(T) * (uint)length;
            if (srcSize == 0) return;

            fixed (T* pSrc = &src[startIndex])
            {
                Unsafe.InitBlock(pSrc, 0, srcSize);
            }
        }

        public unsafe static void CopyFast<T>(T[] src, int srcStartIndex, T[] dst, int dstStartIndex, int length) where T : unmanaged
        {
            var size = (uint)sizeof(T) * (uint)length;
            if (size == 0) return;

            if (length > src.Length - srcStartIndex)
                throw new ArgumentOutOfRangeException($"{nameof(src)} {src.Length} < {length}");

            if (length > dst.Length - dstStartIndex)
                throw new ArgumentOutOfRangeException($"{nameof(dst)} {dst.Length} < {length}");

            fixed (T* pSrc = &src[srcStartIndex])
            fixed (T* pDst = &dst[dstStartIndex])
            {
                Buffer.MemoryCopy(pSrc, pDst, size, size);
            }
        }
    }
}
