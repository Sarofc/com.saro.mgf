using System.Collections.Generic;
using Random = System.Random;

namespace Saro.Utility
{
    /// <summary>
    /// <code>thread unsafe</code>
    /// </summary>
    [System.Obsolete("use 'GRandom' instead", true)]
    public static class RandomUtility
    {
        private static Random s_Rnd = new((int)System.DateTime.UtcNow.Ticks);

        public static void InitSeed(int seed)
        {
            s_Rnd = new Random(seed);
        }

        public static int Next(int min, int max)
        {
            return s_Rnd.Next(min, max);
        }

        public static double NextDouble()
        {
            return s_Rnd.NextDouble();
        }

        public static void NextBytes(byte[] buffer)
        {
            s_Rnd.NextBytes(buffer);
        }

        public static void Shuffle<T>(IList<T> array, int start, int count)
        {
            if (start < 0 || array.Count <= start || array.Count < start + count)
                throw new System.ArgumentOutOfRangeException();

            for (int i = start; i < start + count; i++)
            {
                Swap(array, i, s_Rnd.Next(i, array.Count));
            }
        }

        public static void Shuffle<T>(T[] array, int start, int count)
        {
            if (start < 0 || array.Length <= start || array.Length < start + count)
                throw new System.ArgumentOutOfRangeException();

            for (int i = start; i < start + count; i++)
            {
                Swap(ref array[i], ref array[s_Rnd.Next(i, array.Length)]);
            }
        }

        public static void Shuffle<T>(T[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                Swap(ref array[i], ref array[s_Rnd.Next(i, array.Length)]);
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                Swap(list, i, s_Rnd.Next(i, list.Count));
            }
        }

        public static void Swap<T>(IList<T> list, int idx, int idx1)
        {
            (list[idx], list[idx1]) = (list[idx1], list[idx]);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
    }
}