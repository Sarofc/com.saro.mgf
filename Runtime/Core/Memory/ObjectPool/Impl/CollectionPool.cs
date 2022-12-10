using System.Collections.Generic;

namespace Saro.Pool
{
    /// <summary>
    ///   <para>A Collection such as List, HashSet, Dictionary etc can be pooled and reused by using a CollectionPool.</para>
    ///   <code>thread unsafe</code>
    /// </summary>
    public class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
    {
        [System.Obsolete("Use Rent")]
        public static TCollection Get()
        {
            return CollectionPool<TCollection, TItem>.s_Pool.Rent();
        }

        [System.Obsolete("Use Rent")]
        public static PooledObject<TCollection> Get(out TCollection value)
        {
            return CollectionPool<TCollection, TItem>.s_Pool.Rent(out value);
        }

        public static TCollection Rent()
        {
            return CollectionPool<TCollection, TItem>.s_Pool.Rent();
        }


        public static PooledObject<TCollection> Rent(out TCollection value)
        {
            return CollectionPool<TCollection, TItem>.s_Pool.Rent(out value);
        }

        [System.Obsolete("Use Return")]
        public static void Release(TCollection toRelease)
        {
            CollectionPool<TCollection, TItem>.s_Pool.Return(toRelease);
        }

        public static void Return(TCollection toRelease)
        {
            CollectionPool<TCollection, TItem>.s_Pool.Return(toRelease);
        }

        internal static readonly ObjectPool<TCollection> s_Pool = new ObjectPool<TCollection>(() => new TCollection(), null, (l) => l.Clear(), null, true, 10, 10000);
    }
}
