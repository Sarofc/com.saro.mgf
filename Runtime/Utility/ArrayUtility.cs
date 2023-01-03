using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Saro.Core;
using System.Security.Cryptography;

namespace Saro.Utility
{
    /// <summary>
    /// Array.Sort 存在 new Comparison(Comparer<T>.Compare) 的额外内存分配。
    /// 此类，直接使用 Comparer<T> 泛型类，不使用委托
    /// </summary>
    public static partial class ArrayUtility
    {
        public static void Sort<T>(T[] keys) where T : IComparable<T>
        {
            ArraySortHelper<T>.IntrospectiveSort(keys, 0, keys.Length);
        }

        public static void Sort<T>(T[] keys, int left, int length) where T : IComparable<T>
        {
            ArraySortHelper<T>.IntrospectiveSort(keys, left, length);
        }

        public static void Sort<T>(T[] keys, int left, int length, IComparer<T> comparer) where T : IComparable<T>
        {
            ArraySortHelper<T>.IntrospectiveSort(keys, left, length, comparer);
        }
    }

    partial class ArrayUtility
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

        /// <summary>
        /// 缩短array，移除 <see cref="predicate"/> 返回true 的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="predicate"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns>移除元素的个数</returns>
        public static int ShrinkFast<T>(T[] array, Predicate<T> predicate, int startIndex, int length) where T : unmanaged
        {
            if (length > array.Length - startIndex)
                throw new ArgumentOutOfRangeException($"{nameof(array)} < {length}");

            var arrayLength = startIndex + length - 1;

            int remaindCount = 0;

            var slowIndex = startIndex - 1;
            int midIndex;
            int fastIndex;

            while (slowIndex < arrayLength)
            {
                if (predicate(array[++slowIndex]))
                {
                    break;
                }
                else
                {
                    remaindCount++;
                }
            }

            midIndex = slowIndex;

            while (midIndex < arrayLength)
            {
                while (midIndex < arrayLength)
                {
                    if (!predicate(array[++midIndex]))
                    {
                        remaindCount++;
                        break;
                    }
                }

                fastIndex = midIndex;
                while (fastIndex < arrayLength)
                {
                    if (predicate(array[++fastIndex]))
                    {
                        break;
                    }
                    else
                    {
                        remaindCount++;
                    }
                }

                var _length = fastIndex - midIndex;
                if (_length > 0)
                {
                    ArrayUtility.CopyFast(array, midIndex, array, slowIndex, _length);

                    slowIndex += _length;
                    midIndex = fastIndex - 1;
                }
            }
            return length - remaindCount;
        }
    }

    internal static class ArraySortHelper<T> where T : IComparable<T>
    {
        internal static void IntrospectiveSort(Span<T> keys, int left, int length)
        {
            IntrospectiveSort(keys, left, length, Comparer<T>.Default);
        }

        internal static void IntrospectiveSort(Span<T> keys, int left, int length, IComparer<T> comparer)
        {
            if (length < 2)
            {
                return;
            }
            IntroSort(keys, left, length + left - 1, 2 * FloorLog2(keys.Length), comparer);
        }

        private static int FloorLog2(int n)
        {
            int num = 0;
            while (n >= 1)
            {
                num++;
                n /= 2;
            }
            return num;
        }

        private static void IntroSort(Span<T> keys, int lo, int hi, int depthLimit, IComparer<T> comparer)
        {
            while (hi > lo)
            {
                int num = hi - lo + 1;
                if (num <= 16)
                {
                    if (num == 1)
                    {
                        return;
                    }
                    if (num == 2)
                    {
                        SwapIfGreater(keys, comparer, lo, hi);
                        return;
                    }
                    if (num == 3)
                    {
                        SwapIfGreater(keys, comparer, lo, hi - 1);
                        SwapIfGreater(keys, comparer, lo, hi);
                        SwapIfGreater(keys, comparer, hi - 1, hi);
                        return;
                    }
                    InsertionSort(keys, lo, hi, comparer);
                    return;
                }
                else
                {
                    if (depthLimit == 0)
                    {
                        HeapSort(keys, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;
                    int num2 = PickPivotAndPartition(keys, lo, hi, comparer);
                    IntroSort(keys, num2 + 1, hi, depthLimit, comparer);
                    hi = num2 - 1;
                }
            }
        }

        private static int PickPivotAndPartition(Span<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int num = lo + (hi - lo) / 2;
            SwapIfGreater(keys, comparer, lo, num);
            SwapIfGreater(keys, comparer, lo, hi);
            SwapIfGreater(keys, comparer, num, hi);
            T t = keys[num];
            Swap(keys, num, hi - 1);
            int i = lo;
            int num2 = hi - 1;
            while (i < num2)
            {
                while (comparer.Compare(keys[++i], t) < 0) { }
                while (comparer.Compare(t, keys[--num2]) < 0) { }
                if (i >= num2)
                {
                    break;
                }
                Swap(keys, i, num2);
            }
            Swap(keys, i, hi - 1);
            return i;
        }

        internal static void HeapSort(Span<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int num = hi - lo + 1;
            for (int i = num / 2; i >= 1; i--)
            {
                DownHeap(keys, i, num, lo, comparer);
            }
            for (int j = num; j > 1; j--)
            {
                Swap(keys, lo, lo + j - 1);
                DownHeap(keys, 1, j - 1, lo, comparer);
            }
        }

        private static void DownHeap(Span<T> keys, int i, int n, int lo, IComparer<T> comparer)
        {
            T t = keys[lo + i - 1];
            while (i <= n / 2)
            {
                int num = 2 * i;
                if (num < n && comparer.Compare(keys[lo + num - 1], keys[lo + num]) < 0)
                {
                    num++;
                }
                if (comparer.Compare(t, keys[lo + num - 1]) >= 0)
                {
                    break;
                }
                keys[lo + i - 1] = keys[lo + num - 1];
                i = num;
            }
            keys[lo + i - 1] = t;
        }

        internal static void InsertionSort(Span<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            for (int i = lo; i < hi; i++)
            {
                var t = keys[i + 1];
                var num = i;
                while (num >= lo && comparer.Compare(t, keys[num]) < 0)
                {
                    keys[num + 1] = keys[num];
                    num--;
                }
                keys[num + 1] = t;
            }
        }

        internal static void Swap(Span<T> a, int i, int j)
        {
            if (i != j)
            {
                var t = a[i];
                a[i] = a[j];
                a[j] = t;
            }
        }

        internal static void SwapIfGreater(Span<T> keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
            {
                var t = keys[a];
                keys[a] = keys[b];
                keys[b] = t;
            }
        }
    }
}
