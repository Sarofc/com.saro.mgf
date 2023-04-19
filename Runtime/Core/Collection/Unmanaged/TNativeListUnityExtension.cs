#if UNITY_5_3_OR_NEWER
#if !UNITY_2019_3_OR_NEWER
#error supports Unity 2019_3 and above only
#else
// Unity.Collections.LowLevel.Unsafe.UnsafeUtility is not part of Unity Collection but it comes with Unity
#define UNITY_COLLECTIONS
#endif
#endif

#if UNITY_COLLECTIONS
namespace Saro.Collections
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public static class TNativeListUnityExtension
    {
        public unsafe static NativeArray<T> ToNativeArray<T>(this TNativeList<T> list) where T : unmanaged
        {
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                (void*)list.ToIntPtr(), (int)list.Count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
            return nativeArray;
        }
    }
}
#endif
