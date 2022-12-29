#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Saro
{
    // TODO 需不需要线程安全？

    public static partial class GRandom
    {
        private static Random s_Random = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitState(uint seed) => s_Random.InitState(seed);
        /// <summary>Returns a uniformly random float value in the interval [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single NextFloat() => s_Random.NextFloat();
        /// <summary>Returns a uniformly random float value in the interval [min, max).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single NextFloat(Single min, Single max) => s_Random.NextFloat(min, max);
        public static float2 NextFloat2(Single min, Single max) => s_Random.NextFloat2(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 NextFloat3(Single min, Single max) => s_Random.NextFloat3(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 NextFloat4(Single min, Single max) => s_Random.NextFloat4(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 NextFloat2Direction() => s_Random.NextFloat2Direction();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 NextFloat3Direction() => s_Random.NextFloat3Direction();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextInt(int min, int max) => s_Random.NextInt(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 NextFloat4(int2 min, int2 max) => s_Random.NextInt2(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 NextFloat4(int3 min, int3 max) => s_Random.NextInt3(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 NextFloat4(int4 min, int4 max) => s_Random.NextInt4(min, max);
    }

    partial class GRandom
    {
        public static void Shuffle<T>(IList<T> array, int start, int count)
        {
            if (start < 0 || array.Count <= start || array.Count < start + count)
                throw new System.ArgumentOutOfRangeException();

            for (int i = start; i < start + count; i++)
                Swap(array, i, s_Random.NextInt(i, array.Count));
        }

        public static void Shuffle<T>(T[] array, int start, int count)
        {
            if (start < 0 || array.Length <= start || array.Length < start + count)
                throw new System.ArgumentOutOfRangeException();

            for (int i = start; i < start + count; i++)
                Swap(ref array[i], ref array[s_Random.NextInt(i, array.Length)]);
        }

        public static void Shuffle<T>(T[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
                Swap(ref array[i], ref array[s_Random.NextInt(i, array.Length)]);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
                Swap(list, i, s_Random.NextInt(i, list.Count));
        }

        public static void Swap<T>(IList<T> list, int idx, int idx1) => (list[idx], list[idx1]) = (list[idx1], list[idx]);

        public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);
    }
}
