using System;
using System.Runtime.InteropServices;

namespace Saro.Utility
{
    internal static class TypeCache<T>
    {
#if !UNITY_BURST
        public static readonly int hash = typeof(T).GetHashCode();
#else
        public static readonly int hash = Unity.Burst.BurstRuntime.GetHashCode32<T>();
#endif
    }
}