using System;
using System.Collections.Generic;

namespace Saro.Utility
{
    /// <summary>
    /// Array.Sort 存在 new Comparison(Comparer<T>.Compare) 的额外内存分配。
    /// 此类，直接使用 Comparer<T> 泛型类，不使用委托
    /// </summary>
    public static class ArrayUtility
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
