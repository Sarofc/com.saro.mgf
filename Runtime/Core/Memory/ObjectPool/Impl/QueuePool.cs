using System.Collections.Generic;

namespace Saro.Pool
{
    public class QueuePool<TItem>
    {
        public static Queue<TItem> Rent()
        {
            return QueuePool<TItem>.s_Pool.Rent();
        }

        public static PooledObject<Queue<TItem>> Rent(out Queue<TItem> value)
        {
            return QueuePool<TItem>.s_Pool.Rent(out value);
        }

        public static void Return(Queue<TItem> toRelease)
        {
            QueuePool<TItem>.s_Pool.Return(toRelease);
        }

        internal static readonly ObjectPool<Queue<TItem>> s_Pool = new ObjectPool<Queue<TItem>>(() => new Queue<TItem>(), null, (l) => l.Clear(), null, true, 10, 10000);
    }
}
