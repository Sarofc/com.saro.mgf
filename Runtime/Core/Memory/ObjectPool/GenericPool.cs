//namespace Saro.Pool
//{
//    /// <summary>
//    ///   <para>Provides a static implementation of Pool.ObjectPool_1.</para>
//    /// </summary>
//    public class GenericPool<T> where T : class, new()
//    {
//        public static T Get()
//        {
//            return GenericPool<T>.s_Pool.Get();
//        }

//        public static PooledObject<T> Get(out T value)
//        {
//            return GenericPool<T>.s_Pool.Get(out value);
//        }

//        public static void Release(T toRelease)
//        {
//            GenericPool<T>.s_Pool.Release(toRelease);
//        }

//        internal static readonly ObjectPool<T> s_Pool = new ObjectPool<T>(() => new T(), null, null, null, true, 10, 10000);
//    }
//}
