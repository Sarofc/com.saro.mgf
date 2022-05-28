using Saro.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Saro.Collections
{
    public struct NativeDynamicArrayCast<T> : IDisposable where T : struct
    {
        public bool IsValid => m_Array.IsValid;

        private NativeDynamicArray m_Array;

        public NativeDynamicArrayCast(uint size, EAllocator allocator = EAllocator.Persistent)
        {
            m_Array = NativeDynamicArray.Alloc<T>(allocator, size);
        }
        public NativeDynamicArrayCast(NativeDynamicArray array) : this() { m_Array = array; }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Array.Count<T>();
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Array.Capacity<T>();
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref m_Array.Get<T>((uint)index);
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref m_Array.Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T id) { m_Array.Add(id); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnorderedRemoveAt(uint index) { m_Array.UnorderedRemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index) { m_Array.RemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { m_Array.FastClear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { m_Array.Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddAt(uint lastIndex) { return ref m_Array.AddAt<T>(lastIndex); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize) { m_Array.Resize<T>(newSize); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDynamicArray ToNativeArray() { return m_Array; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, in T value)
        {
            m_Array.Set(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, in T value)
        {
            m_Array.Set((uint)index, value);
        }
    }
}