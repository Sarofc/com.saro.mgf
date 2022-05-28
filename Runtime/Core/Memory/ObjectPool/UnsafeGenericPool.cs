//namespace Saro.Pool
//{
//    /// <summary>
//    ///   <para>Provides a static implementation of Pool.ObjectPool_1.</para>
//    /// </summary>
//    public static class UnsafeGenericPool<T> where T : class, new()
//    {
//        public static T Get()
//        {
//            return UnsafeGenericPool<T>.s_Pool.Get();
//        }

//        public static PooledObject<T> Get(out T value)
//        {
//            return UnsafeGenericPool<T>.s_Pool.Get(out value);
//        }

//        public static void Release(T toRelease)
//        {
//            UnsafeGenericPool<T>.s_Pool.Release(toRelease);
//        }

//        internal static readonly ObjectPool<T> s_Pool = new ObjectPool<T>(() => new T(), null, null, null, false, 10, 10000);
//    }
//}
