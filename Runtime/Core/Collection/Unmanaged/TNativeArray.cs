#if DEBUG || DEVELOPMENT_BUILD
#define DEBUG_MEMORY
#endif

#if UNITY_5_3_OR_NEWER
#if !UNITY_2019_3_OR_NEWER
#error supports Unity 2019_3 and above only
#else
// Unity.Collections.LowLevel.Unsafe.UnsafeUtility is not part of Unity Collection but it comes with Unity
#define UNITY_COLLECTIONS
#endif
#endif

using Saro.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Saro.Collections
{
    internal unsafe struct TNativeArray<T> where T : unmanaged
    {
        internal byte* Ptr => m_Ptr;

        public int Length => m_Length;
        private int m_Length;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        byte* m_Ptr;

#if DEBUG || DEVELOPMENT_BUILD
#pragma warning disable 649
        internal uint id;
#pragma warning restore 649
#endif

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ElementAt(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG || DEVELOPMENT_BUILD
                if (index > m_Length || index < 0)
                    throw new IndexOutOfRangeException($"no writing authorized. index: {index} length: {m_Length}");
#endif
                Unsafe.AsRef<T>(Unsafe.Add<T>(m_Ptr, index)) = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (index > m_Length || index < 0)
                throw new IndexOutOfRangeException($"no reading authorized. index: {index} length: {m_Length}");
#endif
            return ref Unsafe.AsRef<T>(Unsafe.Add<T>(m_Ptr, index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc(int newLength, EAllocator allocator)
        {
            var structSize = (uint)Unsafe.SizeOf<T>();

            uint newCapacityInBytes = structSize * (uint)newLength;
            uint bytesCount = structSize * (uint)m_Length;

            if (m_Ptr == null)
                m_Ptr = (byte*)NativeUtility.Alloc(newCapacityInBytes, allocator);
            else
                m_Ptr = (byte*)NativeUtility.Realloc((IntPtr)m_Ptr, newCapacityInBytes, allocator, bytesCount);

            m_Length = newLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(EAllocator allocator)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (m_Ptr == null)
                throw new Exception("UnsafeArray: try to dispose an already disposed array");
#endif
            NativeUtility.Free((IntPtr)m_Ptr, allocator);

            m_Ptr = null;
            m_Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var structSize = (uint)Unsafe.SizeOf<T>();
            uint bytesCount = structSize * (uint)m_Length;
            NativeUtility.MemClear((IntPtr)m_Ptr, bytesCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            m_Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return AsSpan(0, m_Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
            return new Span<T>(Unsafe.Add<T>(m_Ptr, start), length * Unsafe.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return AsReadOnlySpan(0, m_Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
            return new ReadOnlySpan<T>(Unsafe.Add<T>(m_Ptr, start), length * Unsafe.SizeOf<T>());
        }
    }
}
