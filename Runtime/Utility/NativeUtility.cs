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

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Saro.Utility
{
    public static partial class NativeUtility
    {
        static readonly ConcurrentDictionary<Type, bool> s_CachedTypes = new();

        public static bool IsUnmanaged<T>()
        {
#if !UNITY_BURST
            return !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#else
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.IsUnmanaged<T>();
#endif
        }

        public static bool IsUnmanaged(this Type t)
        {
            //UnsafeUtility.IsUnmanaged(t);

            if (s_CachedTypes.ContainsKey(t))
                return s_CachedTypes[t];

            var result = false;

            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else if (t.IsValueType)
                result = t.GetFields(BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Instance)
                          .All(x => IsUnmanaged(x.FieldType));

            s_CachedTypes.TryAdd(t, result);
            return result;
        }

        public unsafe static ref T NullRef<T>() // where T : unmanaged
        {
            //RuntimeHelpers.IsReferenceOrContainsReferences
            return ref Unsafe.AsRef<T>(null);
        }

        public unsafe static bool IsNullRef<T>(ref T source) // where T : unmanaged
        {
            return Unsafe.AsPointer(ref source) == null;
        }
    }

#if !UNITY_COLLECTIONS
    public enum EAllocator
    {
        Invalid,
        //no allocation needed
        None,
        //temporary allocation, it doesn't have any meaning outside unity atm
        Temp,
        //temporary allocation, it doesn't have any meaning outside unity atm
        TempJob,
        //persistent native allocation, must be disposed of
        Persistent,
        //managed allocation
        Managed
    }
#else
    public enum EAllocator
    {
        /// <summary>
        ///   <para>Invalid allocation.</para>
        /// </summary>
        Invalid = Unity.Collections.Allocator.Invalid,
        /// <summary>
        ///   <para>No allocation.</para>
        /// </summary>
        None = Unity.Collections.Allocator.None,
        /// <summary>
        ///   <para>Temporary allocation.</para>
        /// </summary>
        Temp = Unity.Collections.Allocator.Temp,
        /// <summary>
        ///   <para>Temporary job allocation.</para>
        /// </summary>
        TempJob = Unity.Collections.Allocator.TempJob,
        /// <summary>
        ///   <para>Persistent allocation.</para>
        /// </summary>
        Persistent = Unity.Collections.Allocator.Persistent,

        Managed
    }
#endif

    public static partial class NativeUtility // from Svelto.ECS
    {
        // TODO 非unity平台使用 NativeMemory 代替 Marshal.Alloc 等API

        public static IntPtr Alloc(uint newCapacityInBytes, EAllocator allocator, bool clear = true)
        {
            var signedCapacity = (int)SignedCapacity(newCapacityInBytes);
            IntPtr newPointer = IntPtr.Zero;
#if UNITY_COLLECTIONS
            var castedAllocator = (Unity.Collections.Allocator)allocator;
            unsafe
            {
                newPointer = (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(
                    signedCapacity, (int)OptimalAlignment.alignment, castedAllocator);
            }
#else
            newPointer = Marshal.AllocHGlobal(signedCapacity); //this is guaranteed to be aligned by design
#endif
            //Note MemClear is actually necessary
            if (clear && newCapacityInBytes > 0)
                MemClear(newPointer, (uint)signedCapacity);

            var signedPointer = SignedPointer(newCapacityInBytes, newPointer);

            CheckBoundaries(newPointer);

            return signedPointer;
        }

        public unsafe static IntPtr Realloc
        (IntPtr realBuffer, uint newCapacityInBytes, EAllocator allocator, uint numberOfElementsToCopyInBytes
       , bool copy = true, bool memClear = true)
        {
            //Alloc returns the correct Signed Pointer already
            //if copy == true, memclear is optimised, otherwise memclear if set to true
            var signedPointer = Alloc(newCapacityInBytes, allocator, copy == false && memClear == true);

            //Copy only the real data
            if (copy && numberOfElementsToCopyInBytes > 0)
            {
                if (newCapacityInBytes > numberOfElementsToCopyInBytes)
                {
                    Unsafe.CopyBlock((void*)signedPointer, (void*)realBuffer, numberOfElementsToCopyInBytes);
                    if (memClear)
                    {
                        var bytesToClear = newCapacityInBytes - numberOfElementsToCopyInBytes;
                        var startingBytePointerToClear = signedPointer + (int)numberOfElementsToCopyInBytes;
                        MemClear(startingBytePointerToClear, bytesToClear);
                    }
                }
                else
                    Unsafe.CopyBlock((void*)signedPointer, (void*)realBuffer, newCapacityInBytes);
            }

            //Free unsigns the pointer itself
            Free(realBuffer, allocator);
            return signedPointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc<T>(uint newCapacity, EAllocator allocator, bool clear = true) where T : struct
        {
            var newCapacityInBytes = (uint)(Unsafe.SizeOf<T>() * newCapacity);

            return Alloc(newCapacityInBytes, allocator, clear);
        }

        public static IntPtr Realloc<T>
        (IntPtr realBuffer, uint newCapacity, EAllocator allocator, uint numberOfElementsToCopy, bool copy = true
       , bool memClear = true) where T : struct
        {
            var sizeOf = Unsafe.SizeOf<T>();
            var newCapacityInBytes = (uint)(sizeOf * newCapacity);
            var numberOfElementsToCopyInBytes = (uint)(sizeOf * numberOfElementsToCopy);

            return Realloc(realBuffer, newCapacityInBytes, allocator, numberOfElementsToCopyInBytes, copy, memClear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Free(IntPtr ptr, EAllocator allocator)
        {
            ptr = CheckAndReturnPointerToFree(ptr);

#if UNITY_COLLECTIONS
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(
                (void*)ptr, (Unity.Collections.Allocator)allocator);
#else
            Marshal.FreeHGlobal(ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void MemClear<T>(IntPtr destination, uint size) where T : struct
        {
            var sizeOfInBytes = (uint)(Unsafe.SizeOf<T>() * size);
#if UNITY_COLLECTIONS
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear((void*)destination, sizeOfInBytes);
#else
            Unsafe.InitBlock((void*)destination, 0, sizeOfInBytes);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void MemClear(IntPtr destination, uint sizeOfInBytes)
        {
#if UNITY_COLLECTIONS
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear((void*)destination, sizeOfInBytes);
#else
            Unsafe.InitBlock((void*)destination, 0, sizeOfInBytes);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void MemSet(IntPtr destination, uint sizeOfInBytes, byte value)
        {
            Unsafe.InitBlock((void*)destination, value, sizeOfInBytes);
        }

        /// <summary>
        /// Like Memcpy but safe when memory overlaps
        /// </summary>
        public unsafe static void MemMove<T>(IntPtr source, uint sourceStartIndex, uint destinationStartIndex, uint count)
            where T : struct
        {
            var sizeOf = Unsafe.SizeOf<T>();
            var sizeOfInBytes = (uint)(sizeOf * count);
            //this uses System.Runtime.RuntimeImports::Memmove which is safe if memory overlaps
            /*
             *  public static void MemoryCopy(
                    void* source,
                    void* destination,
                    long destinationSizeInBytes,
                    long sourceBytesToCopy);
             */
            Buffer.MemoryCopy((void*)(source + (int)sourceStartIndex * sizeOf)
              , (void*)(source + (int)destinationStartIndex * sizeOf), sizeOfInBytes, sizeOfInBytes);
        }

        /// <summary>
        /// this is not safe if memory overlaps
        /// </summary>
        public unsafe static void MemCpy<T>(IntPtr source, uint sourceStartIndex, IntPtr destination, uint destinationStartIndex, uint count)
            where T : struct
        {
            var sizeOf = Unsafe.SizeOf<T>();
            var sizeOfInBytes = (uint)(sizeOf * count);
            //issues cpblk that assumes that both the source and destination addressed are aligned to the natural size of the machine.
            /*
             * public static unsafe void CopyBlock(void* destination, void* source, uint byteCount)
             * */
            Unsafe.CopyBlock(Unsafe.Add<T>((void*)destination, ((int)destinationStartIndex)),
                Unsafe.Add<T>((void*)source, ((int)sourceStartIndex)), sizeOfInBytes);
        }

        public static void StructureToBytes<T>(T structure, byte[] bytes, int startIndex, int length) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, bytes, startIndex, length);
            Marshal.FreeHGlobal(ptr);
        }

        public static T BytesToStructure<T>(byte[] bytes, int startIndex, int length)
        {
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.Copy(bytes, startIndex, ptr, length);
            T structure = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);

            return structure;
        }

#if UNITY_COLLECTIONS
        static class OptimalAlignment
        {
            internal static readonly uint alignment;

            static OptimalAlignment() { alignment = (uint)(Environment.Is64BitProcess ? 16 : 8); }
        }
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyStructureToPtr<T>(ref T buffer, IntPtr bufferPtr) where T : struct
        {
            Unsafe.Write((void*)bufferPtr, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T ArrayElementAsRef<T>(IntPtr data, int threadIndex) where T : struct
        {
            return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*)data, threadIndex));
        }

        public static int GetFieldOffset(FieldInfo field)
        {
#if UNITY_COLLECTIONS
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.GetFieldOffset(field);
#else
            int GetFieldOffset(RuntimeFieldHandle h)
            {
                return Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;
            }

            return GetFieldOffset(field.FieldHandle);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //todo unit test
        public static uint Align4(uint input) => (uint)((input + (4 - 1)) & ~(4 - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //todo unit test
        public static uint Pad4(uint input) => (uint)(-input & (4 - 1));

        static long SignedCapacity(uint newCapacity)
        {
#if DEBUG_MEMORY
            return newCapacity + 128;
#else
            return newCapacity;
#endif
        }

        unsafe static IntPtr SignedPointer(uint capacityWithoutSignature, IntPtr pointerToSign)
        {
#if DEBUG_MEMORY
            var value = 0xDEADBEEF;
            for (var i = 0; i < 60; i += 4)
            {
                Unsafe.Write((void*)pointerToSign, value); //4 bytes signature
                pointerToSign += 4;
            }

            Unsafe.Write((void*)pointerToSign, capacityWithoutSignature); //4 bytes size allocated
            pointerToSign += 4;

            for (var i = 0; i < 64; i += 4)
                Unsafe.Write((void*)(pointerToSign + (int)capacityWithoutSignature + i)
                            , value); //4 bytes size allocated

            return (IntPtr)(byte*)pointerToSign;
#else
            return (IntPtr)pointerToSign;
#endif
        }

        static IntPtr UnsignPointer(IntPtr ptr)
        {
#if DEBUG_MEMORY
            return ptr - 64;
#else
            return ptr;
#endif
        }

        static IntPtr CheckAndReturnPointerToFree(IntPtr ptr)
        {
            ptr = UnsignPointer(ptr);

            CheckBoundaries(ptr);
            return ptr;
        }

        [System.Diagnostics.Conditional("DEBUG_MEMORY")]
        static unsafe void CheckBoundaries(IntPtr ptr)
        {
#if DEBUG_MEMORY
            var debugPtr = ptr;

            for (var i = 0; i < 60; i += 4)
            {
                var u = Unsafe.Read<uint>((void*)debugPtr);
                if (u != 0xDEADBEEF)
                    throw new Exception("Memory Boundaries check failed!!!");

                debugPtr += 4;
            }

            var size = Unsafe.Read<uint>((void*)debugPtr);
            debugPtr = debugPtr + (int)(4 + size);

            for (var i = 0; i < 64; i += 4)
            {
                var u = Unsafe.Read<uint>((void*)(debugPtr + i));
                if (u != 0xDEADBEEF)
                    throw new Exception("Memory Boundaries check failed!!!");
            }
#endif
        }
    }
}
