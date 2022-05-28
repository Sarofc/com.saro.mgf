#if DEBUG
#define ENABLE_DEBUG_CHECKS
#endif

using Saro.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Saro.Collections
{
    /*
     * from Svelto.ECS
     */
    public struct NativeDynamicArray : IDisposable
    {
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return m_Array != null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int Count<T>() where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception($"DynamicArray: not expected type used");
#endif
            return (m_Array->Count / MemoryUtility.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int SizeInBytes()
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
#endif
            return (m_Array->Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int Capacity<T>() where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif
            return (m_Array->Capacity / MemoryUtility.SizeOf<T>());
        }

        public static NativeDynamicArray Alloc<T>(uint newLength = 0) where T : struct
        {
            return Alloc<T>(EAllocator.Persistent, newLength);
        }

        public unsafe static NativeDynamicArray Alloc<T>(EAllocator allocator, uint newLength = 0) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            var rtnStruc = new NativeDynamicArray
            {
                m_HashType = TypeHash<T>.hash,
            };
#else
            NativeDynamicArray rtnStruc = default;
#endif
            UnsafeArray* listData = (UnsafeArray*)MemoryUtility.Alloc<UnsafeArray>(1, allocator);

            //clear to nullify the pointers
            //MemoryUtility.MemClear((IntPtr) listData, structSize);

            rtnStruc.m_Allocator = allocator;
            listData->Realloc<T>(newLength, allocator);

            rtnStruc.m_Array = listData;

            return rtnStruc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T Get<T>(uint index) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (index >= Count<T>())
                throw new Exception($"DynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                return ref m_Array->Get<T>(index);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index) where T : struct
        {
            return ref Get<T>((uint)index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Set<T>(uint index, in T value) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (index >= Capacity<T>())
                throw new Exception(
                    $"DynamicArray: out of bound access, index {index} capacity {Capacity<T>()}");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Set(index, value);

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public unsafe void Dispose()
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
                MemoryUtility.Free((IntPtr)m_Array, m_Allocator);

#if ENABLE_DEBUG_CHECKS
            }
#endif
            m_Array = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Add<T>(in T item) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                if (Count<T>() == Capacity<T>())
                {
                    m_Array->Realloc<T>((uint)((Capacity<T>() + 1) * 1.5f), m_Allocator);
                }

                m_Array->Add(item);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T AddAt<T>(uint index) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif
            var structSize = (uint)MemoryUtility.SizeOf<T>();

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                if (index >= Capacity<T>())
                    m_Array->Realloc<T>((uint)((index + 1) * 1.5f), m_Allocator);

                var writeIndex = (index + 1) * structSize;
                if (m_Array->Count < writeIndex)
                    m_Array->SetCountTo(writeIndex);

                return ref m_Array->Get<T>(index);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public unsafe void Resize<T>(uint newCapacity) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Realloc<T>((uint)newCapacity, m_Allocator);

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public unsafe void SetCount<T>(uint count) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif
            uint structSize = (uint)MemoryUtility.SizeOf<T>();
            uint size = (uint)(count * structSize);

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->SetCountTo((uint)size);

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddWithoutGrow<T>(in T item) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

            var structSize = (uint)MemoryUtility.SizeOf<T>();

            if (m_Array->Space - (int)structSize < 0)
                throw new Exception("DynamicArray: no writing authorized");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                m_Array->Add(item);

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void UnorderedRemoveAt<T>(uint index) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
            if (Count<T>() == 0)
                throw new Exception("DynamicArray: empty array invalid operation");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                var indexToMove = Count<T>() - 1;
                if (index < indexToMove)
                {
                    Set<T>(index, Get<T>((uint)indexToMove));
                }

                m_Array->Pop<T>();

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void FastClear()
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

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public unsafe T* ToPTR<T>() where T : unmanaged
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            return (T*)m_Array->Ptr;
        }

        public unsafe IntPtr ToIntPTR<T>() where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            return (IntPtr)m_Array->Ptr;
        }

        public unsafe T[] ToManagedArray<T>() where T : unmanaged
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");

#endif
            var count = Count<T>();
            var ret = new T[count];
            var lengthToCopyInBytes = count * MemoryUtility.SizeOf<T>();

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

        public unsafe T[] ToManagedArrayUntrimmed<T>() where T : unmanaged
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif
            var capacity = Capacity<T>();
            var ret = new T[capacity];

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                fixed (void* handle = ret)
                {
                    MemoryUtility.MemCpy<T>((IntPtr)m_Array->Ptr, 0, (IntPtr)handle, 0, (uint)capacity);
                }

#if ENABLE_DEBUG_CHECKS
            }
#endif

            return ret;
        }

        public unsafe void RemoveAt<T>(uint index) where T : struct
        {
#if ENABLE_DEBUG_CHECKS
            if (m_Array == null)
                throw new Exception("DynamicArray: null-access");
            if (m_HashType != TypeHash<T>.hash)
                throw new Exception("DynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                MemoryUtility.MemMove<T>((IntPtr)m_Array->Ptr, index + 1, index, (uint)(Count<T>() - (index + 1)));

                m_Array->Pop<T>();

#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

        public unsafe void MemClear()
        {
#if ENABLE_DEBUG_CHECKS
            using (m_ThreadSentinel.TestThreadSafety())
            {
#endif
                MemoryUtility.MemClear((IntPtr)m_Array->Ptr, (uint)m_Array->Capacity);
#if ENABLE_DEBUG_CHECKS
            }
#endif
        }

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* m_Array;
#if DEBUG && !PROFILE_SVELTO
        int m_HashType;
#endif

#if ENABLE_DEBUG_CHECKS
        Sentinel m_ThreadSentinel;
#endif

        EAllocator m_Allocator;
    }
}
