//#define UNITY_COLLECTIONS

using Saro.Utility;
using System;
using System.Runtime.CompilerServices;


namespace Saro.Collections
{
    struct UnsafeArray
    {
        internal unsafe byte* Ptr => m_Ptr;

        //expressed in bytes
        internal int Capacity => (int)m_Capacity;

        //expressed in bytes
        internal int Count => (int)m_WriteIndex;

        //expressed in bytes
        internal int Space => Capacity - Count;

#if DEBUG && !PROFILE_SVELTO        
#pragma warning disable 649
        internal uint id;
#pragma warning restore 649
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO              
                uint sizeOf = (uint)MemoryUtility.SizeOf<T>();
                if (index + sizeOf > m_WriteIndex)
                    throw new Exception("no reading authorized");
#endif                
                return ref Unsafe.AsRef<T>(Unsafe.Add<T>(m_Ptr, (int)index));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : struct
        {
            unsafe
            {
                uint sizeOf = (uint)MemoryUtility.SizeOf<T>();
                uint writeIndex = (uint)(index * sizeOf);

#if DEBUG && !PROFILE_SVELTO
                if (m_Capacity < writeIndex + sizeOf)
                    throw new Exception("no writing authorized");
#endif
                Unsafe.AsRef<T>(Unsafe.Add<T>(m_Ptr, (int)index)) = value;

                if (m_WriteIndex < writeIndex + sizeOf)
                    m_WriteIndex = (uint)(writeIndex + sizeOf);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T value) where T : struct
        {
            unsafe
            {
                var structSize = MemoryUtility.SizeOf<T>();

#if DEBUG && !PROFILE_SVELTO
                if (Space - structSize < 0)
                    throw new Exception("no writing authorized");
#endif                
                Unsafe.Write(m_Ptr + m_WriteIndex, value);

                m_WriteIndex += (uint)structSize;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Pop<T>() where T : struct
        {
            unsafe
            {
                var structSize = MemoryUtility.SizeOf<T>();

                m_WriteIndex -= (uint)structSize;

                return ref Unsafe.AsRef<T>(m_Ptr + m_WriteIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc<T>(uint newCapacity, EAllocator allocator) where T : struct
        {
            unsafe
            {
                var structSize = (uint)MemoryUtility.SizeOf<T>();

                uint newCapacityInBytes = structSize * newCapacity;
                if (m_Ptr == null)
                    m_Ptr = (byte*)MemoryUtility.Alloc(newCapacityInBytes, allocator);
                else
                    m_Ptr = (byte*)MemoryUtility.Realloc((IntPtr)m_Ptr, newCapacityInBytes, allocator, (uint)Count);

                m_Capacity = newCapacityInBytes;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(EAllocator allocator)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (m_Ptr == null)
                    throw new Exception("UnsafeArray: try to dispose an already disposed array");
#endif
                MemoryUtility.Free((IntPtr)m_Ptr, allocator);

                m_Ptr = null;
                m_WriteIndex = 0;
                m_Capacity = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_WriteIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCountTo(uint count)
        {
            m_WriteIndex = count;
        }

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe byte* m_Ptr;

        uint m_WriteIndex;
        uint m_Capacity;
    }
}

#if UNITY_COLLECTIONS
namespace Saro.Collections
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    public static class NativeDynamicArrayUnityExtension
    {
        public unsafe static NativeArray<T> ToNativeArray<T>(this DynamicArray array) where T : struct
        {
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                (void*)array.ToIntPTR<T>(), (int)array.Count<T>(), Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
            return nativeArray;
        }
    }
}
#endif
