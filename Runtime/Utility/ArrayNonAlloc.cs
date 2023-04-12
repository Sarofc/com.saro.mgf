using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Saro.Utility
{
    /// <summary>
    /// Array.Sort/List.Sort 存在 new Comparison(Comparer<T>.Compare) 的额外内存
    /// <code>仅限unity中使用，啥时候unity修复了，这个类就没用了</code>
    /// <code>.net 6 的 Array.Sort 性能爆杀，升级到最新的.net，unity将获得不小的性能提升</code>
    /// </summary>
    public static class ArrayNonAlloc
    {
        internal const int IntrosortSizeThreshold = 16;

        public static void Sort<T>(List<T> list)
        {
            var span = CollectionsMarshal.AsSpan(list);
            Sort(span.Slice(0, list.Count), Comparer<T>.Default);
        }

        public static void Sort<T>(List<T> list, Comparer<T> comparer)
        {
            var span = CollectionsMarshal.AsSpan(list);
            Sort(span.Slice(0, list.Count), comparer);
        }

        public static void Sort<T>(List<T> list, int index, int count)
        {
            Sort(list, index, count, Comparer<T>.Default);
        }

        public static void Sort<T>(List<T> list, int index, int count, Comparer<T> comparer)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("neg index");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("neg count");
            }

            if (list.Count - index < count)
                throw new ArgumentException("InvalidOffLen");

            if (count > 1)
            {
                var span = CollectionsMarshal.AsSpan(list);
                Sort(span.Slice(index, count), comparer);
            }
        }

        public static void Sort<T>(T[] keys, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("neg index");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("neg count");
            }

            if (keys.Length - index < count)
                throw new ArgumentException("InvalidOffLen");

            if (count > 1)
            {
                Sort(keys.AsSpan().Slice(index, count), Comparer<T>.Default);
            }
        }

        public static void Sort<T>(T[] keys, int index, int count, Comparer<T> comparer)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("neg index");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("neg count");
            }

            if (keys.Length - index < count)
                throw new ArgumentException("InvalidOffLen");

            if (count > 1)
            {
                Sort(keys.AsSpan().Slice(index, count), comparer);
            }
        }

        public static void Sort<T>(T[] keys)
        {
            ArraySortHelper<T>.Sort(keys, Comparer<T>.Default);
        }

        public static void Sort<T>(Span<T> keys)
        {
            ArraySortHelper<T>.Sort(keys, Comparer<T>.Default);
        }

        public static void Sort<T>(T[] keys, IComparer<T> comparer)
        {
            ArraySortHelper<T>.Sort(keys, comparer);
        }

        public static void Sort<T>(Span<T> keys, IComparer<T> comparer)
        {
            ArraySortHelper<T>.Sort(keys, comparer);
        }

        internal static int FloorLog2PlusOne(int n)
        {
            int num = 0;
            while (n >= 1)
            {
                num++;
                n /= 2;
            }
            return num;
        }
    }

    internal static partial class ArraySortHelper<T>
    {
        public static void Sort(Span<T> keys, IComparer<T>? comparer)
        {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try
            {
                IntrospectiveSort(keys, comparer ?? Comparer<T>.Default);
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException($"BadComparer: {comparer}");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"IComparerFailed: {e}");
            }
        }

        private static void IntrospectiveSort(Span<T> keys, IComparer<T> comparer)
        {
            if (keys.Length > 1)
            {
                IntroSort(keys, 2 * ArrayNonAlloc.FloorLog2PlusOne(keys.Length), comparer);
            }
        }

        private static void IntroSort(Span<T> keys, int depthLimit, IComparer<T> comparer)
        {
            Log.Assert(!keys.IsEmpty);
            Log.Assert(depthLimit >= 0);
            Log.Assert(comparer != null);

            int partitionSize = keys.Length;
            while (partitionSize > 1)
            {
                if (partitionSize <= ArrayNonAlloc.IntrosortSizeThreshold)
                {

                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        SwapIfGreater(keys, comparer, 0, 2);
                        SwapIfGreater(keys, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys[(p + 1)..partitionSize], depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static int PickPivotAndPartition(Span<T> keys, IComparer<T> comparer)
        {
            Log.Assert(keys.Length >= ArrayNonAlloc.IntrosortSizeThreshold);
            Log.Assert(comparer != null);

            int hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, 0, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer.Compare(keys[++left], pivot) < 0) ;
                while (comparer.Compare(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1)
            {
                Swap(keys, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(Span<T> keys, IComparer<T> comparer)
        {
            Log.Assert(comparer != null);
            Log.Assert(!keys.IsEmpty);

            int n = keys.Length;
            for (int i = n >> 1; i >= 1; i--)
            {
                DownHeap(keys, i, n, comparer);
            }

            for (int i = n; i > 1; i--)
            {
                Swap(keys, 0, i - 1);
                DownHeap(keys, 1, i - 1, comparer);
            }
        }

        private static void DownHeap(Span<T> keys, int i, int n, IComparer<T> comparer)
        {
            Log.Assert(comparer != null);

            T d = keys[i - 1];
            while (i <= n >> 1)
            {
                int child = 2 * i;
                if (child < n && comparer.Compare(keys[child - 1], keys[child]) < 0)
                {
                    child++;
                }

                if (!(comparer.Compare(d, keys[child - 1]) < 0))
                    break;

                keys[i - 1] = keys[child - 1];
                i = child;
            }

            keys[i - 1] = d;
        }

        private static void InsertionSort(Span<T> keys, IComparer<T> comparer)
        {
            for (int i = 0; i < keys.Length - 1; i++)
            {
                T t = keys[i + 1];

                int j = i;
                while (j >= 0 && comparer.Compare(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }

                keys[j + 1] = t;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapIfGreater(Span<T> keys, IComparer<T> comparer, int i, int j)
        {
            Log.Assert(i != j);

            if (comparer.Compare(keys[i], keys[j]) > 0)
            {
                T key = keys[i];
                keys[i] = keys[j];
                keys[j] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<T> a, int i, int j)
        {
            Log.Assert(i != j);

            T t = a[i];
            a[i] = a[j];
            a[j] = t;
        }
    }
}
