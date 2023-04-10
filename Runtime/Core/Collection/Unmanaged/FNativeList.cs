#if DEBUG
#define ENABLE_DEBUG_CHECKS
#endif

using Saro.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Saro.Collections
{
    // TODO 添加更多单元测试

    /*
     * from Svelto.ECS
     */
    public unsafe struct FNativeList<T> : IDisposable where T : unmanaged
    {
        internal const int k_DefaultCapacity = 4;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        FNativeArray<T>* m_Array;
#if DEBUG && !PROFILE_SVELTO
        int m_HashType;
#endif

#if ENABLE_DEBUG_CHECKS
        Sentinel m_ThreadSentinel;
#endif

        int m_Count;
        EAllocator m_Allocator;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Array != null;
        }

        public FNativeList(int capacity, EAllocator allocator = EAllocator.Persistent)
        {
#if ENABLE_DEBUG_CHECKS
            m_HashType = TypeCache<T>.hash;
#endif
            FNativeArray<T>* array = (FNativeArray<T>*)NativeUtility.Alloc<FNativeArray<T>>(1, allocator);

            //clear to nullify the pointers
            //MemoryUtility.MemClear((IntPtr) listData, structSize);

            m_Allocator = allocator;
            array->Realloc(capacity, allocator);

            m_Array = array;
            m_Count = 0;
        }

        //public FNativeList(FNativeList<T> list) : this() { m_Array = list.m_Array; }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ElementAt(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if ENABLE_DEBUG_CHECKS
                if (m_Array == null)
                    throw new Exception("DynamicArray: null-access");
                if (m_HashType != TypeCache<T>.hash)
                    throw new Exception("DynamicArray: not expected type used");
                if (index >= Count)
                    throw new Exception(
                        $"DynamicArray: out of bound access, index {index} count {Count}");
#endif

#if ENABLE_DEBUG_CHECKS
                using (m_ThreadSentinel.TestThreadSafety())
                {
#endif
                    m_Array->ElementAt(index) = value;

#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (m_Array == null)
                    throw new Exception("DynamicArray: null-access");
                if (m_HashType != TypeCache<T>.hash)
                    throw new Exception("DynamicArray: not expected type used");
#endif
                return m_Array->Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (index >= Count)
                throw new Exception($"DynamicArray: out of bound access, index {index} count {Count}");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                return ref m_Array->ElementAt(index);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                var capacity = Capacity;
                if (capacity == Count)
                    Grow(capacity + 1);

                (*m_Array)[m_Count++] = item;

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                if (index < 0 || index > Count)
                {
                    throw new IndexOutOfRangeException($"{nameof(Insert)}: {index}");
                }

                var capacity = Capacity;
                if (capacity == Count)
                    Grow(capacity + 1);

                NativeUtility.MemMove<T>((IntPtr)m_Array->Ptr, (uint)index, (uint)index + 1, (uint)(Count - index));

                (*m_Array)[index] = item;
                //m_Array->ElementAt(index) = item; // TODO 看看这俩有啥区别

                m_Count++;

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public void RemoveAt(int index)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                NativeUtility.MemMove<T>((IntPtr)m_Array->Ptr, (uint)index + 1, (uint)index, (uint)(Count - (index + 1)));

                m_Count--;

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtSwapBack(int index)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (Count == 0)
                throw new Exception("DynamicArray: empty array invalid operation");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                var indexToMove = Count - 1; // swap back
                if (index < indexToMove)
                {
                    (*m_Array)[index] = (*m_Array)[indexToMove];
                    //m_Array->ElementAt(index) = m_Array->ElementAt(indexToMove);
                }

                m_Count--;

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Clear();
                m_Count = 0;
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->FastClear();
                m_Count = 0;
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public void Resize(int newCapacity)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Realloc(newCapacity, m_Allocator);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => AsSpan(0, m_Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (m_Count < start)
                throw new ArgumentOutOfRangeException($"DynamicArray. start: {start}");
            if (m_Count < length)
                throw new ArgumentOutOfRangeException($"DynamicArray. length: {length}");
            if (length <= start)
                throw new ArgumentOutOfRangeException($"DynamicArray. start: {start} length: {length}");
#endif
            return m_Array->AsSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => AsReadOnlySpan(0, m_Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (m_Count < start)
                throw new ArgumentOutOfRangeException($"DynamicArray. start: {start}");
            if (m_Count < length)
                throw new ArgumentOutOfRangeException($"DynamicArray. length: {length}");
            if (length <= start)
                throw new ArgumentOutOfRangeException($"DynamicArray. start: {start} length: {length}");
#endif
            return m_Array->AsReadOnlySpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ToPointer()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            return (T*)m_Array->Ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToIntPtr()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            return (IntPtr)m_Array->Ptr;
        }

        public T[] ToManagedArray()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            var ret = new T[m_Count];
            var lengthToCopyInBytes = m_Count * Unsafe.SizeOf<T>();

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                fixed (void* handle = ret)
                {
                    Unsafe.CopyBlock(handle, m_Array->Ptr, (uint)lengthToCopyInBytes);
                }

                return ret;

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public T[] ToManagedArrayUntrimmed()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeCache<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif
            var capacity = Capacity;
            var ret = new T[capacity];

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                fixed (void* handle = ret)
                {
                    NativeUtility.MemCpy<T>((IntPtr)m_Array->Ptr, 0, (IntPtr)handle, 0, (uint)capacity);
                }

#if ENABLE_DEBUG_CHECKS
            }
#endif

            return ret;
        }

        public void Dispose()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Dispose(m_Allocator);
                NativeUtility.Free((IntPtr)m_Array, m_Allocator);

#if ENABLE_DEBUG_CHECKS
            }
#endif
            m_Array = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var length = Capacity;
            Log.Assert(length < capacity, "");

            int newCapacity = length == 0 ? k_DefaultCapacity : 2 * length;

            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            //if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

            // If the computed capacity is still less than specified, set to the original argument.
            // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
            if (newCapacity < capacity) newCapacity = capacity;

            Resize(newCapacity);
        }
    }
}
